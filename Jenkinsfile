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
                    // 1. Gerar Data/Hora (Ex: 2025-12-22T2330)
                    def dataHora = new Date().format("yyyy-MM-dd'T'HHmm", TimeZone.getTimeZone('UTC'))
                    
                    // 2. Definir o prefixo da Tag
                    def prefixo = ""
                    
                    if (env.BRANCH_NAME == 'quality') {
                        prefixo = "quality"  // Tag: quality-DATA
                    } else {
                        prefixo = "dev"      // Tag: dev-DATA (para branches dev_)
                    }
                    
                    // 3. Criar a TAG FINAL (Guardada na variável global)
                    env.TAG_FINAL = "${prefixo}-${dataHora}"
                    
                    echo ">>> Tag gerada para este Build: ${env.TAG_FINAL} <<<"

                    // 4. Build, Tag e Push (Upload)
                    sh "docker build -t ${DOCKER_IMAGE}:${env.TAG_FINAL} ."
                    sh "docker tag ${DOCKER_IMAGE}:${env.TAG_FINAL} ${DOCKER_IMAGE}:latest"
                    
                    // Tenta fazer upload, se falhar continua (para não parar o pipeline)
                    sh "docker push ${DOCKER_IMAGE}:${env.TAG_FINAL} || echo 'Aviso: Upload falhou (sem login).'"
                }
            }
        }

        // --- LÓGICA DE DEPLOY ---

        // CASO 1: QUALITY (Usa o script prod.sh -> Porta 8055)
        stage('Deploy QUALITY') {
            when {
                branch 'quality'
            }
            steps {
                sh 'chmod +x ./deploy/prod.sh'
                sh "./deploy/prod.sh ${DOCKER_IMAGE}:${env.TAG_FINAL}"
            }
        }

        // CASO 2: DEV (Usa o script dev.sh -> Porta 8060)
        // Ativa-se para qualquer branch que comece por "dev_" ou se chame "development"
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
                sh """
                    git config user.email "jenkins@bot.com"
                    git config user.name "Jenkins Bot"
                """

                // Define o nome do ficheiro (ex: quality-2025-12-22T2330_log.txt)
                def nomeFicheiro = "${env.TAG_FINAL}_log.txt"
                
                // Cria o ficheiro com info do build
                sh """
                    echo "Build Jenkins com Sucesso." > ${nomeFicheiro}
                    echo "Data: \$(date)" >> ${nomeFicheiro}
                    echo "Imagem Criada: ${DOCKER_IMAGE}:${env.TAG_FINAL}" >> ${nomeFicheiro}
                """

                // Envia para o GitHub na branch atual
                sh """
                    git add ${nomeFicheiro}
                    git commit -m "Jenkins: Log do build ${env.TAG_FINAL} [skip ci]" || echo "Nada para commitar"
                    git push origin HEAD:${env.BRANCH_NAME}
                """
            }
        }
        failure {
            echo "Pipeline falhou"
        }
    }
}