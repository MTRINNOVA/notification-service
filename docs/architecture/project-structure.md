# Estructura de solución

```text
src/
  NotificationService.Domain/
    Common/ Entities/ ValueObjects/ Enums/ Events/ Exceptions/
  NotificationService.Application/
    Abstractions/ Features/ Behaviors/ Mappings/
  NotificationService.Infrastructure/
    Persistence/ Messaging/RabbitMQ/ Cache/ Storage/Providers/Security/
  NotificationService.Api/
    Endpoints/Public/ Management/ Webhooks/ Authentication/ Authorization/
  NotificationService.Worker/
    Consumers/ Handlers/ Retry/ DeadLetter/
  NotificationService.Scheduler/
    Jobs/ Services/
tests/
  Domain.Tests/
  Application.Tests/
  Infrastructure.Tests/
  Api.Tests/
  Worker.Tests/
  Architecture.Tests/
docs/
deploy/
```

## Dependencias permitidas

- Domain: ninguna dependencia de proyectos internos/infraestructura.
- Application -> Domain.
- Infrastructure -> Application + Domain.
- Api -> Application + Infrastructure (solo composition root).
- Worker -> Application + Infrastructure (solo composition root).
- Scheduler -> Application + Infrastructure (solo composition root).
- Tests referencian el proyecto bajo prueba y dobles de test.

## Convenciones

- DTO HTTP separados de comandos/entidades.
- Async/await y CancellationToken en I/O.
- Nullable reference types habilitado.
- Analyzers y formato consistente.
- No lógica de negocio en controllers/endpoints.
- Migraciones EF Core pertenecen a Infrastructure.
