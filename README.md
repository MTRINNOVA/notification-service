# notification-service

Fundacion tecnica inicial para notification-service, siguiendo Clean Architecture + Hexagonal Architecture y las decisiones documentadas en docs.

## Estructura de la solucion

```text
src/
	NotificationService.Domain/
	NotificationService.Application/
	NotificationService.Infrastructure/
	NotificationService.Api/
	NotificationService.Worker/
	NotificationService.Scheduler/
tests/
	NotificationService.Domain.Tests/
	NotificationService.Application.Tests/
	NotificationService.Infrastructure.Tests/
	NotificationService.Api.Tests/
	NotificationService.Worker.Tests/
	NotificationService.Architecture.Tests/
```

## Requisitos

- .NET SDK 10.0.404 o compatible.

## Configuracion de desarrollo

Cada host tiene configuracion base con la seccion Database:

```json
{
  "Database": {
    "ConnectionString": "Host=localhost;Port=5432;Database=notification_service;Username=postgres;Password=postgres"
  }
}
```

No almacenar secretos reales en el repositorio.

## Comandos

Restaurar y compilar:

```bash
dotnet restore NotificationService.sln
dotnet build NotificationService.sln
```

Ejecutar pruebas:

```bash
dotnet test NotificationService.sln
```

Ejecutar hosts:

```bash
dotnet run --project src/NotificationService.Api
dotnet run --project src/NotificationService.Worker
dotnet run --project src/NotificationService.Scheduler
```

## Endpoints base de API

- GET /
- GET /health
- Swagger UI: /swagger

## Estado actual

- Solucion y proyectos base creados.
- Referencias entre proyectos segun arquitectura aprobada.
- Dependency Injection base por capas.
- Logging estructurado JSON.
- Manejo global de excepciones en API.
- Health checks basicos.
- OpenAPI/Swagger habilitado.
- Dockerfile de desarrollo para API.

No se implemento logica funcional de notificaciones, proveedores ni campanas.
