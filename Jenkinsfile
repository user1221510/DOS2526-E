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

        stage('SonarQube Start') {
            steps {
                withCredentials([string(credentialsId: 'sonarqube-token', variable: 'SONAR_TOKEN')]) {
                    script {
                        def branchName = env.BRANCH_NAME.replaceAll("/", "-") // Substitui barras por traços se houver
                        def projectKey = "dos2526-api-${branchName}"
                        def projectName = "DOS API [${branchName}]"
                        
                        def projectVersion = "${branchName}-${env.DATA_HORA}"
                        
                        env.TAG_FINAL = projectVersion

                        echo ">>> SonarQube: Projeto=${projectName} | Versão=${projectVersion} <<<"

                        sh """
                            dotnet sonarscanner begin \
                                /k:"${projectKey}" \
                                /n:"${projectName}" \
                                /v:"${projectVersion}" \
                                /d:sonar.host.url="http://infra-sonarqube:9000" \
                                /d:sonar.token="${SONAR_TOKEN}" \
                                /d:sonar.cs.opencover.reportsPaths="**/coverage.cobertura.xml" \
                                /d:sonar.qualitygate.wait=true
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
                        echo ">>> A finalizar análise SonarQube..."
                        sh 'dotnet sonarscanner end /d:sonar.token="${SONAR_TOKEN}"'
                    }
                }
            }
        }

        stage('Build Docker Image') {
            steps {
                script {
                    echo ">>> Construindo Docker com Tag: ${env.TAG_FINAL} <<<"

                    sh "docker build -t ${DOCKER_IMAGE}:${env.TAG_FINAL} ."
                    sh "docker tag ${DOCKER_IMAGE}:${env.TAG_FINAL} ${DOCKER_IMAGE}:latest"
                    sh "docker push ${DOCKER_IMAGE}:${env.TAG_FINAL} || echo 'Aviso: Upload Docker ignorado.'"
                }
            }
        }

        stage('Deploy QUALITY') {
            when { branch 'quality' }
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
                withCredentials([usernamePassword(credentialsId: 'github-token', usernameVariable: 'GIT_USER', passwordVariable: 'GIT_PASS')]) {
                    
                    def pasta = "deploy_logs"
                    def caminhoFicheiro = "${pasta}/${env.TAG_FINAL}_log.txt"
                    
                    // Capturar log (até 10000 linhas)
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