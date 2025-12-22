#!/bin/bash

# 1. Ler o argumento que vem do Jenkins
IMAGE_NAME=$1

# Se o Jenkins não enviar nada, usa um valor por defeito para não dar erro
if [ -z "$IMAGE_NAME" ]; then
    IMAGE_NAME="dos2526-api:latest"
fi

CONTAINER_NAME="dos2526-api-prod"
HOST_PORT="8055"

echo "A iniciar Deploy de PRODUÇÃO..."
echo "Imagem a utilizar: $IMAGE_NAME"

echo "A limpar container antigo..."
# O "|| true" impede que o script falhe se o container não existir
docker stop $CONTAINER_NAME || true
docker rm $CONTAINER_NAME || true

echo "A arrancar novo container..."

docker run -d --restart unless-stopped -p $HOST_PORT:8080 --name $CONTAINER_NAME $IMAGE_NAME

echo "Deploy concluído com sucesso!"