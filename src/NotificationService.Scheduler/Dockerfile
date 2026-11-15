# syntax=docker/dockerfile:1
# Imagen de NotificationService.Scheduler.
# Contexto de build: raíz del repositorio.
#   docker build -f src/NotificationService.Scheduler/Dockerfile -t notification-scheduler:dev .
# No contiene secretos: la configuración sensible se inyecta en ejecución
# (p.ej. ConnectionStrings__DefaultConnection desde secretos de Docker/Portainer).

# SDK fijado a la versión exacta de global.json (10.0.404).
FROM mcr.microsoft.com/dotnet/sdk:10.0.404 AS build
WORKDIR /src

# 1) Restore cacheable: solo archivos de proyecto y configuración de build.
COPY global.json Directory.Build.props Directory.Packages.props ./
COPY src/NotificationService.Domain/NotificationService.Domain.csproj src/NotificationService.Domain/
COPY src/NotificationService.Application/NotificationService.Application.csproj src/NotificationService.Application/
COPY src/NotificationService.Infrastructure/NotificationService.Infrastructure.csproj src/NotificationService.Infrastructure/
COPY src/NotificationService.Scheduler/NotificationService.Scheduler.csproj src/NotificationService.Scheduler/
RUN dotnet restore src/NotificationService.Scheduler/NotificationService.Scheduler.csproj

# 2) Código fuente (bin/, obj/, tests/, docs/ y .env* excluidos por .dockerignore).
COPY src/ src/
RUN dotnet publish src/NotificationService.Scheduler/NotificationService.Scheduler.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

# Runtime: en producción fijar tag/digest exacto en el manifiesto de versiones (docs/deployment/docker.md).
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
# Usuario no-root incluido en las imágenes oficiales de .NET (UID 1654).
USER $APP_UID
ENTRYPOINT ["dotnet", "NotificationService.Scheduler.dll"]
