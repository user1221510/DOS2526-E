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

        stage('Setup Names & Ports') {
            steps {
                script {
                    def branchClean = env.BRANCH_NAME.toLowerCase().replace('_', '-')
                    
                    if (branchClean == 'quality') {
                        env.ENV_NAME = 'prod'
                        env.SONAR_PROJECT_NAME = "DOS API [PROD]"
                        env.SONAR_PROJECT_KEY = "dos2526-api-prod"
                        // Porta para PROD
                        env.NODE_PORT = "30055"
                    } else {
                        env.ENV_NAME = branchClean
                        env.SONAR_PROJECT_NAME = "DOS API [${branchClean}]"
                        env.SONAR_PROJECT_KEY = "dos2526-api-${branchClean}"
                        // Porta para DEV
                        env.NODE_PORT = "30050"
                    }
                    
                    env.TAG_FINAL = "${env.ENV_NAME}-${env.DATA_HORA}"
                    
                    echo ">>> CONFIGURAÇÃO <<<"
                    echo "Ambiente: ${env.ENV_NAME}"
                    echo "Tag:      ${env.TAG_FINAL}"
                    echo "Porta:    ${env.NODE_PORT}"
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
                                /d:sonar.host.url="http://host.docker.internal:9001" \
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
                    sh "docker tag ${DOCKER_IMAGE}:${env.TAG_FINAL} ${DOCKER_IMAGE}:latest"
                }
            }
        }

        stage('GitOps Update') {
            when {
                expression { env.BRANCH_NAME.toLowerCase().startsWith('dev_') || env.BRANCH_NAME == 'quality' }
            }
            steps {
                script {
                    echo ">>> Atualizando Git para ArgoCD (Tag: ${env.TAG_FINAL} | Port: ${env.NODE_PORT})..."
                    
                    withCredentials([usernamePassword(credentialsId: 'github-token', usernameVariable: 'GIT_USER', passwordVariable: 'GIT_PASS')]) {
                        sh """
                            git config user.email "jenkins@pipeline.com"
                            git config user.name "Jenkins Pipeline"
                            
                            git pull origin ${env.BRANCH_NAME}
                            
                            # 1. Atualizar a Tag da Imagem
                            sed -i '0,/tag: ".*"/s//tag: "${env.TAG_FINAL}"/' charts/products-api/values.yaml
                            
                            # 2. Atualizar a Porta (NodePort)
                            # Procura 'nodePort: ...' e substitui pelo valor correto do ambiente
                            sed -i 's/nodePort: [0-9]*/nodePort: ${env.NODE_PORT}/' charts/products-api/values.yaml

                            # 3. Configurar ArgoCD App (Branch e Namespace)
                            sed -i "s|targetRevision: .*|targetRevision: ${env.BRANCH_NAME}|" argocd-app.yaml
                            sed -i "/destination:/,/syncPolicy:/ s/namespace: .*/namespace: ${env.ENV_NAME}/" argocd-app.yaml

                            # 4. Commit e Push
                            git add charts/products-api/values.yaml argocd-app.yaml
                            
                            git commit -m "GitOps: Deploy [${env.ENV_NAME}] port:${env.NODE_PORT} ver:${env.TAG_FINAL} [skip ci]"
                            
                            git push https://${GIT_PASS}@github.com/user1221510/DOS2526-E.git HEAD:${env.BRANCH_NAME}
                        """
                    }
                }
            }
        }
    }
}