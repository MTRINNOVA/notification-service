# AGENTS.md — notification-service

Reglas obligatorias para agentes de desarrollo (GitHub Copilot Agent, Claude Code y otros) que trabajen en este repositorio.

**Fuente de verdad:** la documentación versionada en [docs/](docs/). Si este archivo y un documento de `docs/` difieren, prevalece el documento (ADR > decisiones > resto) y debe reportarse la diferencia. Si dos documentos se contradicen, **no** elegir una opción: reportarla y esperar decisión (ver §19).

---

## 1. Propósito

Plataforma de notificaciones multi-organización y multi-aplicación (WhatsApp, Email, SMS) expuesta como API REST. Las aplicaciones consumidoras nunca se conectan directamente a los proveedores. `202 Accepted` significa "persistido y aceptado", nunca "entregado".

Alcance v1: envíos individuales y por lotes, plantillas, adjuntos, seguimiento de estados disponibles, programación, campañas, dashboard, auditoría, cuotas, reintentos e idempotencia. Meta WhatsApp Cloud API **no** forma parte de v1.

## 2. Stack aprobado

| Área                            | Tecnología                                                                                         |
| ------------------------------- | -------------------------------------------------------------------------------------------------- |
| Runtime                         | .NET 10 LTS (SDK fijado en [global.json](global.json): `10.0.404`, `rollForward: latestPatch`), C# |
| API                             | ASP.NET Core Web API, REST, JSON UTF-8, OpenAPI 3.1 / Swagger                                      |
| Persistencia                    | PostgreSQL + EF Core 10 + Npgsql                                                                   |
| Mensajería                      | RabbitMQ (broker principal)                                                                        |
| Caché / rate limit / temporales | Redis (nunca como broker)                                                                          |
| Archivos                        | MinIO (bucket privado)                                                                             |
| Contenedores                    | Docker; imágenes en GitHub Container Registry; operación con Portainer                             |
| Proxy                           | Traefik **existente y externo** (nunca incluirlo en el Compose del servicio)                       |
| Pruebas                         | xUnit, NetArchTest (reglas de arquitectura)                                                        |

No añadir tecnologías, librerías de infraestructura o frameworks (MediatR, AutoMapper, FluentValidation, Polly, MassTransit, etc.) sin justificación técnica escrita y autorización explícita.

## 3. Arquitectura aprobada

Clean Architecture + Hexagonal (Ports and Adapters) — [ADR-0001](docs/adr/0001-architecture.md), [hexagonal.md](docs/architecture/hexagonal.md).

- **DDD no es metodología obligatoria.** Se permiten conceptos puntuales (entidades con comportamiento, Value Objects inmutables, encapsulamiento, invariantes).
- **No imponer** Aggregate Roots, Bounded Contexts, Domain Events, Domain Services, CQRS, Event Sourcing ni otros patrones tácticos/estratégicos. Separar comandos y consultas está permitido cuando aporte claridad (CQRS ligero, MediatR opcional según `hexagonal.md`), sin convertirlo en requisito.
- La carpeta `Domain/Events/` existe por la estructura inicial; su existencia **no** obliga a usar domain events. Introducirlos requiere una necesidad concreta y autorización.
- Prioridad: simple, mantenible, extensible. Ante la duda, la opción menos compleja que respete las capas.

## 4. Documentación de referencia (leer antes de implementar)

| Tema                      | Documento                                                                                                                                                                                                                                                                            |
| ------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Decisiones confirmadas    | [docs/project/DECISIONS-v1.5.md](docs/project/DECISIONS-v1.5.md), [docs/adr/](docs/adr/)                                                                                                                                                                                             |
| Estructura y dependencias | [docs/architecture/project-structure.md](docs/architecture/project-structure.md), [hexagonal.md](docs/architecture/hexagonal.md), C4: [context](docs/architecture/c4-context.md), [containers](docs/architecture/c4-containers.md), [components](docs/architecture/c4-components.md) |
| Datos                     | [docs/data/erd.md](docs/data/erd.md) (modelo lógico, requiere revisión antes de migraciones)                                                                                                                                                                                         |
| API                       | [docs/api/openapi.yaml](docs/api/openapi.yaml) (contrato normativo), [api-design.md](docs/api/api-design.md), [endpoint-inventory.md](docs/api/endpoint-inventory.md)                                                                                                                |
| Seguridad                 | [docs/security/security-model.md](docs/security/security-model.md), [rbac-matrix.md](docs/security/rbac-matrix.md), [ADR-0011](docs/adr/0011-admin-identity.md)                                                                                                                      |
| Mensajería                | [docs/messaging/rabbitmq-outbox.md](docs/messaging/rabbitmq-outbox.md), [ADR-0005](docs/adr/0005-messaging.md), [ADR-0012](docs/adr/0012-delivery-semantics.md)                                                                                                                      |
| Proveedores               | [docs/providers/](docs/providers/) (adapters, contratos v1.5, webhooks, riesgos, fuente legada)                                                                                                                                                                                      |
| Cuotas y retención        | [docs/operations/quotas-retention.md](docs/operations/quotas-retention.md)                                                                                                                                                                                                           |
| Despliegue                | [docs/deployment/](docs/deployment/), [ADR-0010](docs/adr/0010-traefik.md)                                                                                                                                                                                                           |
| Aceptación / DoD          | [docs/acceptance/criteria.md](docs/acceptance/criteria.md), [technical-readiness-v1.5.md](docs/operations/technical-readiness-v1.5.md)                                                                                                                                               |

## 5. Estructura real de la solución

Solución: [NotificationService.sln](NotificationService.sln). No crear proyectos paralelos ni duplicar los existentes.

```text
src/
  NotificationService.Domain/          Common/ Entities/ ValueObjects/ Enums/ Events/ Exceptions/
  NotificationService.Application/     Abstractions/ (puertos, p.ej. Time/IClock) Features/ Behaviors/ Mappings/
  NotificationService.Infrastructure/  Persistence/ Messaging/RabbitMQ/ Cache/ Storage/ Providers/ Security/ Configuration/ Time/
  NotificationService.Api/             Endpoints/{Public,Management,Webhooks}/ Authentication/ Authorization/ Middleware/
  NotificationService.Worker/          Consumers/ Handlers/ Retry/ DeadLetter/
  NotificationService.Scheduler/       Jobs/ Services/
tests/
  NotificationService.{Domain,Application,Infrastructure,Api,Worker,Architecture}.Tests/
```

Cada proyecto tiene un `*AssemblyMarker` en `Common/` usado por las pruebas de arquitectura. `Api`, `Worker` y `Scheduler` son adaptadores de entrada y _composition roots_.

## 6. Dirección de dependencias

| Proyecto                 | Puede referenciar                                                           |
| ------------------------ | --------------------------------------------------------------------------- |
| Domain                   | Nada interno. Sin paquetes de infraestructura.                              |
| Application              | Domain (+ abstracciones `Microsoft.Extensions.*.Abstractions`)              |
| Infrastructure           | Application, Domain                                                         |
| Api / Worker / Scheduler | Application, Infrastructure (solo para registrar DI en el composition root) |
| Tests                    | Proyecto bajo prueba y dobles de prueba                                     |

- Application **define** los puertos (interfaces); Infrastructure los **implementa**.
- Los hosts no usan tipos de Infrastructure fuera de `Program.cs`/registro DI.
- Prohibidas las dependencias circulares. Las reglas se verifican en [tests/NotificationService.Architecture.Tests](tests/NotificationService.Architecture.Tests/); al añadir una regla de capas, añadir su prueba.

## 7. Cinco antipatrones prohibidos

### 7.1 Anemic Domain Model

- Entidades con estado encapsulado: setters `private`/`init`, colecciones expuestas como `IReadOnlyCollection`.
- Las transiciones de estado (p.ej. `ACCEPTED → QUEUED → PROCESSING …`) se hacen con métodos del dominio que validan la transición; no asignando `Status` desde Application.
- Las invariantes viven en el dominio; Application no reimplementa reglas de negocio.

### 7.2 Casos de uso con acceso directo a la base de datos

- Application no referencia `Microsoft.EntityFrameworkCore`, `DbContext`, `DbSet`, `IQueryable` de EF, Npgsql ni SQL.
- Usar los puertos de salida (`I*Repository`, `IUnitOfWork`, `IOutboxRepository`, etc.; ver `hexagonal.md`). Crear un puerto solo cuando un caso de uso lo necesite.

### 7.3 Lógica de negocio en controladores/endpoints

- Un endpoint: recibe la solicitud, valida protocolo/esquema, obtiene el contexto autenticado, invoca **un** caso de uso y traduce el resultado a HTTP.
- Prohibido en endpoints: reglas de negocio, cálculos de cuota, selección de remitente/proveedor, acceso a `DbContext` o a repositorios concretos.

### 7.4 Value Objects sin invariantes

- Validar en la creación (factory/constructor); imposible construir un estado inválido.
- Inmutables; igualdad por valor (`record`/`record struct` o igualdad explícita).
- Crear un VO solo cuando encapsule una regla real (p.ej. zona horaria IANA, número normalizado, idempotency key). No envolver cada primitivo.

### 7.5 Dependencias de Infrastructure desde Domain

Domain no puede depender de: EF Core, Npgsql, RabbitMQ.Client, StackExchange.Redis, SDK de MinIO, clientes Evolution/SMTP/SMS, SDK de proveedores, ASP.NET Core ni tipos de Api. Tampoco atributos de mapeo de EF (`[Table]`, `[Column]`…): el mapeo se hace con Fluent API en Infrastructure.

### Verificación obligatoria antes de cerrar una funcionalidad

1. Revisar los cinco antipatrones en el código nuevo o modificado.
2. Verificar la dirección de dependencias (`dotnet test` incluye Architecture.Tests).
3. Confirmar que cada regla está en la capa correcta.
4. Añadir pruebas unitarias de las invariantes de dominio introducidas.
5. Documentar cualquier excepción arquitectónica justificada (ADR o nota en el PR) **con autorización previa**. Nunca por conveniencia.

## 8. Buenas prácticas por capa

**Domain:** comportamiento + invariantes; independiente de infraestructura; sin DTOs de API; sin dependencias NuGet salvo justificación. Los instantes como `DateTimeOffset`/UTC; obtener la hora vía `IClock`, nunca `DateTime.Now`.

**Application:** un caso de uso = una responsabilidad; valida entradas; coordina dominio y puertos; define contratos de salida; devuelve resultados/errores de aplicación tipados; sin detalles HTTP (`IActionResult`, `HttpContext`, códigos HTTP) ni EF Core.

**Infrastructure:** EF Core con Fluent API (`IEntityTypeConfiguration<T>`); migraciones solo aquí y solo en la fase autorizada; adaptadores de RabbitMQ, Redis, MinIO y proveedores; opciones validadas (`ValidateDataAnnotations` + `ValidateOnStart`, como `DatabaseOptions`); timeouts explícitos en toda llamada externa; resiliencia (reintentos/circuit breaker) solo donde el documento de mensajería/proveedor lo indique; los DTOs y excepciones de proveedores se traducen antes de salir del adaptador.

**Api:** endpoints delgados; DTOs de request/response propios (nunca entidades de dominio ni de EF); validación de solicitud; errores RFC 9457 (`application/problem+json` con `code` y `traceId`) mediante manejo centralizado (`GlobalExceptionMiddleware`/ProblemDetails); autenticación y autorización; versionado `/api/v1`; `X-Correlation-Id` aceptado o generado y devuelto; nunca exponer excepciones internas ni stack traces.

**Worker / Scheduler:** adaptadores de entrada delgados que invocan casos de uso; ack manual tras procesar; respetar `CancellationToken` en el apagado.

## 9. Multi-tenancy

- Ámbitos: `Organization` → `Application`; usuarios en varias organizaciones vía membresías; roles/permisos por organización; `super_admin` de plataforma separado y siempre auditado.
- El `organization_id`/`application_id` efectivo **se deriva del principal autenticado** (API Key → aplicación → organización; JWT → membresía activa). Un ID en ruta, query o body nunca es fuente de autoridad.
- Toda consulta y comando de negocio filtra por organización y, cuando aplique, aplicación. Recurso de otro tenant → `404` (no revelar existencia).
- Remitentes: una aplicación puede tener varios; un remitente puede asignarse a varias aplicaciones **de la misma organización**; `senderId` explícito o el predeterminado de la aplicación; sin predeterminado válido → error de negocio documentado.
- Cuotas WhatsApp: 500/día por organización (agregada entre aplicaciones y números), reinicio 00:00 en la zona horaria de la organización; límites por aplicación solo pueden reducir el efectivo; validación fallida no consume; aceptación consume; reintentos técnicos e idempotencia no consumen de nuevo; exceso → `429` con código estable.
- Tiempo: zona horaria IANA obligatoria por organización (default `America/Lima`); las aplicaciones la heredan salvo override permitido; instantes en UTC; cálculos de calendario, programación y cuotas en la zona de la organización.

## 10. Integraciones y adaptadores

Contrato interno neutral; un adaptador por proveedor en `Infrastructure/Providers/`. Detalle: [provider-adapters.md](docs/providers/provider-adapters.md), [provider-contracts-v1.5.md](docs/providers/provider-contracts-v1.5.md).

- **WhatsApp — Evolution API v2.3.7:** puertos separados de envío (`IWhatsAppProvider`) y de conexión/QR (`IWhatsAppConnectionManager`); el dominio no conoce rutas, payloads ni estados de Evolution; una instancia por número y organización; nombre de instancia generado por el backend; QR efímero, nunca persistido ni en logs. Meta Cloud API: solo reservar el valor de proveedor; **no** crear endpoints, flujos ni adaptador en v1.
- **Email — Postfix/SMTP y SMTP externo por organización:** credenciales cifradas fuera del dominio; `SMTP_ACCEPTED` ≠ `DELIVERED`.
- **SMS — Android SMS Gateway:** dispositivo/SIM por organización; versión, autenticación (Basic vs token privado) y callbacks **pendientes de validar**: no asumir el contrato legado.
- **MinIO:** binarios en bucket privado; PostgreSQL guarda metadatos y checksum; validar tamaño, extensión y MIME real; URLs firmadas de corta duración; nunca exponer credenciales.
- **Webhooks:** validar autenticidad → limitar tamaño → persistir con clave de deduplicación → responder rápido → procesar asíncrono; tolerar duplicados y desorden; resolver tenant desde `providerAccountId` interno. No implementar un webhook antes de cerrar su contrato ([webhook-contract-readiness.md](docs/providers/webhook-contract-readiness.md)).
- Aceptación del proveedor ≠ entrega. Solo reportar estados que el proveedor permita comprobar. Resultado ambiguo (timeout tras envío) → no reenviar ciegamente; conciliar.
- No implementar un adaptador hasta validar el contrato real de la versión fijada del proveedor.

## 11. Persistencia y versiones de paquetes

- PostgreSQL es la fuente de verdad. EF Core y Npgsql solo en Infrastructure.
- Versiones vigentes (validadas el 2026-11-15, todas estables):

| Paquete                                          | Versión                                                                                                                        |
| ------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------ |
| Microsoft.EntityFrameworkCore                    | 10.0.15                                                                                                                        |
| Microsoft.EntityFrameworkCore.Relational         | 10.0.15 (fijado explícitamente para alinear con EF Core)                                                                       |
| Npgsql.EntityFrameworkCore.PostgreSQL            | 10.0.3 (requiere EF Core ≥ 10.0.4 y < 11)                                                                                      |
| Npgsql (transitivo)                              | 10.0.3                                                                                                                         |
| Microsoft.EntityFrameworkCore.Design / dotnet-ef | aún no usados; cuando se autorice la fase de migraciones: misma versión que EF Core (10.0.x), dotnet-ef como herramienta local |

- Reglas: misma versión mayor y menor en todos los paquetes EF Core; no mezclar EF Core 9 con Npgsql 10 ni viceversa; nada preview/alpha/beta/rc; no saltar a 11.x (corresponde a .NET 11; ADR-0008 fija .NET 10 LTS) sin ADR; no actualizar paquetes en lote ni "de paso"; revisar transitivos (`dotnet list package --include-transitive`) y vulnerabilidades (`--vulnerable`); documentar cada cambio de versión y su motivo.
- No existe `Directory.Packages.props`: las versiones se declaran en cada `.csproj`. Si se adopta la gestión centralizada (pendiente de aprobación), todas las versiones pasan a ese archivo.
- Migraciones: solo en la fase autorizada, revisadas, compatibles con despliegue expand/contract y ejecutadas como tarea única de release, nunca automáticamente por cada réplica.
- No modificar el modelo de datos sin que el ERD lo respalde. JSONB solo para payloads/configuración variable.

## 12. Mensajería: RabbitMQ, Outbox, Inbox e idempotencia

Detalle en [rabbitmq-outbox.md](docs/messaging/rabbitmq-outbox.md).

- Semántica: at-least-once + efectos idempotentes. Nunca prometer exactly-once.
- **Outbox:** recurso de negocio + reserva de cuota + `outbox_messages` en la misma transacción PostgreSQL; `202` solo después de confirmar esa transacción. Publicador con `FOR UPDATE SKIP LOCKED` (o equivalente) y publisher confirms.
- **Inbox:** consumidores con ack manual y deduplicación por (`consumer_name`, `message_id`).
- **Idempotency-Key** obligatorio en creación de envíos, lotes, campañas y programaciones: misma key + mismo hash → recurso original; misma key + hash distinto → `409`.
- Reintentos: clasificar transitorio/permanente; backoff exponencial con jitter y límite; respetar `Retry-After`; errores permanentes → fallo terminal/DLQ inspeccionable; no bucles inmediatos.
- Propagar `CorrelationId` en mensajes y logs. Nunca secretos en mensajes.
- Redis no es broker ni cola.

## 13. Seguridad

- Sin secretos en código, `appsettings*.json` versionados, tests ni mensajes. Los valores de `appsettings.json` son solo para desarrollo local. No subir `.env` reales; `.env.example` solo con nombres y valores ficticios.
- Panel: access JWT (~15 min) validando firma, issuer, audience, expiración y `kid`; refresh token opaco, rotativo, guardado como hash, en cookie `HttpOnly` + `Secure` + `SameSite`; detección de reutilización; protección CSRF; contraseñas Argon2id.
- Aplicaciones: `X-API-Key` por aplicación, guardada como hash con prefijo, mostrada una sola vez, con scopes, expiración, rotación y revocación. Las API Keys no heredan permisos humanos.
- Autorización en el backend en cada operación (RBAC + ámbito); el frontend no es control de seguridad. Pruebas negativas obligatorias contra IDOR y acceso cruzado.
- Credenciales de proveedores cifradas (AES-GCM o secret manager, clave fuera de PostgreSQL); nunca devueltas por la API, al frontend ni a aplicaciones consumidoras.
- Logs estructurados sin tokens, contraseñas, API Keys, QR, contenido completo ni datos personales innecesarios (redactar teléfonos/destinatarios).
- Rate limiting por organización/aplicación/canal; protección SSRF en URLs configurables; HTTPS en entornos expuestos; CORS restrictivo.
- Nunca deshabilitar la validación TLS ni certificados para "hacer funcionar" una integración.

## 14. Estrategia de pruebas

| Proyecto             | Qué cubre                                                                                                                                                         |
| -------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Domain.Tests         | Invariantes, transiciones de estado, Value Objects (unitarias, sin mocks de infraestructura)                                                                      |
| Application.Tests    | Casos de uso con dobles de los puertos                                                                                                                            |
| Infrastructure.Tests | Persistencia contra PostgreSQL (EF InMemory no reproduce PostgreSQL; la herramienta para levantar la BD de prueba requiere aprobación), adaptadores, Outbox/Inbox |
| Api.Tests            | Integración HTTP, contratos OpenAPI, autenticación/autorización, aislamiento multi-tenant                                                                         |
| Worker.Tests         | Consumo, idempotencia, reintentos, DLQ                                                                                                                            |
| Architecture.Tests   | Dirección de dependencias y reglas de capas                                                                                                                       |

- Proveedores: probar con adaptadores simulados y pruebas de contrato; nunca contra producción.
- Toda funcionalidad nueva incluye pruebas; las invariantes de dominio siempre.
- Usar `IClock` para pruebas deterministas de tiempo, cuotas y zonas horarias.
- Compilar no es terminar. No eliminar ni desactivar pruebas para que pase el build.
- Los archivos `UnitTest1.cs` actuales son pruebas de base; al añadir pruebas reales, nombrar los archivos por la clase o comportamiento probado.

## 15. Convenciones de código

- Nullable e ImplicitUsings habilitados ([Directory.Build.props](Directory.Build.props)). No suprimir advertencias de nulabilidad sin justificación.
- Código, identificadores y nombres de tipos en inglés; documentación del proyecto en español.
- `sealed` por defecto en clases no diseñadas para herencia; constructores primarios para DI cuando sea legible.
- `async`/`await` de extremo a extremo; `CancellationToken` en toda operación de I/O; prohibido `.Result`, `.Wait()`, `GetAwaiter().GetResult()` y `Task.Run` para "hacer async" en ASP.NET Core.
- Sin `catch` vacíos; no usar excepciones como flujo normal de negocio (usar resultados/errores tipados); excepciones solo para situaciones excepcionales.
- Diferenciar errores de validación (`400`), dominio/negocio (`409`/`422` según contrato), autorización (`401`/`403`/`404`), cuota (`429`), infraestructura/proveedor (`503`) e inesperados (`500`).
- SOLID, KISS, YAGNI, DRY razonable; clases y métodos pequeños y cohesivos; sin Service Locator (`IServiceProvider` solo en composition roots/factories justificadas); sin abstracciones especulativas.
- Inyección de dependencias por constructor; registro en `DependencyInjection.cs` de cada capa (`AddApplication`, `AddInfrastructure`).
- Configuración mediante opciones tipadas y validadas; nombres de variables de entorno según [compose-topology.md](docs/deployment/compose-topology.md).

## 16. Comandos de desarrollo

```bash
dotnet --info
dotnet restore NotificationService.sln
dotnet build NotificationService.sln
dotnet test NotificationService.sln
dotnet list NotificationService.sln package --include-transitive
dotnet list NotificationService.sln package --outdated
dotnet list NotificationService.sln package --vulnerable --include-transitive

dotnet run --project src/NotificationService.Api        # GET /, /health, /swagger
dotnet run --project src/NotificationService.Worker
dotnet run --project src/NotificationService.Scheduler
```

No ejecutar `dotnet ef migrations add` ni `dotnet ef database update` fuera de la fase autorizada.

## 17. Despliegue

- Compose del backend: API, Worker, Scheduler, PostgreSQL, RabbitMQ, Redis, MinIO. Evolution/Postfix pueden ir en Compose separado.
- Traefik externo: conectar solo API (y Admin en su repo) a la red externa existente mediante labels; nombre real de la red pendiente de confirmar.
- Datos sin puertos publicados al host en producción; imágenes por versión/digest (nunca `latest`); usuario no-root; healthchecks liveness/readiness; límites de recursos.

## 18. Trabajo por fases

1. Leer la documentación relevante antes de implementar.
2. Ejecutar solo el alcance de la fase solicitada; no adelantar funcionalidades.
3. No modificar decisiones aprobadas ni incorporar tecnologías sin autorización.
4. No duplicar funcionalidades ni proyectos existentes.
5. Cambios en endpoints/esquemas: actualizar primero `openapi.yaml` y la documentación relacionada; ningún endpoint fuera de OpenAPI.
6. No generar migraciones fuera de la fase autorizada.
7. No declarar terminado algo incompleto; listar lo pendiente.
8. Problema fuera de alcance: documentarlo, explicar impacto, proponer solución y **esperar autorización** antes de ampliar el alcance.

## 19. Contradicciones y pendientes conocidos

No convertir en regla ninguno de estos puntos hasta que se resuelvan en `docs/`:

- Nombres de roles: `rbac-matrix.md` usa `org_admin`, `operator`, `viewer`; ADR-0011 usa `organization_admin`, `application_admin`, `operator`, `viewer`.
- Configuración de conexión: el código usa la sección `Database:ConnectionString`; `compose-topology.md` documenta `ConnectionStrings__DefaultConnection`.
- Estados de entrega: `rabbitmq-outbox.md` no incluye el estado `UNKNOWN/RECONCILIATION_REQUIRED` de `provider-contracts-v1.5.md`.
- Puerto de proveedor: `hexagonal.md` define `INotificationProvider` genérico; ADR-0004 y `provider-adapters.md` proponen `IWhatsAppProvider` + `IWhatsAppConnectionManager`.
- Uso de `422` frente a `400`/`409` para errores de negocio ("si se adopta" en `api-design.md`).
- Subdominios propuestos distintos en `docker.md` y `environment-decisions.md` (ninguno confirmado).
- `timezone_override` de aplicación: permitido solo "si la política de producto lo habilita" (política no definida).

## 20. Checklist antes de entregar cambios

- [ ] Alcance limitado a la fase solicitada; nada adelantado.
- [ ] Documentación leída; ninguna decisión aprobada modificada.
- [ ] Cinco antipatrones revisados (§7) y dependencias entre capas correctas.
- [ ] Filtro por organización/aplicación derivado del principal; pruebas negativas de acceso cruzado.
- [ ] Sin secretos ni datos sensibles en código, configuración versionada o logs.
- [ ] OpenAPI y documentación actualizados si cambió el contrato.
- [ ] Sin migraciones ni cambios de modelo no autorizados.
- [ ] Paquetes coherentes y estables; cambios de versión documentados.
- [ ] `dotnet restore`, `dotnet build` (0 errores, sin advertencias nuevas) y `dotnet test` ejecutados y en verde.
- [ ] Informe con archivos modificados, comandos ejecutados con resultados reales y pendientes.
