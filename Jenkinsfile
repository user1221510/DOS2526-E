pipeline {
    agent any

    environment {
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
                dotnet add ProductsAPI.Tests package JunitXml.TestLogger
                dotnet test ProductsAPI.Tests \
                  --logger "junit;LogFileName=test-results.xml" \
                  --collect:"XPlat Code Coverage"
                '''
            }
            post {
                always {
                    junit '**/test-results.xml'
                }
            }
        }

        stage('Build .NET') {
            steps {
                sh 'dotnet publish ProductsAPI.csproj -c Release -o publish'
            }
        }

        stage('Build Docker Image') {
            steps {
                script {
                    def dataHora = new Date().format("yyyy-MM-dd'T'HHmm", TimeZone.getTimeZone('UTC'))
                    
                    def prefixo = ""
                    if (env.BRANCH_NAME == 'quality') {
                        prefixo = "quality"
                    } else {
                        prefixo = "dev"
                    }
                    
                    env.TAG_FINAL = "${prefixo}-${dataHora}"
                    
                    echo ">>> Tag gerada: ${env.TAG_FINAL} <<<"

                    sh "docker build -t ${DOCKER_IMAGE}:${env.TAG_FINAL} ."
                    sh "docker tag ${DOCKER_IMAGE}:${env.TAG_FINAL} ${DOCKER_IMAGE}:latest"
                    
                    // Tenta upload (ignora erro se não houver login)
                    sh "docker push ${DOCKER_IMAGE}:${env.TAG_FINAL} || echo 'Aviso: Upload Docker ignorado.'"
                }
            }
        }

        stage('Deploy QUALITY') {
            when {
                branch 'quality'
            }
            steps {
                sh 'chmod +x ./deploy/prod.sh'
                sh "./deploy/prod.sh ${DOCKER_IMAGE}:${env.TAG_FINAL}"
            }
        }

        stage('Deploy DEV') {
            when {
                expression { env.BRANCH_NAME.startsWith('dev_') || env.BRANCH_NAME == 'development' }
            }
            steps {
                sh 'chmod +x ./deploy/dev.sh'
                sh "./deploy/dev.sh ${DOCKER_IMAGE}:${env.TAG_FINAL}"
            }
        }
    }

    post {
        success {
            echo "Pipeline executado com sucesso"
            script {
                // Vai buscar o token seguro para fazer o push
                withCredentials([usernamePassword(credentialsId: 'github-token', usernameVariable: 'GIT_USER', passwordVariable: 'GIT_PASS')]) {
                    
                    def nomeFicheiro = "${env.TAG_FINAL}_log.txt"
                    
                    // --- ALTERAÇÃO AQUI: IDENTIDADE DO GIT ---
                    sh """
                        git config user.email "noreply@jenkins.log"
                        git config user.name "JenkinsLog"
                    """

                    // Criar o ficheiro de log
                    sh """
                        echo "Build Jenkins com Sucesso." > ${nomeFicheiro}
                        echo "Data: \$(date)" >> ${nomeFicheiro}
                        echo "Imagem Criada: ${DOCKER_IMAGE}:${env.TAG_FINAL}" >> ${nomeFicheiro}
                    """

                    // Enviar para o GitHub usando a variável segura ${GIT_PASS}
                    sh """
                        git add ${nomeFicheiro}
                        git commit -m "JenkinsLog: Registo do build ${env.TAG_FINAL} [skip ci]" || echo "Nada para commitar"
                        git push https://${GIT_PASS}@github.com/user1221510/DOS2526-E.git HEAD:${env.BRANCH_NAME}
                    """
                }
            }
        }
        failure {
            echo "Pipeline falhou"
        }
    }
}