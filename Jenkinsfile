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

        stage('Restore') {
            steps {
                sh 'dotnet restore'
            }
        }

        stage('Test + Coverage') {
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
            steps {
                sh 'dotnet publish -c Release -o publish'
            }
        }

        stage('Build Docker Image') {
            steps {
                script {
                    def tag = "${env.BUILD_NUMBER}" 
                    
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
                branch 'quality'
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