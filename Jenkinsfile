pipeline {
    agent any

    environment {
        DOCKER_IMAGE = "dos2526-api"
        DATA_HORA = sh(script: "date +%Y-%m-%d-%H%M", returnStdout: true).trim()
    }

    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Setup Names') {
            steps {
                script {
                    def branchClean = env.BRANCH_NAME.toLowerCase()
                    
                    if (branchClean == 'quality') {
                        env.ENV_NAME = 'prod'
                        env.SONAR_PROJECT_NAME = "DOS API [PROD]"
                        env.SONAR_PROJECT_KEY = "dos2526-api-prod"
                    } else {
                        env.ENV_NAME = branchClean
                        env.SONAR_PROJECT_NAME = "DOS API [${branchClean}]"
                        env.SONAR_PROJECT_KEY = "dos2526-api-${branchClean}"
                    }
                    
                    env.TAG_FINAL = "${env.ENV_NAME}-${env.DATA_HORA}"
                    
                    echo ">>> CONFIGURAÇÃO <<<"
                    echo "Branch Real: ${env.BRANCH_NAME}"
                    echo "Ambiente:    ${env.ENV_NAME}"
                    echo "Tag Final:   ${env.TAG_FINAL}"
                }
            }
        }

        stage('SonarQube Start') {
            steps {
                withCredentials([string(credentialsId: 'sonarqube-token', variable: 'SONAR_TOKEN')]) {
                    script {
                        // ATUALIZAÇÃO: Adicionado '**/app_publish/**' às exclusões
                        sh """
                        dotnet sonarscanner begin \
                                /k:"${env.SONAR_PROJECT_KEY}" \
                                /n:"${env.SONAR_PROJECT_NAME}" \
                                /v:"${env.TAG_FINAL}" \
                                /d:sonar.host.url="http://infra-sonarqube:9000" \
                                /d:sonar.token="${SONAR_TOKEN}" \
                                /d:sonar.cs.opencover.reportsPaths="**/coverage.cobertura.xml" \
                                /d:sonar.qualitygate.wait=true \
                                /d:sonar.exclusions="**/bin/**,**/obj/**,**/publish/**,**/app_publish/**,**/TestResults/**"
                        """
                    }
                }
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
                // CORREÇÃO CRÍTICA: Mudança da pasta de saída para 'app_publish'
                sh 'dotnet publish ProductsAPI.csproj -c Release -o app_publish'
            }
        }

        stage('SonarQube End') {
            steps {
                withCredentials([string(credentialsId: 'sonarqube-token', variable: 'SONAR_TOKEN')]) {
                    script {
                        sh 'dotnet sonarscanner end /d:sonar.token="${SONAR_TOKEN}"'
                    }
                }
            }
        }

        stage('Build Docker Image') {
            steps {
                script {
                    echo ">>> Construindo Imagem: ${DOCKER_IMAGE}:${env.TAG_FINAL} <<<"
                    sh "docker build -t ${DOCKER_IMAGE}:${env.TAG_FINAL} ."
                    sh "docker tag ${DOCKER_IMAGE}:${env.TAG_FINAL} ${DOCKER_IMAGE}:latest"
                    
                    echo ">>> Imagem construída com sucesso (Local) <<<"
                }
            }
        }

        stage('Deploy PROD') {
            when { branch 'quality' }
            steps {
                script {
                    def containerName = "dos2526-api-prod"
                    def port = "8055"
                    
                    echo ">>> A iniciar Deploy PROD em ${port}..."
                    
                    sh "docker stop ${containerName} || true"
                    sh "docker rm ${containerName} || true"
                    sh "docker run -d --restart unless-stopped -p ${port}:8080 --name ${containerName} ${DOCKER_IMAGE}:${env.TAG_FINAL}"
                }
            }
        }

        stage('Deploy DEV') {
            when {
                expression { env.BRANCH_NAME.toLowerCase().startsWith('dev_') }
            }
            steps {
                script {
                    def containerName = "dos2526-api-${env.ENV_NAME}"
                    def port = "8050"
                    
                    echo ">>> A iniciar Deploy DEV (${env.ENV_NAME}) em ${port}..."
                    
                    sh "docker stop ${containerName} || true"
                    sh "docker rm ${containerName} || true"
                    sh "docker run -d --restart unless-stopped -p ${port}:8080 --name ${containerName} ${DOCKER_IMAGE}:${env.TAG_FINAL}"
                }
            }
        }
    }

    post {
        success {
            echo "Pipeline executado com sucesso"
            script {
                withCredentials([usernamePassword(credentialsId: 'github-token', usernameVariable: 'GIT_USER', passwordVariable: 'GIT_PASS')]) {
                    
                    def pasta = "deploy_logs"
                    def caminhoFicheiro = "${pasta}/${env.TAG_FINAL}_log.txt"
                    
                    def logs = currentBuild.rawBuild.getLog(10000)
                    def logContent = logs.join("\n")
                    
                    sh """
                        git config user.email "noreply@jenkins.log"
                        git config user.name "JenkinsLog"
                        mkdir -p ${pasta}
                    """
                    
                    writeFile file: caminhoFicheiro, text: logContent

                    sh """
                        git add ${caminhoFicheiro}
                        git commit -m "JenkinsLog: ${env.TAG_FINAL} [skip ci]" || echo "Nada para commitar"
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