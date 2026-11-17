# Decisiones confirmadas — Technical Readiness v1.5

**Estado:** decisiones funcionales confirmadas; implementación no iniciada.

Este documento registra la aceptación del usuario de las nueve recomendaciones propuestas para cerrar el diseño de `notification-service`. Las decisiones se incorporan a la especificación; no autorizan por sí mismas el inicio de programación.

## 1. Organizaciones, usuarios y aplicaciones

- Se permite que un mismo usuario administre varias organizaciones mediante una relación muchos-a-muchos entre usuarios y organizaciones.
- Los roles y permisos se evalúan dentro del ámbito de cada organización.
- Se mantiene un `super_admin` de plataforma.
- Cada organización administra sus administradores y operadores.
- Cada aplicación pertenece a una única organización.
- El backend debe validar el ámbito de organización y aplicación en cada operación; no confiar en identificadores enviados por el cliente.

## 2. Evolution API y números WhatsApp

- Modalidad inicial: una instalación centralizada de Evolution API, administrada por la plataforma.
- Cada organización podrá conectar múltiples números, cada uno representado por una instancia/sesión independiente.
- Las sesiones, credenciales, remitentes y eventos deben quedar aislados por organización.
- El onboarding inicial será mediante QR.
- El contrato interno de WhatsApp será neutral respecto al proveedor; Meta Cloud API podrá incorporarse posteriormente mediante otro adaptador.
- Meta Cloud API no forma parte del alcance funcional inicial.

## 3. Asignación de remitentes a aplicaciones

- Una aplicación podrá tener varios remitentes autorizados.
- Un remitente podrá asignarse a varias aplicaciones de la misma organización.
- La solicitud podrá indicar `senderId` explícitamente.
- Si no se indica, se utilizará el remitente predeterminado de la aplicación.
- Si no existe un remitente predeterminado válido, la solicitud se rechazará con un error de negocio documentado.
- No se permitirá asignar remitentes entre organizaciones distintas.
- _(Precisado el 2026-11-15, TP-33):_ el remitente predeterminado pertenece a la **aplicación** (`application_senders.is_default`, `defaultSenderId`), con como máximo uno válido por aplicación y canal, validado en una transacción. No existe predeterminado propio del remitente ni predeterminado global por organización; se retiraron `whatsapp_senders.is_default` y `Sender.isDefault`.

## 4. SMS Gateway

- Modalidad inicial: cada empresa cliente proporciona y administra su propio dispositivo Android y SIM.
- La plataforma centraliza la integración y administración lógica, pero debe mantener la asociación del gateway/dispositivo con su organización.
- La autenticación, versión del servidor SMSGate, versión de la app Android y contrato de callbacks siguen sujetos a validación técnica antes de habilitar el adaptador en producción.

## 5. Email

- Se soportarán ambas modalidades:
  1. Postfix/SMTP administrado por la plataforma como proveedor inicial.
  2. Servidor SMTP externo configurado por una organización.
- Las credenciales SMTP externas se almacenarán cifradas y nunca se expondrán al frontend ni a las aplicaciones consumidoras.
- La selección del perfil SMTP se resolverá en el backend según la organización y la configuración autorizada.

## 6. Alcance funcional de primera versión

Se confirma como alcance objetivo inicial:

- WhatsApp mediante Evolution API/Baileys.
- Email mediante Postfix/SMTP y perfiles SMTP externos.
- SMS mediante Android SMS Gateway.
- Envío individual y por lotes.
- Plantillas.
- Adjuntos.
- Seguimiento de estados disponibles por proveedor.
- Programación de envíos.
- Campañas.
- Dashboard administrativo.

Meta WhatsApp Cloud API queda expresamente fuera de la primera versión.

## 7. Autenticación del panel

- Access token JWT de corta duración.
- Refresh token opaco, rotativo, almacenado en cookie `HttpOnly` y `Secure`.
- Protección CSRF para operaciones autenticadas mediante cookie.
- API Keys independientes por aplicación consumidora, con rotación y revocación.
- Los secretos de proveedores no se entregan al navegador.

## 8. Cuotas

- Cuota de referencia: hasta 500 mensajes WhatsApp diarios por organización.
- La cuota es agregada y compartida entre todas las aplicaciones y números de la organización.
- El reinicio ocurre a las 00:00 en la zona horaria de la organización.
- Las solicitudes rechazadas por validación no consumen cuota.
- Una solicitud aceptada para procesamiento consume cuota.
- Los reintentos técnicos del mismo mensaje no consumen cuota adicional.
- _(Precisado el 2026-11-15, TP-44 y TP-45):_ una notificación programada consume cuota al ejecutarse, en el día local de ejecución; el límite de la organización vive en `organization_limits` y solo lo cambia `super_admin`.
- Al superar el límite se responde HTTP 429 con código de error estable y datos de reintento cuando se conozcan.
- Se podrán configurar límites menores por aplicación.
- La cuota no constituye SLA ni garantía de entrega o throughput.

## 9. Zona horaria

- Cada organización tendrá una zona horaria IANA obligatoria.
- Valor inicial predeterminado para nuevas organizaciones: `America/Lima`.
- Las aplicaciones heredan la zona horaria de su organización, salvo que exista una configuración explícita permitida. _(Precisado el 2026-11-15, PD-05: en v1 no existe ninguna configuración por aplicación; ver «Decisiones de cierre».)_
- Los instantes se almacenan en UTC; los cálculos de calendario, cuotas y programación respetan la zona horaria configurada.

## Reglas transversales

- Toda consulta y comando de negocio debe validar `organization_id` y, cuando corresponda, `application_id`.
- La semántica de estados será común, pero solo se reportarán estados que el proveedor realmente permita comprobar.
- Un timeout ambiguo no se reintentará ciegamente; se aplicará reconciliación/deduplicación para reducir envíos duplicados.
- RabbitMQ será el broker; Redis no se utilizará como cola principal.
- Se mantiene PostgreSQL + EF Core, MinIO, .NET 10, Clean Architecture + Hexagonal y frontend independiente.

## Decisiones de cierre PD-01 a PD-08 (aprobadas el 2026-11-15)

Aprobadas expresamente por el usuario. Precisan, sin sustituirlas, las decisiones de negocio anteriores. Historial y alternativas: [PENDING-DECISIONS.md](../history/PENDING-DECISIONS.md).

| ID    | Decisión                                                                                                                                                                                   | Documento normativo                                                                                               |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ----------------------------------------------------------------------------------------------------------------- |
| PD-01 | Roles v1: `super_admin` (privilegio de plataforma en `platform_role_assignments`), `organization_admin`, `operator`, `viewer` (roles de membresía). Sin `application_admin` ni `org_admin` | [ADR-0011](../adr/0011-admin-identity.md), [rbac-matrix.md](../security/rbac-matrix.md), [erd.md](../data/erd.md) |
| PD-02 | Estados en tres niveles: público, entrega e intento; `RECONCILIATION_REQUIRED` solo en entrega; historial `delivery_status_history`                                                        | [ADR-0012](../adr/0012-delivery-semantics.md)                                                                     |
| PD-03 | Puertos de proveedores especializados por canal; sin `INotificationProvider`                                                                                                               | [hexagonal.md](../architecture/hexagonal.md), [ADR-0004](../adr/0004-whatsapp-adapters.md)                        |
| PD-04 | `400` formato, `409` conflicto, `422` regla de negocio; RFC 9457 con catálogo cerrado de `code`                                                                                            | [api-design.md](../api/api-design.md), [error-codes.md](../api/error-codes.md)                                    |
| PD-05 | Sin `applications.timezone_override` en v1; zona horaria única de la organización para cuotas y programación                                                                               | [erd.md](../data/erd.md)                                                                                          |
| PD-06 | Hosts por variables `NOTIFICATIONS_API_HOST`, `NOTIFICATIONS_ADMIN_HOST`, `NOTIFICATIONS_WEBHOOKS_HOST`; DNS reales sin fijar; Traefik externo                                             | [docker.md](../deployment/docker.md)                                                                              |
| PD-07 | Una imagen por host: `notification-api`, `notification-worker`, `notification-scheduler`                                                                                                   | [docker.md](../deployment/docker.md)                                                                              |
| PD-08 | `openapi.yaml` declara `409`, `422`, `429` (y `413`/`415`/`503` donde corresponde) por operación, con componentes RFC 9457 reutilizables                                                   | [openapi.yaml](../api/openapi.yaml)                                                                               |

## Decisiones técnicas TP-02 y TP-15 a TP-22 (aprobadas el 2026-11-15)

| ID    | Decisión                                                                                                                                                  | Documento normativo                                                                                           |
| ----- | --------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------- |
| TP-02 | Testcontainers con PostgreSQL 17 real para las pruebas de integración de persistencia                                                                        | [integration-testing-postgresql.md](../testing/integration-testing-postgresql.md)                             |
| TP-15 | Bootstrap manual e idempotente del primer `super_admin` (`SYSTEM_BOOTSTRAP`) y Platform API para concesiones y revocaciones; protección del último activo | [ADR-0011](../adr/0011-admin-identity.md), [super-admin-bootstrap.md](../operations/super-admin-bootstrap.md) |
| TP-16 | Contrato OpenAPI normalizado (rutas, paginación, operaciones del inventario); retirada de `PUT /users/{userId}/roles`                                     | [openapi.yaml](../api/openapi.yaml)                                                                           |
| TP-17 | Puerto opcional `IProviderAccountDiagnostics` para prueba de conexión y salud                                                                             | [hexagonal.md](../architecture/hexagonal.md)                                                                  |
| TP-18 | Relación muchos a muchos `notification_attachments`                                                                                                       | [erd.md](../data/erd.md)                                                                                      |
| TP-19 | Reintento técnico, reenvío manual y conciliación separados; conciliación configurable por proveedor                                                       | [ADR-0012](../adr/0012-delivery-semantics.md)                                                                 |
| TP-20 | Sin roles personalizados en v1: catálogo cerrado `organization_admin`, `operator`, `viewer`                                                               | [ADR-0011](../adr/0011-admin-identity.md), [rbac-matrix.md](../security/rbac-matrix.md)                       |
| TP-21 | Reenvío manual solicitado por la aplicación consumidora (API Key con scope `notifications.retry`); actor auditado `APPLICATION` | [ADR-0012](../adr/0012-delivery-semantics.md), [rbac-matrix.md](../security/rbac-matrix.md) |
| TP-22 | `openapi.yaml` como fuente normativa única del contrato HTTP; `PATCH .../memberships/{membershipId}/status` incorporada; `PATCH /senders/{id}/default` descartada; esquemas sin uso eliminados | [api-design.md](../api/api-design.md), [openapi.yaml](../api/openapi.yaml) |


## Decisiones técnicas TP-05 a TP-07, TP-28 y TP-33 a TP-37 (aprobadas el 2026-11-15)

Aprobadas expresamente por el usuario y especificadas en la documentación. **No están implementadas:** el estado de implementación y validación de cada una está en [technical-pending-register.md](../operations/technical-pending-register.md).

| ID    | Decisión | Documento normativo |
| ----- | -------- | ------------------- |
| TP-05 | Worker y Scheduler exponen `/health/live` y `/health/ready` por HTTP interno, consultados por Docker `HEALTHCHECK`/Portainer y no publicados por Traefik | [health-checks.md](../deployment/health-checks.md) |
| TP-06 | `tests/NotificationService.Scheduler.Tests` se crea con la primera lógica funcional propia del Scheduler; no se crea vacío | [scheduler-tests.md](../testing/scheduler-tests.md) |
| TP-07 | GitHub Actions con dos responsabilidades separadas: CI de Pull Requests y publicación controlada en GHCR (tags inmutables y digest). El despliegue automático al VPS queda fuera | [ci-cd.md](../deployment/ci-cd.md) |
| TP-28 | `GET /public/attachments/{attachmentId}` exige el scope independiente `attachments.read`; la subida sigue con `attachments.upload` | [openapi.yaml](../api/openapi.yaml), [rbac-matrix.md](../security/rbac-matrix.md) |
| TP-33 | Remitente predeterminado solo por aplicación; retirados `whatsapp_senders.is_default` y `Sender.isDefault` | [erd.md](../data/erd.md), [openapi.yaml](../api/openapi.yaml) |
| TP-34 | Los esquemas `*Page` declaran como requeridos `items`, `page`, `pageSize` y `total` | [openapi.yaml](../api/openapi.yaml), [api-design.md](../api/api-design.md) |
| TP-35 | Ninguna organización puede quedarse sin `organization_admin` activo (`422 LAST_ORGANIZATION_ADMIN_REQUIRED`); recuperación excepcional por `super_admin` | [ADR-0011](../adr/0011-admin-identity.md), [rbac-matrix.md](../security/rbac-matrix.md) |
| TP-36 | `POST /management/notifications/{notificationId}/cancel` para usuarios autorizados, con el mismo caso de uso que la Public API | [openapi.yaml](../api/openapi.yaml), [api-design.md](../api/api-design.md) |
| TP-37 | Refresh y logout declaran `RefreshCookie` (cookie `refresh`) y `CsrfHeader` (`X-CSRF-Token`) | [ADR-0011](../adr/0011-admin-identity.md), [openapi.yaml](../api/openapi.yaml) |

## Decisiones técnicas TP-38 y TP-39 (aprobadas el 2026-11-15)

Aprobadas expresamente por el usuario y especificadas en la documentación; **no implementadas** (estado en [technical-pending-register.md](../operations/technical-pending-register.md)).

| ID    | Decisión | Documento normativo |
| ----- | -------- | ------------------- |
| TP-38 | Token CSRF sincronizado con la sesión: lo genera el backend con un generador criptográficamente seguro, se guarda como hash, lo devuelven login y refresh en `csrfToken`, el panel lo conserva en memoria y lo envía en `X-CSRF-Token`. Sin doble envío por cookie | [ADR-0011](../adr/0011-admin-identity.md), [openapi.yaml](../api/openapi.yaml), [admin-panel-auth-contract.md](../api/admin-panel-auth-contract.md) |
| TP-39 | Cancelar una notificación ya `CANCELLED` → `409 RESOURCE_STATE_CONFLICT`, igual en la Public API y en la Management API; sin idempotencia con `200` | [openapi.yaml](../api/openapi.yaml), [api-design.md](../api/api-design.md) |

## Decisiones de preparación de la Fase 2 (aprobadas el 2026-11-15)

Aprobadas expresamente por el usuario para cerrar los huecos documentales que impedían iniciar la persistencia. **No están implementadas.** Evidencia: [decision-closure-report-phase2-readiness.md](../history/decision-closure-report-phase2-readiness.md).

| ID    | Decisión | Documento normativo |
| ----- | -------- | ------------------- |
| TP-40 | Parámetros del token CSRF: 32 bytes aleatorios, SHA-256 en `refresh_tokens.csrf_token_hash`, comparación en tiempo constante, `Origin` obligatorio en login/refresh/logout. El refresh token sigue el mismo criterio | [ADR-0011](../adr/0011-admin-identity.md), [erd.md](../data/erd.md) |
| TP-43 | Lotes con aceptación **parcial**: `notification_batches`, `notifications.batch_id` y resultado por ítem en `AcceptedBatch.items` | [openapi.yaml](../api/openapi.yaml), [erd.md](../data/erd.md) |
| TP-44 | La cuota de una notificación programada se reserva **al ejecutarse**, en el día local de ejecución; si falla, `FAILED` sin reprogramación automática | [erd.md](../data/erd.md), [quotas-retention.md](../operations/quotas-retention.md) |
| TP-45 | Límites de organización en `organization_limits` por canal (WhatsApp 500/día por defecto); solo `super_admin` (`/platform/organizations/{organizationId}/limits`); efectivo = menor entre aplicación y organización | [openapi.yaml](../api/openapi.yaml), [erd.md](../data/erd.md) |
| TP-46 | Alta de usuarios con contraseña temporal y cambio obligatorio (`must_change_password`, `changePassword`, `403 PASSWORD_CHANGE_REQUIRED`); `createMembership` solo vincula usuarios existentes | [ADR-0011](../adr/0011-admin-identity.md) |
| TP-47 | Cifrado PGP (EMAIL) y ZIP AES-256 (EMAIL/WHATSAPP con adjuntos) en v1, heredado del sistema anterior; secreto ZIP cifrado fuera del dominio | [openapi.yaml](../api/openapi.yaml), [erd.md](../data/erd.md), [hexagonal.md](../architecture/hexagonal.md) |
| TP-48 | `application_permissions` es el techo de scopes de las API Keys de la aplicación | [rbac-matrix.md](../security/rbac-matrix.md), [erd.md](../data/erd.md) |
| TP-49 | `provider_accounts.is_default`: como máximo una cuenta predeterminada por organización y canal (Email/SMS) | [erd.md](../data/erd.md), [openapi.yaml](../api/openapi.yaml) |
| TP-50 | Dominios cerrados de todas las columnas de estado | [erd.md](../data/erd.md) |
| TP-51 | Catálogo cerrado de permisos de rol (`RolePermissionCode`) y asignaciones iniciales | [rbac-matrix.md](../security/rbac-matrix.md) |
| TP-52 | Rotación de API Key: fila nueva y revocación de la anterior en la misma transacción, sin solape | [erd.md](../data/erd.md), [openapi.yaml](../api/openapi.yaml) |
| TP-53 | Alineación ERD ↔ OpenAPI: `applicationId` en plantillas, campañas y programaciones; columnas que faltaban; retirada de `notifications.priority` | [erd.md](../data/erd.md), [openapi.yaml](../api/openapi.yaml) |
| TP-41 | Recuperación de sesión al cargar el panel con `resumeSession` (cookie + `Origin`, sin CSRF); serialización entre pestañas | [ADR-0011](../adr/0011-admin-identity.md), [admin-panel-auth-contract.md](../api/admin-panel-auth-contract.md) |
| TP-42 | Se mantiene la distinción `401` (sesión) / `403` (CSRF u `Origin`); riesgo aceptado | [ADR-0011](../adr/0011-admin-identity.md) |
| TP-54 | El secreto ZIP se purga al llegar a un estado terminal; las notificaciones `ZIP_AES256` no se reenvían | [erd.md](../data/erd.md), [ADR-0012](../adr/0012-delivery-semantics.md) |
| TP-55 | Política NIST SP 800-63B (12 a 128 caracteres, lista de comunes y filtradas) y recuperación por reasignación de contraseña temporal | [ADR-0011](../adr/0011-admin-identity.md) |
| TP-56 | Un gateway SMS por cuenta de proveedor; `notification_deliveries.sms_gateway_id` | [erd.md](../data/erd.md) |
| TP-58 | Aviso de cierre 5 minutos antes (`sessionExpiresAt`/`sessionExpiresIn`) con reautenticación por contraseña en el propio aviso; el login revoca la sesión anterior | [ADR-0011](../adr/0011-admin-identity.md), [admin-panel-auth-contract.md](../api/admin-panel-auth-contract.md) |
| TP-57 | Sesión del panel con duración absoluta de 60 minutos desde el login (sustituye los 7 días iniciales); access token de 15 minutos con renovación interna | [ADR-0011](../adr/0011-admin-identity.md), [erd.md](../data/erd.md) |

**Fuera de v1** (revisión del sistema anterior `Notificaciones-master`, 2026-11-15): lista de supresión, registro de consentimiento (el opt-in es responsabilidad de la organización cliente), metadatos de origen normalizados (se usan los `metadata` libres) y prioridad de notificación. Detalle en [legacy-source-assessment.md](../providers/legacy-source-assessment.md).

## Base de datos (confirmada el 2026-11-15)

- **PostgreSQL 17** como versión principal en desarrollo, pruebas y producción.
- Testcontainers usa PostgreSQL 17 para las pruebas de integración (TP-02).
- Nunca `latest`: la imagen concreta se fija con versión patch explícita (`postgres:17.<patch>`) y, cuando corresponda, con digest (TP-11). No se modificaron configuraciones ejecutables ni Compose en esta decisión.

## Pendientes técnicos que no son decisiones abiertas

La lista original de ocho puntos se migró el 2026-11-15 al **registro operativo único** [technical-pending-register.md](../operations/technical-pending-register.md): Evolution → TP-24; SMSGate → TP-25; SMTP/DNS → TP-26; dominios, red Traefik, rutas y almacenamiento → TP-27; RPO/RTO → TP-29; pruebas de aislamiento, carga y recuperación → TP-32. El punto de completar OpenAPI, ERD y contratos de error quedó resuelto con PD-01..PD-08, TP-16 y TP-22.

**Estado final:** las nueve decisiones consultadas quedan aceptadas. La programación sigue sin iniciarse; el siguiente entregable es consolidar estas decisiones en el modelo de datos, OpenAPI y criterios de aceptación antes de autorizar el desarrollo.
