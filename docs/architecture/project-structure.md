# Estructura de solución

```text
src/
  NotificationService.Domain/
    Common/ Entities/ ValueObjects/ Enums/ Events/ Exceptions/
  NotificationService.Application/
    Abstractions/ Features/ Behaviors/ Mappings/
  NotificationService.Infrastructure/
    Common/ Configuration/ Persistence/ Messaging/RabbitMQ/ Cache/ Storage/ Providers/ Security/ Time/
  NotificationService.Api/                 (Dockerfile)
    Common/ Middleware/ Endpoints/{Public,Management,Webhooks}/ Authentication/ Authorization/
  NotificationService.Worker/              (Dockerfile)
    Common/ Consumers/ Handlers/ Retry/ DeadLetter/
  NotificationService.Scheduler/           (Dockerfile)
    Common/ Jobs/ Services/
tests/
  NotificationService.Domain.Tests/
  NotificationService.Application.Tests/
  NotificationService.Infrastructure.Tests/
  NotificationService.Api.Tests/
  NotificationService.Worker.Tests/
  NotificationService.Architecture.Tests/
docs/          (incluye docs/testing/ con la estrategia de pruebas de integración)
deploy/        (previsto para Compose/manifiestos; aún no creado)
```

Cada host (`Api`, `Worker`, `Scheduler`) tiene su Dockerfile y genera una imagen independiente: `notification-api`, `notification-worker` y `notification-scheduler` (PD-07, aprobada; ver [docker.md](../deployment/docker.md)).

Archivos de raíz: `NotificationService.sln`, `global.json` (SDK), `Directory.Build.props` (propiedades comunes), `Directory.Packages.props` (versiones NuGet centralizadas), `.env.example`, `.dockerignore`.

No existe `NotificationService.Scheduler.Tests`. Las pruebas de composición del Scheduler están temporalmente en `Architecture.Tests` (TP-06 en [technical-pending-register.md](../operations/technical-pending-register.md)).

## Dependencias permitidas

- Domain: ninguna dependencia de proyectos internos/infraestructura.
- Application -> Domain.
- Infrastructure -> Application + Domain.
- Api -> Application + Infrastructure (solo composition root).
- Worker -> Application + Infrastructure (solo composition root).
- Scheduler -> Application + Infrastructure (solo composition root).
- Tests referencian el proyecto bajo prueba y dobles de test.

Estas reglas se verifican automáticamente en `NotificationService.Architecture.Tests` (tipos con NetArchTest y referencias declaradas en los `.csproj`).

## Convenciones

- DTO HTTP separados de comandos/entidades.
- Async/await y CancellationToken en I/O.
- Nullable reference types habilitado.
- Analyzers y formato consistente.
- No lógica de negocio en controllers/endpoints.
- Migraciones EF Core pertenecen a Infrastructure.
- Versiones de paquetes solo en `Directory.Packages.props`; los `.csproj` no declaran `Version`.
- Composition root de Worker/Scheduler en `Common/*HostBuilderExtensions.cs`; `Program.cs` solo construye y ejecuta el host.
