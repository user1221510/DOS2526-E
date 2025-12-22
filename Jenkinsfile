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
                sh '''
                cd DOS2526-E
                dotnet restore
                '''
            }
        }

        stage('Test + Coverage') {
            steps {
                sh '''
                cd DOS2526-E
                dotnet test ProductsAPI.Tests \
                  --logger "trx;LogFileName=test-results.trx" \
                  --collect:"XPlat Code Coverage"
                '''
            }
            post {
                always {
                    junit 'DOS2526-E/**/test-results.trx'
                }
            }
        }

        stage('Build .NET') {
            steps {
                sh '''
                cd DOS2526-E
                dotnet publish -c Release -o publish
                '''
            }
        }

        stage('Build Docker Image') {
            steps {
                script {
                    def tag = "${env.BRANCH_NAME}-${env.BUILD_NUMBER}"
                    sh """
                    cd DOS2526-E
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
                sh '''
                cd DOS2526-E
                ./deploy/dev.sh
                '''
            }
        }

        stage('Deploy PROD') {
            when {
                branch 'main'
            }
            steps {
                sh '''
                cd DOS2526-E
                ./deploy/prod.sh
                '''
            }
        }
    }

    post {
        success {
            echo "Pipeline executado com sucesso"
        }
        failure {
            echo "Pipeline falhou"
        }
    }
}
