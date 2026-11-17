# notification-service

Backend de la plataforma de notificaciones multi-organización y multi-aplicación (WhatsApp, Email, SMS). Sigue Clean Architecture + Hexagonal Architecture y las decisiones documentadas en [docs/](docs/). El frontend (`notification-admin`) vive en otro repositorio.

> **Estado actual:** fundación técnica saneada. **No hay funcionalidad de negocio**: sin entidades, persistencia, autenticación, mensajería, proveedores ni endpoints funcionales. La Fase 2 (persistencia y modelo de datos) **no está autorizada**. Ver [repository-sanitization-report.md](docs/operations/repository-sanitization-report.md).

Instrucciones para agentes de desarrollo: [AGENTS.md](AGENTS.md) (y [CLAUDE.md](CLAUDE.md) para Claude Code).

## Estructura de la solución

```text
src/
  NotificationService.Domain/          Núcleo: sin dependencias
  NotificationService.Application/     Casos de uso y puertos
  NotificationService.Infrastructure/  Adaptadores de salida (EF Core/Npgsql, ...)
  NotificationService.Api/             Adaptador HTTP (composition root)
  NotificationService.Worker/          Adaptador RabbitMQ (composition root; aún solo heartbeat)
  NotificationService.Scheduler/       Adaptador de planificación (composition root; aún solo heartbeat)
tests/
  NotificationService.{Domain,Application,Infrastructure,Api,Worker,Architecture}.Tests/
```

Detalle y reglas de dependencia: [docs/architecture/project-structure.md](docs/architecture/project-structure.md).

## Requisitos

- .NET SDK **10.0.404** (fijado en [global.json](global.json), `rollForward: latestPatch`).
- Docker (opcional; solo para construir imágenes).
- PostgreSQL solo será necesario a partir de la Fase 2. Hoy ningún endpoint abre conexiones, pero la cadena de conexión es **obligatoria** y se valida al arrancar.

Las versiones de los paquetes NuGet se gestionan de forma centralizada en [Directory.Packages.props](Directory.Packages.props) (EF Core 10.0.15, Npgsql.EntityFrameworkCore.PostgreSQL 10.0.3).

## Configuración

| Clave                                 | Variable de entorno                    | Uso                                                                  |
| ------------------------------------- | -------------------------------------- | -------------------------------------------------------------------- |
| `ConnectionStrings:DefaultConnection` | `ConnectionStrings__DefaultConnection` | PostgreSQL (Api, Worker, Scheduler). Obligatoria; única convención   |
| `OpenApi:Enabled`                     | `OpenApi__Enabled`                     | Expone OpenAPI/Swagger UI fuera de Development (por defecto `false`) |

Los `appsettings*.json` **no** contienen credenciales. Para desarrollo local, usar User Secrets: los tres hosts comparten el mismo `UserSecretsId`, así que basta configurarlo una vez:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=notification_service;Username=<user>;Password=<password>" --project src/NotificationService.Api
```

Alternativa: variables de entorno (ver [.env.example](.env.example), que solo contiene placeholders). Nunca versionar `.env` ni secretos reales.

## Comandos

```bash
dotnet restore NotificationService.sln
dotnet build NotificationService.sln --configuration Release
dotnet test NotificationService.sln --configuration Release

dotnet list NotificationService.sln package --include-transitive
dotnet list NotificationService.sln package --outdated
dotnet list NotificationService.sln package --vulnerable --include-transitive
```

Las pruebas actuales (unitarias, de composición y de arquitectura) no requieren Docker. A partir de la Fase 2, las pruebas de integración de persistencia usarán **Testcontainers con PostgreSQL 17** y necesitarán Docker en ejecución ([integration-testing-postgresql.md](docs/testing/integration-testing-postgresql.md)).

Ejecutar hosts (requieren la cadena de conexión configurada):

```bash
dotnet run --project src/NotificationService.Api
dotnet run --project src/NotificationService.Worker
dotnet run --project src/NotificationService.Scheduler
```

## Endpoints técnicos de la API

| Ruta                   | Descripción                                                                       |
| ---------------------- | --------------------------------------------------------------------------------- |
| `GET /`                | Identificación del servicio                                                       |
| `GET /health/live`     | Liveness: el proceso está en ejecución                                            |
| `GET /health/ready`    | Readiness: comprobaciones de dependencias (aún no hay dependencias implementadas) |
| `GET /openapi/v1.json` | Documento OpenAPI 3.1 generado (Development, o `OpenApi:Enabled=true`)            |
| `GET /swagger`         | Swagger UI (mismas condiciones)                                                   |

El contrato normativo de la API es [docs/api/openapi.yaml](docs/api/openapi.yaml). El documento generado describe solo lo implementado.

Los errores no controlados se devuelven como `application/problem+json` (RFC 9457), con `code` y `traceId` y sin detalles internos.

## Docker

Una imagen independiente por host (decisión PD-07): `notification-api`, `notification-worker`, `notification-scheduler`. Multi-stage, Release, usuario no-root y SDK fijado por `global.json`; se publican en GHCR. Se construyen desde la raíz del repositorio:

```bash
docker build -f src/NotificationService.Api/Dockerfile -t notification-api:dev .
docker build -f src/NotificationService.Worker/Dockerfile -t notification-worker:dev .
docker build -f src/NotificationService.Scheduler/Dockerfile -t notification-scheduler:dev .
```

Worker y Scheduler expondrán `/health/live` y `/health/ready` internos (TP-05, aprobado; no implementado: [health-checks.md](docs/deployment/health-checks.md)). CI y publicación en GHCR con GitHub Actions están aprobadas pero sin workflows todavía (TP-07: [ci-cd.md](docs/deployment/ci-cd.md)).

Aún no hay Docker Compose. Traefik es externo y nunca forma parte de este repositorio ([ADR-0010](docs/adr/0010-traefik.md)).

## Decisiones y pendientes

- Decisiones aprobadas: [docs/adr/](docs/adr/), [DECISIONS-v1.5.md](docs/project/DECISIONS-v1.5.md).
- Historial de las decisiones PD-01 a PD-08 (todas aprobadas): [PENDING-DECISIONS.md](docs/project/PENDING-DECISIONS.md).
- Catálogo de códigos de error de la API: [error-codes.md](docs/api/error-codes.md).
- Contrato HTTP v1: [openapi.yaml](docs/api/openapi.yaml) es la **fuente normativa única** ([api-design.md](docs/api/api-design.md#fuente-de-verdad)).
- Bootstrap de `super_admin`: [super-admin-bootstrap.md](docs/operations/super-admin-bootstrap.md).
- **Registro único de pendientes:** [technical-pending-register.md](docs/operations/technical-pending-register.md).
- Índice completo (normativos e históricos): [notification-architecture-index.md](docs/project/notification-architecture-index.md). Los informes de `docs/operations/*report*.md` son históricos.
