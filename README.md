# notification-service

Backend de la plataforma de notificaciones multi-organización y multi-aplicación (WhatsApp, Email, SMS). Sigue Clean Architecture + Hexagonal Architecture y las decisiones documentadas en [docs/](docs/). El frontend (`notification-admin`) vive en otro repositorio.

> **Estado actual:** la Fase 2 de persistencia está implementada (entidades, `DbContext`, configuraciones EF Core, migraciones y repositorios). Esta tarea mantiene ese estado y añade configuración Docker Compose para operación local y despliegue en VPS, sin iniciar nuevas fases funcionales.

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
- Docker Engine + Docker Compose plugin.
- PostgreSQL es requerido por Api/Worker/Scheduler; la cadena de conexión es **obligatoria** y se valida al arrancar.

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

Inicializar `.env` local:

```bash
cp .env.example .env
```

PowerShell:

```powershell
Copy-Item .env.example .env
```

`.env` está ignorado por Git. En producción, usar secretos de Docker/Portainer (o equivalente), no el `.env` local.

## Comandos .NET

```bash
dotnet restore NotificationService.sln
dotnet build NotificationService.sln --configuration Release
dotnet test NotificationService.sln --configuration Release

dotnet list NotificationService.sln package --include-transitive
dotnet list NotificationService.sln package --outdated
dotnet list NotificationService.sln package --vulnerable --include-transitive
```

Las pruebas incluyen cobertura de persistencia con PostgreSQL real (Testcontainers), por lo que Docker debe estar disponible para el conjunto completo.

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

Worker y Scheduler no exponen endpoints HTTP de healthcheck en la implementación actual (TP-05 sigue pendiente). CI y publicación en GHCR con GitHub Actions están aprobadas pero sin workflows todavía (TP-07: [ci-cd.md](docs/deployment/ci-cd.md)).

Traefik es externo y nunca forma parte de este repositorio ([ADR-0010](docs/adr/0010-traefik.md)).

### Compose

Archivos:

- `compose.yaml` (base)
- `compose.dev.yaml` (desarrollo local, build desde Dockerfiles)
- `compose.prod.yaml` (producción, imágenes GHCR)

Validar configuración:

```bash
docker compose -f compose.yaml config
docker compose -f compose.yaml -f compose.dev.yaml config
docker compose -f compose.yaml -f compose.prod.yaml config
```

Levantar entorno completo en desarrollo:

```bash
docker compose -f compose.yaml -f compose.dev.yaml up -d
```

Levantar solo infraestructura (útil cuando ejecutas hosts desde el IDE):

```bash
docker compose -f compose.yaml -f compose.dev.yaml up -d postgres rabbitmq redis minio
```

Parar servicios sin eliminar volúmenes:

```bash
docker compose -f compose.yaml -f compose.dev.yaml down
```

Ver logs:

```bash
docker compose -f compose.yaml -f compose.dev.yaml logs -f notification-api
docker compose -f compose.yaml -f compose.dev.yaml logs -f notification-worker
docker compose -f compose.yaml -f compose.dev.yaml logs -f notification-scheduler
```

Despliegue en VPS (sin build local):

```bash
docker compose -f compose.yaml -f compose.prod.yaml pull
docker compose -f compose.yaml -f compose.prod.yaml up -d
```

Notas operativas:

- No publicar puertos de PostgreSQL/RabbitMQ/Redis/MinIO en producción.
- Ejecutar migraciones EF Core como tarea controlada de release, nunca automáticamente por réplica.
- No usar `docker compose down -v` en entornos con datos que deban preservarse.

### Backups y restauración

- PostgreSQL: backup lógico (`pg_dump`) y restauración (`pg_restore`/`psql`) como proceso de release.
- MinIO: respaldo del volumen de objetos + validación de restauración.
- RabbitMQ: respaldo de definiciones y datos de colas si aplica a tu RPO/RTO.
- Redis: confirmar si AOF/RDB forma parte de la estrategia de recuperación o se trata como caché reconstruible.

Ejemplo de backup lógico PostgreSQL (desde el contenedor):

```bash
docker compose -f compose.yaml -f compose.dev.yaml exec -T postgres \
  pg_dump -U "$POSTGRES_USER" -d "$POSTGRES_DB" > backup-notification-service.sql
```

Ejemplo de restauración PostgreSQL:

```bash
cat backup-notification-service.sql | docker compose -f compose.yaml -f compose.dev.yaml exec -T postgres \
  psql -U "$POSTGRES_USER" -d "$POSTGRES_DB"
```

Ejemplo de backup de datos MinIO (tar del volumen montado):

```bash
docker run --rm \
  -v notification_minio_data:/data:ro \
  -v "$PWD":/backup \
  alpine:3.22 sh -c 'tar czf /backup/minio-data-backup.tgz -C /data .'
```

## Decisiones y pendientes

- Decisiones aprobadas: [docs/adr/](docs/adr/), [DECISIONS-v1.5.md](docs/project/DECISIONS-v1.5.md).
- Historial de las decisiones PD-01 a PD-08 (todas aprobadas): [PENDING-DECISIONS.md](docs/history/PENDING-DECISIONS.md).
- Catálogo de códigos de error de la API: [error-codes.md](docs/api/error-codes.md).
- Contrato HTTP v1: [openapi.yaml](docs/api/openapi.yaml) es la **fuente normativa única** ([api-design.md](docs/api/api-design.md#fuente-de-verdad)).
- Bootstrap de `super_admin`: [super-admin-bootstrap.md](docs/operations/super-admin-bootstrap.md).
- **Registro único de pendientes:** [technical-pending-register.md](docs/operations/technical-pending-register.md).
- Índice completo (normativos e históricos): [notification-architecture-index.md](docs/project/notification-architecture-index.md). Los informes de auditoría y el historial de decisiones están en [docs/history/](docs/history/README.md) y son solo trazabilidad.
