terraform {
  required_providers {
    docker = {
      source  = "kreuzwerker/docker"
      version = "~> 3.0"
    }
  }
}

provider "docker" {}

# Imagem MSSQL Server
resource "docker_image" "mssql" {
  name = "microsoft/mssql-server"
}

# Volume para persistência dos dados (ponto extra)
resource "docker_volume" "mssql_data" {
  name = "mssql_data"
}

# Container MSSQL Server
resource "docker_container" "mssql" {
  name  = "mssql_server"
  image = docker_image.mssql.latest

  # Porta exposta
  ports {
    internal = 1433
    external = 1433
  }

  # Variáveis obrigatórias do MSSQL server
  env = [
    "ACCEPT_EULA=Y",
    "SA_PASSWORD=Your_password123"
  ]

  # 2GB de RAM
  memory = 2048

  # Guardar dados numa pasta do host (ponto extra)
  mounts {
    target = "/var/opt/mssql"
    source = docker_volume.mssql_data.name
    type   = "volume"
  }

  # Health Check (ponto extra)
  healthcheck {
    test     = ["CMD", "/opt/mssql-tools/bin/sqlcmd", "-S", "localhost", "-U", "SA", "-P", "Your_password123", "-Q", "SELECT 1"]
    interval = "30s"
    timeout  = "10s"
    retries  = 5
  }
}
