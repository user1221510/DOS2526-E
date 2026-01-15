pipeline {
    agent any

    environment {
        // --- CONFIGURAÇÃO GERAL ---
        IMAGE_NAME = "dos2526-api"
        // Gera data/hora para a tag da imagem
        DATA_HORA = sh(script: "date +%Y-%m-%d-%H%M", returnStdout: true).trim()
        
        // Define o Namespace: 'production' para a branch quality, 'staging' para as outras
        KubeNamespace = "${env.BRANCH_NAME == 'quality' ? 'production' : 'staging'}"
        ReleaseName = "products-api-${env.BRANCH_NAME == 'quality' ? 'prod' : 'dev'}"
    }

    stages {
        stage('Checkout') {
            steps { checkout scm }
        }

        stage('Configurar Variáveis') {
            steps {
                script {
                    env.TAG_FINAL = "${env.BRANCH_NAME}-${env.DATA_HORA}"
                    echo ">>> A preparar deploy para o Namespace: ${KubeNamespace}"
                }
            }
        }

        stage('Testes + Coverage') {
            steps {
                // Executa os testes unitários e gera relatório
                sh '''
                dotnet restore
                dotnet add ProductsAPI.Tests package JunitXml.TestLogger
                dotnet test ProductsAPI.Tests --logger "junit;LogFileName=test-results.xml"
                '''
            }
        }

        stage('Build & Push Docker') {
            steps {
                script {
                    // Usa a credencial do DockerHub para login e push
                    withCredentials([usernamePassword(credentialsId: 'dockerhub-token', usernameVariable: 'DOCKER_USER', passwordVariable: 'DOCKER_PASS')]) {
                        sh """
                        echo ">>> Login no Docker Hub..."
                        echo $DOCKER_PASS | docker login -u $DOCKER_USER --password-stdin
                        
                        echo ">>> Construindo imagem: $DOCKER_USER/${IMAGE_NAME}:${env.TAG_FINAL}"
                        docker build -t $DOCKER_USER/${IMAGE_NAME}:${env.TAG_FINAL} .
                        
                        echo ">>> Enviando para o Docker Hub..."
                        docker push $DOCKER_USER/${IMAGE_NAME}:${env.TAG_FINAL}
                        """
                    }
                }
            }
        }

        stage('Deploy no K8s (Helm)') {
            // Executa apenas na branch 'quality' ou branches 'dev_'
            when {
                expression { env.BRANCH_NAME == 'quality' || env.BRANCH_NAME.startsWith('dev_') }
            }
            steps {
                script {
                    // 1. Credenciais DockerHub (para configurar o Helm chart)
                    withCredentials([usernamePassword(credentialsId: 'dockerhub-token', usernameVariable: 'DOCKER_USER', passwordVariable: 'DOCKER_PASS')]) {
                        
                        // 2. Credenciais Kubernetes (Ficheiro Config Secreto)
                        withCredentials([file(credentialsId: 'kubernet-token', variable: 'KUBECONFIG_FILE')]) {
                            
                            echo ">>> Configuração K8s carregada. A preparar ambiente..."

                            // Script robusto para corrigir acesso ao Docker Desktop (Windows/Linux)
                            sh """
                            # Copia o ficheiro secreto para um temporário editável
                            cp \$KUBECONFIG_FILE k8s-config-temp
                            chmod 600 k8s-config-temp
                            
                            # Substitui os endereços locais pelo endereço da rede interna do Docker
                            sed -i 's|kubernetes.docker.internal|host.docker.internal|g' k8s-config-temp
                            sed -i 's|127.0.0.1|host.docker.internal|g' k8s-config-temp
                            sed -i 's|localhost|host.docker.internal|g' k8s-config-temp
                            
                            # Define este ficheiro corrigido como a configuração ativa
                            export KUBECONFIG=\$(pwd)/k8s-config-temp

                            # Remove dados de certificação antigos e força aceitação de SSL inseguro (ambiente dev)
                            kubectl config unset clusters.docker-desktop.certificate-authority-data
                            kubectl config set-cluster docker-desktop --insecure-skip-tls-verify=true
                            
                            echo ">>> A testar ligação ao Cluster..."
                            kubectl get nodes
                            
                            echo ">>> Atualizando Release Helm: ${ReleaseName} no namespace ${KubeNamespace}..."
                            
                            # Garante que o namespace existe
                            kubectl create namespace ${KubeNamespace} --dry-run=client -o yaml | kubectl apply -f -
                            
                            # Executa o Helm Upgrade com as variáveis dinâmicas
                            helm upgrade --install ${ReleaseName} ./charts/products-api \
                              --namespace ${KubeNamespace} \
                              --set app.image.repository=$DOCKER_USER/${IMAGE_NAME} \
                              --set app.image.tag=${env.TAG_FINAL} \
                              --set app.env.environment=${env.BRANCH_NAME == 'quality' ? 'Production' : 'Development'}
                            """
                        }
                    }
                }
            }
        }
    }

    post {
        always {
            script {
                // Bloco para recuperar os logs e enviar para o GitHub
                // Requer a credencial 'github-token' configurada no Jenkins
                withCredentials([usernamePassword(credentialsId: 'github-token', usernameVariable: 'GIT_USER', passwordVariable: 'GIT_PASS')]) {
                    
                    def pasta = "deploy_logs"
                    def nomeFicheiro = "${env.TAG_FINAL}_log.txt"
                    def caminhoFicheiro = "${pasta}/${nomeFicheiro}"
                    
                    // Captura as últimas 10.000 linhas de log desta execução
                    def logs = currentBuild.rawBuild.getLog(10000)
                    def logContent = logs.join("\n")
                    
                    echo ">>> A gravar logs de deployment no Git..."
                    
                    sh """
                        # Configuração temporária do Git para o commit
                        git config user.email "noreply@jenkins.log"
                        git config user.name "JenkinsLog"
                        
                        mkdir -p ${pasta}
                    """
                    
                    // Escreve o log no disco
                    writeFile file: caminhoFicheiro, text: logContent

                    // Faz commit e push. 
                    sh """
                        git add ${caminhoFicheiro}
                        git commit -m "Log Deploy: ${env.TAG_FINAL} [skip ci]" || echo "Nada para commitar"
                        git push https://${GIT_PASS}@github.com/user1221510/DOS2526-E.git HEAD:${env.BRANCH_NAME}
                    """
                }
            }
        }
    }
}