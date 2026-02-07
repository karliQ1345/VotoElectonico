# Capa de compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Copiamos los archivos de proyecto (.csproj) de AMBOS proyectos
# Esto permite que Docker cachee las capas de restauración
COPY ["VotoElect.MVC/VotoElect.MVC.csproj", "VotoElect.MVC/"]
COPY ["VotoElectonico/VotoElectonico.csproj", "VotoElectonico/"]

# 2. Restauramos dependencias de toda la solución
RUN dotnet restore "VotoElect.MVC/VotoElect.MVC.csproj"

# 3. Copiamos todo el código fuente
COPY . .

# 4. Publicamos el MVC
WORKDIR "/src/VotoElect.MVC"
RUN dotnet publish "VotoElect.MVC.csproj" -c Release -o /app/publish

# Capa final
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 10000
ENV ASPNETCORE_URLS=http://0.0.0.0:10000
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "VotoElect.MVC.dll"]