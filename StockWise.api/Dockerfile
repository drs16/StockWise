# Imagen base del SDK para compilar la aplicación
using System.Text;

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar los archivos del proyecto y restaurar dependencias
COPY ["StockWise.api/StockWise.api.csproj", "StockWise.api/"]
RUN dotnet restore "StockWise.api/StockWise.api.csproj"

# Copiar todo el código y compilar
COPY . .
WORKDIR "/src/StockWise.api"
RUN dotnet publish -c Release -o /app/publish

# Imagen final: solo el runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "StockWise.api.dll"]
# Imagen base del SDK para compilar la aplicación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar los archivos del proyecto y restaurar dependencias
COPY ["StockWise.api/StockWise.api.csproj", "StockWise.api/"]
RUN dotnet restore "StockWise.api/StockWise.api.csproj"

# Copiar todo el código y compilar
COPY . .
WORKDIR "/src/StockWise.api"
RUN dotnet publish -c Release -o /app/publish

# Imagen final: solo el runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "StockWise.api.dll"]
