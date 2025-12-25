FROM mcr.microsoft.com/dotnet/sdk:9.0

WORKDIR /app

COPY ProductsAPI.csproj ./
RUN dotnet restore

COPY . .
# Especifica o projeto da API para não compilar os testes desnecessariamente
RUN dotnet publish ProductsAPI.csproj -c Release -o out

ENTRYPOINT ["dotnet", "out/ProductsAPI.dll"]