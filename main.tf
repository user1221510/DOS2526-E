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
resource "docker_container" "sql_server" {
  name  = "sql_server"
  image = "mcr.microsoft.com/mssql/server:latest"

  # Variáveis de Ambiente Essenciais
  env = [
    "ACCEPT_EULA=Y",
    "SA_PASSWORD=GrupoE2526!",
  ]

  # Configuração da Porta de Acesso
  ports {
    internal = 1433
    external = 1433
  }

  # Configuração de Limite de RAM (2GB = 2048 MB)
  memory = 2048

  # Ponto Extra: Volume Persistente para Dados
  volumes {
    volume_name = "mssql_data"
    container_path = "/var/opt/mssql"
  }

  # Ponto Extra: Implementação de Health Check
  healthcheck {
    test = ["CMD-SHELL", "/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P GrupoE2526! -Q \"SELECT 1\" || exit 1"]
    interval = "30s"
    timeout = "10s"
    retries = 5
    start_period = "30s"
  }

  restart = "unless-stopped"
}

# Cria um volume para persistência de dados
resource "docker_volume" "mssql_data" {
  name = "mssql_data"
}

# Outputs úteis - CORRIGIDO
output "sql_server_details" {
  description = "Detalhes de conexão do SQL Server"
  value = {
    container_name = docker_container.sql_server.name
    connection_string = "Server=localhost,1433;Database=master;User Id=sa;Password=GrupoE2526!;TrustServerCertificate=true;"
  }
  sensitive = true
}

output "container_created" {
  description = "Confirmação de criação do container"
  value = "Container SQL Server 'sql_server' criado com sucesso!"
}