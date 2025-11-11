# Imagen base del SDK para compilar la aplicación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto y restaurar dependencias
COPY ["StockWise.api/StockWise.api.csproj", "StockWise.api/"]
RUN dotnet restore "StockWise.api/StockWise.api.csproj"

# Copiar todo el código y compilar
COPY . .
WORKDIR "/src/StockWise.api"
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Imagen final más ligera para ejecutar la app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "StockWise.api.dll"]
