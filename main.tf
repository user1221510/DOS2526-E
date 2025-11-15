# 1. Configurar o Provider Docker
terraform {
  required_providers {
    docker = {
      source = "kreuzwerker/docker"
      version = "~> 3.0.1"
    }
  }
}

provider "docker" {}

# 2. Configurar a Imagem do SQL Server
resource "docker_image" "mssql_server" {
  name = "microsoft/mssql-server:latest"
}

# 3. Configurar o Container
resource "docker_container" "mssql_db" {
  name  = "mssql-dev"
  image = docker_image.mssql_server.name
  
  # Variáveis de ambiente obrigatórias para o SQL Server
  env = [
    "ACCEPT_EULA=Y", 
    "SA_PASSWORD=YourStrongPassword!123" # Altere a password
  ]

  # 4. Configuração de Memória (2GB)
  memory = 2048 # em Megabytes

  # 5. Mapeamento de Porta (exemplo: 1433 local -> 1433 container)
  ports {
    internal = 1433
    external = 1433
  }
  
  # 6. Pontos Extra: Persistência (Exemplo de Bind Mount)
  # Certifique-se que a pasta 'data_sql' existe no seu host
  volumes {
    host_path      = "/caminho/para/sua/pasta/data_sql"
    container_path = "/var/opt/mssql"
  }
  
  # 7. Pontos Extra: Health Check
  healthcheck {
    test = ["CMD", "/opt/mssql-tools/bin/sqlcmd", "-S", "localhost", "-U", "sa", "-P", "YourStrongPassword!123", "-Q", "SELECT 1"]
    interval = "30s"
    timeout  = "5s"
    retries  = 5
    start_period = "30s"
  }

}