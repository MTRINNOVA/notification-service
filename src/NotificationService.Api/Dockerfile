FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["src/NotificationService.Domain/NotificationService.Domain.csproj", "src/NotificationService.Domain/"]
COPY ["src/NotificationService.Application/NotificationService.Application.csproj", "src/NotificationService.Application/"]
COPY ["src/NotificationService.Infrastructure/NotificationService.Infrastructure.csproj", "src/NotificationService.Infrastructure/"]
COPY ["src/NotificationService.Api/NotificationService.Api.csproj", "src/NotificationService.Api/"]
RUN dotnet restore "src/NotificationService.Api/NotificationService.Api.csproj"

COPY . .
RUN dotnet publish "src/NotificationService.Api/NotificationService.Api.csproj" -c Debug -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NotificationService.Api.dll"]