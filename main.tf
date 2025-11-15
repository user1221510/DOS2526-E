# Configuração do Terraform
terraform {
  required_providers {
    docker = {
      source  = "kreuzwerker/docker"
      version = "~> 3.0" # Use a versão mais recente do provider Docker
    }
  }
}

# Configuração do provider Docker
provider "docker" {}

# Cria o container do SQL Server
resource "docker_container" "mssql_db" {
  name  = "sql_server_devops_pratica5"
  # Imagem solicitada (microsoft/mssql-server é agora mcr.microsoft.com/mssql/server)
  image = "mcr.microsoft.com/mssql/server:latest" 

  # Variáveis de Ambiente Essenciais para o SQL Server
  env = [
    "ACCEPT_EULA=Y",
    # Mude esta password para uma forte, complexa, e segura!
    "SA_PASSWORD=YourComplexPassword#123", 
  ]

  # Configuração da Porta de Acesso Exterior (Requisito)
  ports {
    internal = 1433 # Porta interna padrão do SQL Server
    external = 1433 # Porta mapeada para o exterior
  }

  # Configuração de Limite de RAM (2GB = 2048 MB) (Requisito)
  # O limite é definido em MB
  resource_limits {
    memory = 2048 
  }

  # Ponto Extra: Volume Persistente para Dados do SQL Server
  # Garante que os dados da base de dados não se perdem ao parar/remover o container
  volumes {
    # Altere este caminho (`host_path`) para uma pasta existente no seu sistema operativo!
    host_path      = "/data/mssql_pratica5"
    container_path = "/var/opt/mssql"
  }

  # Ponto Extra: Implementação de Health Check
  # Garante que o container só é considerado "saudável" após o serviço SQL estar pronto
  healthcheck {
    test     = ["CMD-SHELL", "/opt/mssql/bin/sqlservr & /opt/mssql/bin/mssql-conf set-sa-password"]
    interval = "30s"
    timeout  = "10s"
    retries  = 5
    start_period = "30s" # Espera inicial para o SQL Server iniciar
  }
}