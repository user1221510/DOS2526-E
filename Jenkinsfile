pipeline {
    agent any

    environment {
        APP_NAME = "dos2526-api"
        DOCKER_IMAGE = "dos2526-api"
        DOTNET_ENV = "Production"
    }

    stages {

        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        // --- FASES QUE PRECISAM DE .NET ---
        // Usamos um agente Docker aqui para ter o comando 'dotnet' disponível
        stage('Restore') {
            agent {
                docker { 
                    image 'mcr.microsoft.com/dotnet/sdk:9.0' 
                    // O reuseNode garante que usamos o mesmo workspace
                    reuseNode true 
                }
            }
            steps {
                sh 'dotnet restore'
            }
        }

        stage('Test + Coverage') {
            agent {
                docker { 
                    image 'mcr.microsoft.com/dotnet/sdk:9.0' 
                    reuseNode true 
                }
            }
            steps {
                sh '''
                dotnet test ProductsAPI.Tests \
                  --logger "trx;LogFileName=test-results.trx" \
                  --collect:"XPlat Code Coverage"
                '''
            }
            post {
                always {
                    junit '**/test-results.trx'
                }
            }
        }

        stage('Build .NET') {
            agent {
                docker { 
                    image 'mcr.microsoft.com/dotnet/sdk:9.0' 
                    reuseNode true 
                }
            }
            steps {
                sh 'dotnet publish -c Release -o publish'
            }
        }

        // --- FASES QUE PRECISAM DE DOCKER (Volta ao agent any/host) ---
        stage('Build Docker Image') {
            steps {
                script {
                    // Se a tag falhar por ser null, usa 'latest' ou um timestamp
                    def tag = env.BUILD_NUMBER ?: "latest"
                    
                    sh """
                    docker build -t ${DOCKER_IMAGE}:${tag} .
                    docker tag ${DOCKER_IMAGE}:${tag} ${DOCKER_IMAGE}:latest
                    """
                }
            }
        }

        stage('Deploy DEV') {
            when {
                branch 'development' 
            }
            steps {
                sh 'chmod +x ./deploy/dev.sh' 
                sh './deploy/dev.sh'
            }
        }

        stage('Deploy PROD') {
            when {
                branch 'main' 
            }
            steps {
                sh 'chmod +x ./deploy/prod.sh'
                sh './deploy/prod.sh'
            }
        }
    }

    post {
        success {
            echo "Pipeline executado com sucesso"
            script {
                sh """
                    git config user.email "jenkins@bot.com"
                    git config user.name "Jenkins Bot"
                """
                sh "echo 'Deploy efetuado em ' \$(date) > deploy_log.txt"
                sh """
                    git add deploy_log.txt
                    git commit -m "Jenkins: Update deploy log [skip ci]" || echo "Nada para commitar"
                """
            }
        }
        failure {
            echo "Pipeline falhou"
        }
    }
}