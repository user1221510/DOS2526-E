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
                // Executa os testes unitários
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
                    // Usa a credencial do DockerHub (dockerhub-token) para login
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
            // Executa apenas se for a branch 'quality' ou começar por 'dev_'
            when {
                expression { env.BRANCH_NAME == 'quality' || env.BRANCH_NAME.startsWith('dev_') }
            }
            steps {
         script {
                    withCredentials([usernamePassword(credentialsId: 'dockerhub-token', usernameVariable: 'DOCKER_USER', passwordVariable: 'DOCKER_PASS')]) {
                        withCredentials([file(credentialsId: 'kubernet-token', variable: 'KUBECONFIG_FILE')]) {
                            
                            echo ">>> Configuração K8s carregada. A preparar ambiente..."

                            sh """
                            # 1. Copiar o ficheiro secreto para um temporário
                            cp \$KUBECONFIG_FILE k8s-config-temp
                            chmod 600 k8s-config-temp
                            
                            # 2. Corrigir o endereço de rede (como já tinhas)
                            sed -i 's|kubernetes.docker.internal|host.docker.internal|g' k8s-config-temp
                            sed -i 's|127.0.0.1|host.docker.internal|g' k8s-config-temp
                            sed -i 's|localhost|host.docker.internal|g' k8s-config-temp
                            
                            # 3. Definir este ficheiro como a configuração ativa
                            export KUBECONFIG=\$(pwd)/k8s-config-temp
                            
                            # --- A CORREÇÃO NOVA ESTÁ AQUI EM BAIXO ---
                            # Removemos a autoridade de certificação antiga
                            kubectl config unset clusters.docker-desktop.certificate-authority-data
                            # Dizemos ao kubectl para não validar o certificado SSL (ignora o erro do nome)
                            kubectl config set-cluster docker-desktop --insecure-skip-tls-verify=true
                            # -------------------------------------------

                            echo ">>> A testar ligação ao Cluster..."
                            kubectl get nodes
                            
                            echo ">>> Atualizando Release Helm: ${ReleaseName} no namespace ${KubeNamespace}..."
                            
                            # Garante que o namespace existe
                            kubectl create namespace ${KubeNamespace} --dry-run=client -o yaml | kubectl apply -f -
                            
                            # Executa o Helm Upgrade
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
}