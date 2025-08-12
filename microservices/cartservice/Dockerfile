# Stage 1: Build the application
# Utiliza la imagen del SDK de .NET 8 para compilar el proyecto
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src
# Copia el archivo csproj del servicio de carrito
COPY ["microservices/cartservice/cartservice.csproj", "microservices/cartservice/"]
# Restaura las dependencias del proyecto
RUN dotnet restore "microservices/cartservice/cartservice.csproj"
# Copia todo el código fuente
COPY . .
# Navega al directorio del proyecto y compila en modo Release
WORKDIR "/src/microservices/cartservice"
RUN dotnet build "cartservice.csproj" -c Release -o /app/build

# Stage 2: Publish the application
# Utiliza la etapa de compilación para publicar solo los archivos necesarios
FROM build AS publish
RUN dotnet publish "cartservice.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: The final runtime image for Azure Functions
# Utiliza la imagen del runtime de Azure Functions para .NET 8 aisladas (isolated)
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:8.0-alpine
WORKDIR /home/site/wwwroot
# Establece las variables de entorno para el host de funciones
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true
# Copia los archivos publicados a la ubicación de ejecución
COPY --from=publish /app/publish .