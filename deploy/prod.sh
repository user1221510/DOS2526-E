#!/bin/bash

set -e # Parar se houver erro



echo "A iniciar Deploy de PRODUÇÃO..."



# --- CONFIGURAÇÃO ---

CONTAINER_NAME="dos2526-api-prod"

IMAGE_NAME="dos2526-api:latest"

# Nome da rede criada pelo docker-compose

NETWORK_NAME="dos2526-e_default" 

DB_HOST="db-1" 



# 1. Parar versão antiga

echo "A limpar container antigo..."

docker stop $CONTAINER_NAME || true

docker rm $CONTAINER_NAME || true



# 2. Arrancar nova versão

echo "A arrancar novo container..."



docker run -d \

  --name $CONTAINER_NAME \

  --restart always \

  --network $NETWORK_NAME \

  -p 8050:8080 \

  -e "ASPNETCORE_ENVIRONMENT=Production" \

  -e "ConnectionStrings__DefaultConnection=Server=$DB_HOST;Database=ProductsDB;User Id=sa;Password=GrupoE2526!;TrustServerCertificate=True;" \

  $IMAGE_NAME



echo " Deploy PROD concluído com sucesso! (Porta 8050)"