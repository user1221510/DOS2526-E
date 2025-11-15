# Define o provider (ferramenta) que o Terraform vai usar: Docker
terraform {
  required_providers {
    docker = {
      source  = "kreuzwerker/docker"
      version = "~> 3.0"
    }
  }
}

# Configura o Provider Docker
provider "docker" {}

# Cria o container do SQL Server
resource "docker_container" "mssql_server_pratica5" {
  name  = "sql_server_devops_pratica5"
  # A imagem moderna é esta, substituindo a anterior 'microsoft/mssql-server'
  image = "mcr.microsoft.com/mssql/server:latest" 

  # Variáveis de Ambiente Essenciais
  env = [
    "ACCEPT_EULA=Y",
    "SA_PASSWORD=YourSecurePassword#123", # Mude esta password!
  ]

  # Configuração da Porta de Acesso (Requisito)
  ports {
    internal = 1433 # Porta do container
    external = 1433 # Porta do seu computador
  }

  # Configuração de Limite de RAM (2GB = 2048 MB) (Requisito)
  resource_limits {
    memory = 2048 
  }

  # Ponto Extra: Volume Persistente para Dados (Requisito)
  volumes {
    # ATENÇÃO: Altere o 'host_path' para um caminho real e válido no seu sistema operativo!
    host_path      = "/data/mssql_pratica5"
    container_path = "/var/opt/mssql" # Caminho interno padrão do SQL Server no Linux
  }

  # Ponto Extra: Implementação de Health Check (Requisito)
  # Verifica se o serviço SQL Server está operacional
  healthcheck {
    test     = ["CMD-SHELL", "/opt/mssql/bin/sqlservr & /opt/mssql/bin/mssql-conf set-sa-password"]
    interval = "30s"
    timeout  = "10s"
    retries  = 5
  }
}