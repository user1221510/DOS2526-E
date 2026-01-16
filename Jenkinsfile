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
                        // Se for quality, o ambiente é 'prod'
                        env.ENV_NAME = 'prod'
                        env.SONAR_PROJECT_NAME = "DOS API [PROD]"
                        env.SONAR_PROJECT_KEY = "dos2526-api-prod"
                    } else {
                        // Se for dev_francisco, o ambiente é 'dev-francisco'
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
                    sh "docker tag ${DOCKER_IMAGE}:${env.TAG_FINAL} ${DOCKER_IMAGE}:latest"
                }
            }
        }

        stage('GitOps Update') {
            // Executa apenas se for branch dev_ ou quality
            when {
                expression { env.BRANCH_NAME.toLowerCase().startsWith('dev_') || env.BRANCH_NAME == 'quality' }
            }
            steps {
                script {
                    echo ">>> GitOps: Configurando ambiente [${env.ENV_NAME}] na branch [${env.BRANCH_NAME}]..."
                    
                    withCredentials([usernamePassword(credentialsId: 'github-token', usernameVariable: 'GIT_USER', passwordVariable: 'GIT_PASS')]) {
                        sh """
                            # 1. Configurar identidade do Git
                            git config user.email "jenkins@pipeline.com"
                            git config user.name "Jenkins Pipeline"
                            
                            # Faz pull para garantir a versão mais recente
                            git pull origin ${env.BRANCH_NAME}
                            
                            # -----------------------------------------------------
                            # 2. ATUALIZAR VALUES.YAML (Tag da Imagem)
                            # -----------------------------------------------------
                            sed -i 's/tag: ".*"/tag: "${env.TAG_FINAL}"/' charts/products-api/values.yaml
                            
                            # -----------------------------------------------------
                            # 3. ATUALIZAR ARGOCD-APP.YAML (Branch e Namespace)
                            # -----------------------------------------------------
                            
                            # A. Define a Branch que o ArgoCD vai ler (targetRevision)
                            sed -i "s|targetRevision: .*|targetRevision: ${env.BRANCH_NAME}|" argocd-app.yaml

                            # B. Define o Namespace de destino (dentro do bloco destination)
                            # Procura o bloco entre 'destination:' e 'syncPolicy:' e altera o namespace lá dentro
                            sed -i "/destination:/,/syncPolicy:/ s/namespace: .*/namespace: ${env.ENV_NAME}/" argocd-app.yaml

                            # -----------------------------------------------------
                            # 4. COMMIT E PUSH
                            # -----------------------------------------------------
                            git add charts/products-api/values.yaml argocd-app.yaml
                            
                            # O [skip ci] impede loop infinito
                            git commit -m "GitOps: Deploy env [${env.ENV_NAME}] version ${env.TAG_FINAL} [skip ci]"
                            
                            git push https://${GIT_PASS}@github.com/user1221510/DOS2526-E.git HEAD:${env.BRANCH_NAME}
                        """
                    }
                }
            }
        }
    }
}