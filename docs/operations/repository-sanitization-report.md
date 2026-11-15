# Informe de saneamiento del repositorio — 2026-11-15

> **Documento histórico** (evidencia de la auditoría del 2026-11-15). No es fuente operativa: el estado vigente de los pendientes está en [technical-pending-register.md](technical-pending-register.md) y las decisiones en [DECISIONS-v1.5.md](../project/DECISIONS-v1.5.md).

**Alcance:** saneamiento técnico y documental de la fundación, previo a la Fase 2. **No se implementó la Fase 2** ni ninguna funcionalidad de negocio: sin entidades, `DbSet`, Fluent API, repositorios, migraciones, autenticación, RabbitMQ, Redis, MinIO, proveedores ni endpoints funcionales. No se modificaron ADR, `DECISIONS-v1.5.md` ni `openapi.yaml`. No se hicieron commits ni push.

## 1. Estado inicial

- Solución con 6 proyectos `src/` y 6 de `tests/`, todos `net10.0`. SDK `10.0.404` fijado en `global.json`.
- EF Core 10.0.15 / Npgsql.EntityFrameworkCore.PostgreSQL 10.0.3, tras la corrección de la tarea anterior.
- Versiones NuGet declaradas en cada `.csproj` (sin gestión centralizada) y parches mezclados en Microsoft.Extensions (10.0.0 / 10.0.15).
- 9 pruebas, casi todas comprobaban solo que existía un *assembly marker*.
- Último commit: `e25b08d`. `.gitignore` contenía solo `bin/` y `obj/`; no había artefactos ni secretos versionados.

## 2. Problemas detectados (auditoría inicial)

| # | Problema | Archivo | Impacto técnico | Impacto funcional | Corrección | ¿Aprobación? |
|---|---|---|---|---|---|---|
| 1 | Sin gestión centralizada de paquetes; parches mezclados | `*.csproj` | Riesgo de deriva de versiones | — | `Directory.Packages.props` | No (solicitado) |
| 2 | Clave de conexión `Database:ConnectionString` frente a `ConnectionStrings__DefaultConnection` documentada | Infrastructure, appsettings, `compose-topology.md` | Dos convenciones; despliegue roto con la variable documentada | — | Unificar en `ConnectionStrings:DefaultConnection` | No (preferencia del usuario y ya documentada) |
| 3 | Credenciales predeterminadas `postgres/postgres` en los `appsettings.json` de los 3 hosts | appsettings | Arranque "silencioso" con credenciales débiles | Riesgo de usarlas en producción | Eliminar; User Secrets/entorno; validación al iniciar | No |
| 4 | `.gitignore` mínimo | `.gitignore` | `.env`, secretos, IDE y logs versionables | Riesgo de fuga de secretos | Ampliar | No |
| 5 | Swashbuckle (generador + UI) y `Microsoft.AspNetCore.OpenApi` (sin usar) a la vez | `Api.csproj`, `Program.cs` | Dependencia redundante; documento no 3.1 | — | OpenAPI 3.1 nativo + solo Swagger UI | No (estrategia delegada) |
| 6 | Swagger expuesto en todos los entornos | `Program.cs` | Superficie pública innecesaria | — | Solo en Development o con `OpenApi:Enabled=true` | No |
| 7 | El middleware responde `application/json`: `WriteAsJsonAsync` sobrescribe el content type | `GlobalExceptionMiddleware.cs` | Incumple RFC 9457 y la convención de `api-design.md` | Clientes no reconocen Problem Details | Pasar el content type explícito | No |
| 8 | El middleware no incluye `code` (obligatorio en el esquema `ProblemDetails` de OpenAPI), no controla `HasStarted` y convierte cancelaciones del cliente en 500 | `GlobalExceptionMiddleware.cs` | Excepción secundaria al escribir sobre una respuesta iniciada; logs de error falsos | Respuestas que no cumplen el contrato | Corregido | No |
| 9 | `infrastructure-self` siempre *Healthy*; sin separación liveness/readiness | `DependencyInjection.cs`, `Program.cs` | Declara sana una dependencia no comprobada | Orquestador engañado | `/health/live` y `/health/ready`; eliminar check falso | No |
| 10 | Dockerfile en `Debug`, como root, sin `global.json` ni `.dockerignore` | `Api/Dockerfile` | Imagen no apta; `COPY . .` puede incluir `.env`, `bin/`, `obj/` | — | Reescrito; Dockerfiles base para Worker/Scheduler | Estrategia de imágenes: sí (PD-07) |
| 11 | `IClock` registrado como `Scoped` | `DependencyInjection.cs` | Servicio sin estado con ciclo de vida innecesariamente corto (no inyectable en singletons) | — | `Singleton` | No |
| 12 | `Nullable`/`ImplicitUsings` duplicados en cada `.csproj` y en `Directory.Build.props` | `*.csproj` | Ruido; deriva posible | — | Dejar solo en `Directory.Build.props` | No |
| 13 | `NotificationService.Api.http` apunta a `/weatherforecast` (plantilla) | `.http` | Archivo engañoso | — | Endpoints reales | No |
| 14 | Composición de Worker/Scheduler en `Program.cs`, no testeable | `Program.cs` | Sin pruebas de carga del host | — | `Common/*HostBuilderExtensions.cs` | No |
| 15 | Pruebas de fundación casi vacías; las reglas NetArchTest pasan trivialmente con ensamblados vacíos | `tests/` | Falsa sensación de protección | — | Pruebas de DI, configuración, hosts, logging, middleware y reglas sobre `.csproj` | No |
| 16 | Roles: `org_admin` (RBAC) frente a `organization_admin` + `application_admin` (ADR-0011); `super_admin` sin almacenamiento en el ERD | `rbac-matrix.md`, ADR-0011, `erd.md` | Modelo de identidad indefinido | Permisos y seed | Propuesta PD-01 | **Sí** |
| 17 | Estados: el flujo único no contempla el resultado ambiguo `UNKNOWN/RECONCILIATION_REQUIRED` | `rabbitmq-outbox.md`, `provider-contracts-v1.5.md` | Enums y columnas indefinidos | Estados públicos | Propuesta PD-02 | **Sí** |
| 18 | `INotificationProvider` universal frente a `IWhatsAppProvider` + `IWhatsAppConnectionManager` | `hexagonal.md`, ADR-0004 | Contrato de adaptadores indefinido | — | Propuesta PD-03 | **Sí** |
| 19 | 422 "si se adopta" | `api-design.md` | — | Códigos de error para clientes | Propuesta PD-04 | **Sí** |
| 20 | 409/429 exigidos por la documentación pero ausentes en las 93 operaciones de `openapi.yaml` | `openapi.yaml` | Contrato incompleto | Idempotencia y cuotas sin contrato | Propuesta PD-08 (OpenAPI no modificado) | **Sí** |
| 21 | Política de `timezone_override` no definida; API en UTC frente a "hora local" en los criterios de aceptación | `erd.md`, `api-design.md`, `criteria.md` | Columna dudosa | Programación y cuotas | Propuesta PD-05 | **Sí** |
| 22 | Subdominios distintos en `docker.md` y `environment-decisions.md` | Despliegue | — | — | Variables `NOTIFICATIONS_*_HOST`; DNS pendiente (PD-06) | Nombres reales: sí |
| 23 | `project-structure.md` desactualizado (`Storage/Providers/Security/` unidos, nombres de tests, `deploy/` inexistente) | `project-structure.md` | Instrucciones incorrectas para agentes | — | Actualizado | No |
| 24 | El índice citaba `DECISIONS-2026-11.md` y `CHANGELOG-v1.4.md`, que no existen | Índice | — | — | Ya corregido por el usuario antes de este saneamiento; se añadieron las nuevas referencias | No |
| 25 | No existe `Scheduler.Tests` | `tests/` | Pruebas del Scheduler sin proyecto propio | — | Temporalmente en `Architecture.Tests` (TP-06) | **Sí** (nuevo proyecto) |

## 3. Cambios realizados

### Paquetes (Central Package Management)

| Paquete | Antes | Después | Motivo |
|---|---|---|---|
| Microsoft.EntityFrameworkCore / .Relational | 10.0.15 | 10.0.15 | Sin cambio (ya coherente) |
| Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.3 | 10.0.3 | Sin cambio; última estable 10.x; requiere EF Core [10.0.4, 11) |
| Npgsql (transitivo) | 10.0.3 | 10.0.3 | Sin cambio |
| Microsoft.Extensions.Options.ConfigurationExtensions | 10.0.0 | **10.0.15** | Alinear con el resto de Microsoft.Extensions 10.0.15 |
| Microsoft.Extensions.Options.DataAnnotations | 10.0.0 | **Eliminado** | Sustituido por `IValidateOptions<DatabaseOptions>` |
| Swashbuckle.AspNetCore | 10.0.0 | **Eliminado** | Generador redundante con `Microsoft.AspNetCore.OpenApi` |
| Swashbuckle.AspNetCore.SwaggerUI | — | **10.0.0** | Solo la UI, misma versión que la familia anterior |
| Microsoft.AspNetCore.OpenApi | 10.0.15 (sin uso) | 10.0.15 (en uso) | Documento OpenAPI 3.1 nativo |
| Microsoft.AspNetCore.Mvc.Testing | — | **10.0.15** (solo Api.Tests) | Pruebas de arranque del host API |
| Microsoft.Extensions.DependencyInjection | — | **10.0.15** (solo Application.Tests) | Construir y validar el contenedor DI |
| Microsoft.Extensions.Hosting, DI.Abstractions, Diagnostics.HealthChecks | 10.0.15 | 10.0.15 | Sin cambio |
| xunit 2.9.3, xunit.runner.visualstudio 3.1.4, Microsoft.NET.Test.Sdk 17.14.1, coverlet.collector 6.0.4, NetArchTest.Rules 1.3.2 | — | Sin cambio | Funcionan en net10.0; las nuevas versiones mayores se evaluarán aparte (TP-10) |
| dotnet-ef / EF Core Design | No instalados | No instalados | No necesarios hasta la Fase 2 (TP-03) |

Las versiones 11.x existen en NuGet, pero corresponden a .NET 11. ADR-0008 fija .NET 10 LTS, así que no se adoptan.

### Configuración

- Única clave `ConnectionStrings:DefaultConnection` (`ConnectionStrings__DefaultConnection`). Se lee en `DatabaseOptions` y la valida al iniciar `DatabaseOptionsValidator`: no vacía, parseable por Npgsql, con `Host` y `Database`. Los mensajes de error **nunca** incluyen el valor.
- Sin credenciales en `appsettings*.json`. Los tres hosts comparten un `UserSecretsId` para desarrollo local.
- Mismo `LogLevel` en los tres hosts, con `Microsoft.EntityFrameworkCore` en `Warning` para no registrar SQL ni parámetros.
- `.gitignore` ampliado (build, cobertura, `.env*` salvo `.env.example`, `secrets.json`, certificados, IDE, temporales y logs). `.env.example` solo con placeholders.

### API y hosts

- OpenAPI 3.1 con `Microsoft.AspNetCore.OpenApi` en `/openapi/v1.json` y Swagger UI en `/swagger`. Solo en Development o con `OpenApi:Enabled=true`.
- `GlobalExceptionMiddleware` corregido:
  - responde `application/problem+json` con `status`, `title`, `detail`, `instance`, `code=INTERNAL_SERVER_ERROR` y `traceId` (W3C si hay `Activity`);
  - no expone el mensaje ni el tipo de la excepción;
  - si la respuesta ya empezó, registra el error y relanza;
  - ignora las cancelaciones solicitadas por el cliente (log `Information`);
  - escribe con el `CancellationToken` de la solicitud.
- Logging JSON homogéneo en Api, Worker y Scheduler: solo consola JSON, timestamps UTC y scopes.
- Health: `/health/live` (check `self`, etiqueta `live`) y `/health/ready` (etiqueta `ready`; hoy sin checks, porque no hay dependencias implementadas). Se eliminó el check falso `infrastructure-self` y la ruta `/health`.
- Worker y Scheduler componen su host en `Common/*HostBuilderExtensions.cs`.

### Docker

- Dockerfiles multi-stage para Api, Worker y Scheduler:
  - SDK `10.0.404` (igual a `global.json`) y restore cacheable;
  - copian `global.json`, `Directory.Build.props` y `Directory.Packages.props`;
  - publican en Release con `--no-restore`;
  - runtime `aspnet:10.0` para la API y `runtime:10.0` para Worker/Scheduler (verificado: sus `runtimeconfig.json` solo requieren `Microsoft.NETCore.App`);
  - ejecutan con usuario no-root (`$APP_UID`) y no incluyen secretos.
- `.dockerignore`: excluye `bin/`, `obj/`, `tests/`, `docs/`, `.git/`, `.env*`, `secrets.json` y archivos de IDE.
- Sin Docker Compose y sin Traefik.

### Pruebas (de 9 a 63)

| Proyecto | Pruebas | Qué verifican |
|---|---:|---|
| Domain.Tests | 1 | Marker (sin dominio todavía) |
| Application.Tests | 2 | Registro DI y contenedor válido (`ValidateOnBuild`) |
| Infrastructure.Tests | 15 | Opciones obligatorias y validación al iniciar, 7 casos inválidos, clave antigua rechazada, mensajes sin secretos, DbContext con Npgsql sin abrir conexión, `IClock` singleton UTC, ausencia de health checks no verificados |
| Api.Tests | 14 | Arranque del host, liveness/readiness, OpenAPI 3.1 por entorno, fallo de arranque sin conexión, logging JSON, 6 casos del middleware (Problem Details, traceId, respuesta iniciada, cancelación del cliente, cancelación interna, sin excepción) |
| Worker.Tests | 5 | Composición del host, hosted service, validación al iniciar, logging JSON |
| Architecture.Tests | 26 | Reglas NetArchTest previas y nuevas (frameworks prohibidos en Domain/Application, hosts independientes); reglas sobre `.csproj` (referencias permitidas, Domain sin paquetes, Application solo con `*.Abstractions`, `net10.0`, sin versiones inline, sin prerelease, EF Core alineado, Npgsql con la misma versión mayor); composición del Scheduler |

Ninguna prueba usa servicios externos ni EF Core InMemory. No se eliminó ninguna prueba: los `UnitTest1.cs` se renombraron con `git mv` y conservan sus pruebas originales.

### Documentación

- Nuevos: [PENDING-DECISIONS.md](../project/PENDING-DECISIONS.md) (PD-01 a PD-08, todas **pendientes**), [technical-pending-register.md](technical-pending-register.md) (TP-01 a TP-14) y este informe.
- Actualizados: `docker.md`, `environment-decisions.md` y `compose-topology.md` (variables de host, convención de conexión, health, imágenes); `project-structure.md`; el índice; notas "pendiente de aprobación" en `rbac-matrix.md`, `rabbitmq-outbox.md`, `provider-contracts-v1.5.md`, `hexagonal.md`, `api-design.md` y `erd.md` (sin cambiar su contenido aprobado); `README.md`; `AGENTS.md`; `CLAUDE.md`.

## 4. Archivos modificados

**Creados:** `Directory.Packages.props`, `.dockerignore`, `.env.example`, `src/NotificationService.Api/Common/{ApiDocumentation,StructuredLogging}.cs`, `src/NotificationService.Infrastructure/Common/HealthCheckTags.cs`, `src/NotificationService.Infrastructure/Configuration/DatabaseOptionsValidator.cs`, `src/NotificationService.Worker/Common/WorkerHostBuilderExtensions.cs`, `src/NotificationService.Scheduler/Common/SchedulerHostBuilderExtensions.cs`, `src/NotificationService.{Worker,Scheduler}/Dockerfile`, `tests/NotificationService.Api.Tests/{ApiTestFactory,GlobalExceptionMiddlewareTests}.cs`, `tests/NotificationService.Architecture.Tests/{ProjectStructureRulesTests,SchedulerHostCompositionTests}.cs`, `docs/project/PENDING-DECISIONS.md`, `docs/operations/{technical-pending-register,repository-sanitization-report}.md`.

**Modificados:** `.gitignore`, `README.md`, `AGENTS.md`, `CLAUDE.md`, los 12 `.csproj`, `src/NotificationService.Api/{Program.cs,Dockerfile,NotificationService.Api.http,appsettings.json,appsettings.Development.json,Middleware/GlobalExceptionMiddleware.cs}`, `src/NotificationService.Infrastructure/{DependencyInjection.cs,Configuration/DatabaseOptions.cs}`, `src/NotificationService.{Worker,Scheduler}/{Program.cs,appsettings.json,appsettings.Development.json}` y los 12 documentos de `docs/` citados en §3.

**Renombrados (`git mv`):** `tests/*/UnitTest1.cs` → `DomainFoundationTests.cs`, `ApplicationFoundationTests.cs`, `InfrastructureFoundationTests.cs`, `ApiFoundationTests.cs`, `WorkerHostCompositionTests.cs`, `DependencyRulesTests.cs`.

**Eliminados:** ninguno.

**No modificados a propósito:** `docs/adr/*`, `docs/project/DECISIONS-v1.5.md`, `docs/api/openapi.yaml`, `global.json`, `Directory.Build.props`.

## 5. Decisiones que requieren aprobación

*Situación al cerrar el saneamiento:* PD-01 a PD-08 quedaron pendientes de aprobación ([PENDING-DECISIONS.md](../project/PENDING-DECISIONS.md)), junto con TP-02, TP-05, TP-06 y TP-07.

> **Actualización 2026-11-15:** el usuario **aprobó PD-01 a PD-08**, que ya están incorporadas a los ADR, el ERD, la matriz RBAC, OpenAPI y la documentación de despliegue. Detalle en [decision-closure-report.md](decision-closure-report.md). Siguen requiriendo aprobación TP-02, TP-05, TP-06, TP-07 y los puntos abiertos TP-15 a TP-20 ([technical-pending-register.md](technical-pending-register.md)).

## 6. Riesgos técnicos pendientes

- **Imágenes Docker no construidas:** el daemon de Docker no estaba disponible en este entorno (`failed to connect to the docker API`). Los Dockerfiles no tienen evidencia de build (TP-01).
- Readiness sin dependencias reales (TP-04). Worker y Scheduler sin sonda de salud (TP-05).
- El commit `e25b08d` contiene los antiguos `appsettings.json` con `Username=postgres;Password=postgres`. Son valores de ejemplo, no credenciales reales, pero no deben existir en ninguna base de datos real.
- **Cambio de comportamiento:** los hosts ya no arrancan sin `ConnectionStrings:DefaultConnection`. Antes usaban `postgres/postgres` por defecto. La ruta `/health` se sustituyó por `/health/live` y `/health/ready`.
- Proveedores (Evolution, SMSGate, SMTP) **no validados en ejecución**. El despliegue **no** está listo para producción.

## 7. Comandos ejecutados

```bash
dotnet --info
dotnet restore NotificationService.sln
dotnet build NotificationService.sln --configuration Release --no-incremental
dotnet test NotificationService.sln --configuration Release --no-build
dotnet list NotificationService.sln package --include-transitive
dotnet list NotificationService.sln package --outdated
dotnet list NotificationService.sln package --vulnerable --include-transitive
dotnet publish src/NotificationService.{Worker,Scheduler} -c Release   # inspección de runtimeconfig (salida temporal)
docker info                                                           # falló: daemon no disponible
```

## 8. Resultados reales

| Validación | Resultado |
|---|---|
| `dotnet --info` | SDK 10.0.404, host 10.0.15 |
| `dotnet restore` | Exit 0, sin advertencias NU1xxx |
| `dotnet build -c Release` | **Build succeeded, 0 Warning(s), 0 Error(s)** |
| `dotnet test -c Release` | **63 superadas, 0 fallidas, 0 omitidas** (Domain 1, Application 2, Infrastructure 15, Api 14, Worker 5, Architecture 26) |
| `--include-transitive` | EF Core, Microsoft.Extensions.* y ASP.NET en 10.0.15; Npgsql 10.0.3; sin mezclas de versión |
| `--outdated` | Solo hay versiones 11.x (.NET 11) y versiones mayores nuevas de paquetes de pruebas; no se adoptan (ADR-0008, TP-10) |
| `--vulnerable --include-transitive` | Ningún paquete vulnerable en los 12 proyectos |
| `docker build` | **No ejecutado** (daemon no disponible) |

## 9. Recomendación sobre la Fase 2

**Preparado técnicamente, pero pendiente de decisiones.** La fundación compila sin advertencias, las pruebas pasan, las dependencias son coherentes y no hay secretos versionados. Antes de autorizar la Fase 2 hay que aprobar:

1. **PD-01** (roles y almacenamiento de `super_admin`), **PD-02** (estados) y **PD-05** (`timezone_override`): determinan tablas, columnas y restricciones.
2. **TP-02**: el mecanismo de PostgreSQL para las pruebas de integración de persistencia.

Recomendado, aunque no bloqueante: construir las imágenes Docker (TP-01).

> **Actualización 2026-11-15 (cierre de decisiones):** PD-01, PD-02 y PD-05 están aprobadas e incorporadas al ERD, que es ahora el modelo lógico aprobado para v1. Para autorizar la Fase 2 siguen pendientes **TP-02** (PostgreSQL de pruebas; recomendación: Testcontainers), **TP-18** (vínculo notificación–adjunto) y **TP-20** (roles personalizados), que afectan a tablas. Los resultados de build y pruebas de este informe no cambian: no se modificó código.
