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
                    def branchClean = env.BRANCH_NAME.toLowerCase().replace('_', '-')
                    
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
                                /d:sonar.exclusions="**/bin/**,**/obj/**,**/publish/**,**/app_publish/**,**/TestResults/**"
                        """
                    }
                }
            }
        }

        stage('Restore & Test') {
            steps {
                sh 'dotnet restore'
                sh '''
                dotnet add ProductsAPI.Tests package JunitXml.TestLogger
                dotnet test ProductsAPI.Tests \
                  --logger "junit;LogFileName=test-results.xml" \
                  --collect:"XPlat Code Coverage"
                '''
            }
            post {
                always { junit '**/test-results.xml' }
            }
        }

        stage('Build & Analysis End') {
            steps {
                sh 'dotnet publish ProductsAPI.csproj -c Release -o app_publish'
                withCredentials([string(credentialsId: 'sonarqube-token', variable: 'SONAR_TOKEN')]) {
                    sh 'dotnet sonarscanner end /d:sonar.token="${SONAR_TOKEN}"'
                }
            }
        }

        stage('Build Docker Image') {
            steps {
                script {
                    echo ">>> Construindo Imagem: ${DOCKER_IMAGE}:${env.TAG_FINAL} <<<"
                    sh "docker build -t ${DOCKER_IMAGE}:${env.TAG_FINAL} ."
                    
                    // IMPORTANTE: Para o ArgoCD funcionar localmente, precisamos de garantir 
                    // que a tag 'latest' aponta sempre para o build mais recente.
                    sh "docker tag ${DOCKER_IMAGE}:${env.TAG_FINAL} ${DOCKER_IMAGE}:latest"
                }
            }
        }

        stage('GitOps Update') {
            // Executa apenas nas branches certas
            when {
                expression { env.BRANCH_NAME.toLowerCase().startsWith('dev_') || env.BRANCH_NAME == 'quality' }
            }
            steps {
                script {
                    echo ">>> Atualizando versão no Git para o ArgoCD..."
                    
                    withCredentials([usernamePassword(credentialsId: 'github-token', usernameVariable: 'GIT_USER', passwordVariable: 'GIT_PASS')]) {
                        sh """
                            # 1. Configurar identidade do Git
                            git config user.email "jenkins@pipeline.com"
                            git config user.name "Jenkins Pipeline"
                            
                            # 2. Atualizar o values.yaml
                            # Substitui a primeira ocorrência de tag: "..." pela nova tag
                            sed -i '0,/tag: ".*"/s//tag: "${env.TAG_FINAL}"/' charts/products-api/values.yaml
                            
                            # 3. Commit e Push
                            git add charts/products-api/values.yaml
                            
                            # O [skip ci] impede que este commit dispare outro pipeline (loop infinito)
                            git commit -m "GitOps: Deploy version ${env.TAG_FINAL} [skip ci]"
                            
                            git push https://${GIT_PASS}@github.com/user1221510/DOS2526-E.git HEAD:${env.BRANCH_NAME}
                        """
                    }
                }
            }
        }
    }
}