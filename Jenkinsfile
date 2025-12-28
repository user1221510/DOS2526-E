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
                        sh """
                            dotnet sonarscanner begin \
                                /k:"${env.SONAR_PROJECT_KEY}" \
                                /n:"${env.SONAR_PROJECT_NAME}" \
                                /v:"${env.TAG_FINAL}" \
                                /d:sonar.host.url="http://infra-sonarqube:9000" \
                                /d:sonar.token="${SONAR_TOKEN}" \
                                /d:sonar.cs.opencover.reportsPaths="**/coverage.cobertura.xml" \
                                /d:sonar.qualitygate.wait=true \
                                /d:sonar.exclusions="**/bin/**,**/obj/**,**/publish/**,**/TestResults/**"
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
                sh 'dotnet publish ProductsAPI.csproj -c Release -o publish'
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
                    def basePort = 8055
                    def maxAttempts = 5
                    def deployed = false
                    
                    echo ">>> A iniciar Deploy PROD a partir da porta ${basePort}..."
                    
                    for (int i = 0; i < maxAttempts && !deployed; i++) {
                        def currentPort = basePort + i
                        
                        sh "docker stop ${containerName} || true"
                        sh "docker rm ${containerName} || true"
                        
                        // Tentar executar o container
                        def result = sh(
                            script: "docker run -d --restart unless-stopped -p ${currentPort}:8080 --name ${containerName} ${DOCKER_IMAGE}:${env.TAG_FINAL} 2>&1",
                            returnStatus: true,
                            returnStdout: true
                        )
                        
                        if (result.status == 0) {
                            echo ">>> Container implantado com sucesso na porta ${currentPort}"
                            deployed = true
                        } else if (result.output.contains("port is already allocated")) {
                            echo ">>> Porta ${currentPort} indisponível, tentando próxima..."
                        } else {
                            error ">>> Erro ao implantar container: ${result.output}"
                        }
                    }
                    
                    if (!deployed) {
                        error ">>> Não foi possível implantar o container em nenhuma porta (${basePort}-${basePort + maxAttempts - 1})."
                    }
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
                    def basePort = 8050
                    def maxAttempts = 10
                    def deployed = false
                    
                    // Parar o container do docker-compose se estiver a usar a porta
                    sh "docker stop dos2526-e-web-1 || true"
                    sh "docker rm dos2526-e-web-1 || true"
                    
                    echo ">>> A iniciar Deploy DEV (${env.ENV_NAME}) a partir da porta ${basePort}..."
                    
                    for (int i = 0; i < maxAttempts && !deployed; i++) {
                        def currentPort = basePort + i
                        
                        sh "docker stop ${containerName} || true"
                        sh "docker rm ${containerName} || true"
                        
                        // Tentar executar o container
                        def result = sh(
                            script: "docker run -d --restart unless-stopped -p ${currentPort}:8080 --name ${containerName} ${DOCKER_IMAGE}:${env.TAG_FINAL} 2>&1",
                            returnStatus: true,
                            returnStdout: true
                        )
                        
                        if (result.status == 0) {
                            echo ">>> Container implantado com sucesso na porta ${currentPort}"
                            deployed = true
                        } else if (result.output.contains("port is already allocated")) {
                            echo ">>> Porta ${currentPort} indisponível, tentando próxima..."
                        } else {
                            error ">>> Erro ao implantar container: ${result.output}"
                        }
                    }
                    
                    if (!deployed) {
                        error ">>> Não foi possível implantar o container em nenhuma porta (${basePort}-${basePort + maxAttempts - 1})."
                    }
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