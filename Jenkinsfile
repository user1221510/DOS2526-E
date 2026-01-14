pipeline {
    agent any

    environment {
        // --- CONFIGURAÇÃO ---
        IMAGE_NAME = "dos2526-api"
        DATA_HORA = sh(script: "date +%Y-%m-%d-%H%M", returnStdout: true).trim()
        
        // Define o Namespace do Kubernetes baseado na branch
        // Se for 'quality' vai para 'production', caso contrário 'staging'
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
                // Mantém a lógica original de testes
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
                    // Lê o username e password da credencial 'dockerhub-token'
                    withCredentials([usernamePassword(credentialsId: 'dockerhub-token', usernameVariable: 'DOCKER_USER', passwordVariable: 'DOCKER_PASS')]) {
                        sh """
                        echo ">>> Login no Docker Hub com o utilizador: $DOCKER_USER"
                        echo $DOCKER_PASS | docker login -u $DOCKER_USER --password-stdin
                        
                        echo ">>> Construindo imagem: $DOCKER_USER/${IMAGE_NAME}:${env.TAG_FINAL}"
                        docker build -t $DOCKER_USER/${IMAGE_NAME}:${env.TAG_FINAL} .
                        
                        echo ">>> Enviando imagem para o registry..."
                        docker push $DOCKER_USER/${IMAGE_NAME}:${env.TAG_FINAL}
                        """
                    }
                }
            }
        }

        stage('Deploy no K8s (Helm)') {
            // Executa apenas se for a branch 'quality' ou uma branch de desenvolvimento 'dev_'
            when {
                expression { env.BRANCH_NAME == 'quality' || env.BRANCH_NAME.startsWith('dev_') }
            }
            steps {
                script {
                    withCredentials([usernamePassword(credentialsId: 'dockerhub-token', usernameVariable: 'DOCKER_USER', passwordVariable: 'DOCKER_PASS')]) {
                        
                        echo ">>> Atualizando Release Helm: ${ReleaseName} no namespace ${KubeNamespace} <<<"
                        
                        // 1. Garante que o namespace existe (cria se não existir)
                        sh "kubectl create namespace ${KubeNamespace} --dry-run=client -o yaml | kubectl apply -f -"
                        
                        // 2. Executa o Helm Upgrade
                        // O --set app.image.repository garante que usamos o utilizador dinamicamente
                        sh """
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