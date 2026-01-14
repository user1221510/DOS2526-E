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
                    // 1. Vai buscar o user/pass do DockerHub para configurar o Helm
                    withCredentials([usernamePassword(credentialsId: 'dockerhub-token', usernameVariable: 'DOCKER_USER', passwordVariable: 'DOCKER_PASS')]) {
                        
                        // 2. Vai buscar o ficheiro de configuração do Kubernetes (ID: kubernet-token)
                        withCredentials([file(credentialsId: 'kubernet-token', variable: 'KUBECONFIG_FILE')]) {
                            
                            echo ">>> Configuração K8s carregada. A preparar ambiente..."

                            // Script de correção de rede para Windows/Docker Desktop
                            sh """
                            # Copia o ficheiro secreto para um temporário editável
                            cp \$KUBECONFIG_FILE k8s-config-temp
                            chmod 600 k8s-config-temp
                            
                            # Substitui os endereços do Docker Desktop pelo endereço interno acessível pelo Jenkins
                            sed -i 's|kubernetes.docker.internal|host.docker.internal|g' k8s-config-temp
                            sed -i 's|127.0.0.1|host.docker.internal|g' k8s-config-temp
                            sed -i 's|localhost|host.docker.internal|g' k8s-config-temp
                            
                            # Define este ficheiro corrigido como a configuração ativa para esta sessão
                            export KUBECONFIG=\$(pwd)/k8s-config-temp
                            
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