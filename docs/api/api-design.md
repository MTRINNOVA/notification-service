# Criterios de diseño de API — v1

## Fuente de verdad

| Documento | Papel |
|---|---|
| [openapi.yaml](openapi.yaml) | **Fuente normativa única** del contrato HTTP v1: operaciones, rutas, métodos, autenticación, parámetros, respuestas y esquemas |
| [endpoint-inventory.md](endpoint-inventory.md) | Resumen **derivado** de `openapi.yaml` (operación y permiso); no define contrato propio |
| Este documento | Criterios y reglas de diseño que explican el contrato. **No define rutas ni esquemas** |
| [error-codes.md](error-codes.md) | Catálogo cerrado de valores de `code`, referenciado por OpenAPI |

Ante cualquier discrepancia prevalece `openapi.yaml`, y la discrepancia se corrige en el documento secundario. Una ruta o esquema nuevo se añade **primero** a `openapi.yaml`, con revisión de seguridad, pruebas de contrato y changelog; después se actualiza el inventario. Ningún documento secundario autoriza implementar operaciones o payloads que no estén en OpenAPI.

## Convenciones

- Base path: `/api/v1`.
- REST sobre HTTPS; JSON UTF-8 para operaciones de negocio.
- UUID para identificadores internos; fechas RFC 3339 en UTC (`Z`).
- `X-Correlation-Id` se acepta o se genera, y se devuelve en la respuesta.
- `Idempotency-Key` obligatorio para crear envíos, lotes, campañas y programaciones, y para el reenvío manual.
- API Keys de aplicaciones en `X-API-Key`, con un scope por operación declarado en `x-required-scopes` (catálogo cerrado `ApiKeyScope`; [rbac-matrix.md](../security/rbac-matrix.md#scopes-de-api-key-public-api)). Usuarios del panel con access JWT Bearer. Solo `refresh` y `logout` se autentican con la cookie `refresh` más la cabecera `X-CSRF-Token` (TP-37, [ADR-0011](../adr/0011-admin-identity.md#sesión-del-panel-cookie-de-refresh-y-csrf-tp-37-aprobada-2026-11-15)). El token CSRF está sincronizado con la sesión y lo devuelven login y refresh en `csrfToken` (TP-38; contrato del panel en [admin-panel-auth-contract.md](admin-panel-auth-contract.md)).
- El contexto de organización se deriva de las credenciales y la membresía. Un `organizationId` en ruta o cuerpo nunca sustituye a la autorización.
- Paginación `page`/`pageSize` solo en colecciones; máximo 100; orden estable por fecha e ID. Las consultas de un recurso individual no se paginan. Valores por defecto: `page = 1` y `pageSize = 25`. Toda respuesta paginada (`*Page`) declara como requeridos `items`, `page` (≥ 1), `pageSize` (1..100) y `total` (≥ 0). `total` es el número de resultados que coinciden con los filtros aplicados, antes de paginar (TP-34).
- Errores RFC 9457 `application/problem+json` con `type`, `title`, `status`, `detail`, `instance`, `code` y `traceId`; `errors` y `retryAt` cuando correspondan.
- `202 Accepted` confirma persistencia y aceptación, no envío ni entrega.
- Archivos mediante `multipart/form-data`; respuesta JSON con `attachmentId`.
- Zona horaria: las aplicaciones heredan la de su organización; en v1 no existe zona horaria por aplicación (PD-05). Las horas locales de programación se interpretan en la zona de la organización y se almacenan en UTC.

## Superficies de API

| API | Base | Autenticación | Actor y ámbito |
|---|---|---|---|
| Public API | `/api/v1/public` | `X-API-Key` con scopes | La **aplicación** consumidora, dentro de su organización |
| Management API | `/api/v1/management` | JWT + membresía/rol | Usuario del panel en las organizaciones accesibles |
| Platform API | `/api/v1/platform` | JWT + `super_admin` activo | Operaciones globales auditadas ([ADR-0011](../adr/0011-admin-identity.md)) |
| Webhook API | `/api/v1/webhooks` | Secreto/firma por proveedor y cuenta | Cuenta de proveedor resuelta en el backend |

Operaciones concretas: [openapi.yaml](openapi.yaml) y su resumen en [endpoint-inventory.md](endpoint-inventory.md). No se exponen endpoints globales a API Keys de aplicaciones.

## Estado público de notificación

`ACCEPTED`, `QUEUED`, `PROCESSING`, `SENT`, `DELIVERED`, `READ`, `FAILED`, `CANCELLED` (esquema `NotificationPublicStatus`). Es un resumen derivado del estado de entrega ([ADR-0012](../adr/0012-delivery-semantics.md)):

- `SENT` significa que el proveedor **aceptó** el mensaje, no que se entregó.
- `DELIVERED` y `READ` solo aparecen si el proveedor ofrece esa evidencia; muchos canales nunca los alcanzan (por ejemplo, SMTP no confirma entrega ni lectura).
- Mientras una entrega requiere conciliación por un resultado ambiguo, el estado público es `PROCESSING`. Los estados internos de entrega no se exponen.

## Reglas de creación de notificaciones

- Debe especificarse exactamente una modalidad de contenido: plantilla o contenido directo, según el canal.
- Para WhatsApp se resuelve el `senderId` explícito o el remitente predeterminado de la aplicación, que se fija con `defaultSenderId` en la asignación de remitentes de la aplicación. Sin predeterminado válido → `422 DEFAULT_SENDER_NOT_CONFIGURED`. El predeterminado es **siempre por aplicación** (`application_senders.is_default`): el remitente no tiene un atributo de predeterminado propio y no existe un predeterminado global por organización (TP-33).
- El remitente debe estar activo y asignado a la aplicación (`422 SENDER_NOT_CONNECTED` / `422 SENDER_NOT_ASSIGNED_TO_APPLICATION`). Un remitente de otra organización → `404`.
- La cuota WhatsApp se reserva en la transacción de aceptación. Al superarla → `429 WHATSAPP_DAILY_QUOTA_EXCEEDED` con `Retry-After` y `retryAt`.
- La misma `Idempotency-Key` con el mismo hash devuelve el recurso original; con un cuerpo distinto → `409 IDEMPOTENCY_KEY_CONFLICT`.
- `attachmentIds[]`: adjuntos de la misma aplicación, asociados mediante `notification_attachments` sin duplicar el archivo. Un adjunto eliminado, expirado o bloqueado por el escaneo → `422 ATTACHMENT_NOT_AVAILABLE`; de otra aplicación u organización → `404` (TP-18).

## Lotes, programación, cifrado y metadatos

- **Lotes (TP-43):** `POST /public/notifications/bulk` acepta los ítems de forma **parcial**. Cada ítem se valida y acepta por separado; los rechazados se informan en `AcceptedBatch.items` con el `code` que tendrían en un envío individual. La respuesta es `202` aunque se rechacen todos (`status = REJECTED`). Solo hay errores del lote completo por formato (`400`), idempotencia (`409`) o rate limit (`429 RATE_LIMIT_EXCEEDED`). El lote se persiste y es idempotente.
- **Programación (TP-44):** se valida al aceptar (`scheduledAt` o `createScheduledNotification`), pero la cuota diaria se reserva **al ejecutarse**, en el día local de ejecución de la organización. Una ejecución que falla deja la programación en `FAILED` con `failureCode` y no se reprograma automáticamente.
- **Cifrado (TP-47):** `security.encryption` admite `NONE` (por defecto, en todos los canales), `TLS_ONLY` (EMAIL), `ZIP_AES256` (EMAIL y WHATSAPP, solo con adjuntos y `zipPassword`) y `PGP` (EMAIL, con `pgpPublicKey`). SMS solo admite `NONE`. Una combinación no válida → `422 ENCRYPTION_OPTIONS_INVALID`. El cifrado lo aplica el servicio, no el proveedor; `zipPassword` nunca se devuelve, registra ni publica en mensajes.
- **Metadatos:** `metadata` es un mapa libre del cliente que se persiste tal cual. No se usa para autorización ni debe contener secretos.
- **Recursos de Management con aplicación (TP-53):** `createTemplate`, `createCampaign` y `createScheduledNotification` exigen `applicationId`, que debe pertenecer a la organización seleccionada (si no, `404`). No es fuente de autoridad: solo identifica el recurso dentro del tenant ya autorizado.

## Sesión y contraseñas del panel

- `resumeSession` (`POST /management/auth/session`) recupera la sesión tras recargar el panel, con la cookie `refresh` y un `Origin` obligatorio, sin `X-CSRF-Token` (TP-41). Los códigos `401`/`403` de las operaciones de sesión se mantienen distintos (TP-42).
- Política de contraseñas NIST SP 800-63B (12 a 128 caracteres, lista de comunes y filtradas). Recuperación por reasignación de una contraseña temporal (`resetUserTemporaryPassword`), sin autoservicio (TP-55).

## Límites y API Keys

- **Límites de organización (TP-45):** `GET/PUT /platform/organizations/{organizationId}/limits`, solo `super_admin`, con motivo auditado. WhatsApp parte de 500 por día. El límite efectivo de una aplicación es el menor entre el suyo y el de su organización.
- **Permisos de aplicación (TP-48):** `setApplicationPermissions` fija el techo de scopes. Crear una API Key con un scope fuera de él → `422 API_KEY_SCOPE_NOT_PERMITTED`.
- **Rotación (TP-52):** `rotateApiKey` crea una clave nueva (nuevo `id`) y revoca la anterior en la misma transacción, sin solape.
- **Cuenta de proveedor predeterminada (TP-49):** como máximo una por organización y canal (`isDefault`). Se usa en Email y SMS.

## Reintento técnico, reenvío manual y conciliación

Semántica normativa en [ADR-0012](../adr/0012-delivery-semantics.md) (TP-19):

- **Reintento técnico y conciliación:** son internos. No se exponen como operaciones ni consumen cuota.
- **Reenvío manual:** `POST /public/notifications/{notificationId}/retry`. Lo solicita la **aplicación consumidora** con su API Key y el scope `notifications.retry`; el actor es la aplicación, nunca un usuario humano. Crea una nueva notificación vinculada (`resendOfNotificationId`), exige una `Idempotency-Key` nueva y un motivo, y consume cuota una vez. No existe operación de reenvío para usuarios del panel en v1. Una notificación `ZIP_AES256` no se puede reenviar, porque su secreto se purga al llegar a un estado terminal: `409 NOTIFICATION_NOT_RETRYABLE` (TP-54).

## Cancelación de notificaciones (TP-36)

- Dos puntos de entrada y **un solo caso de uso**: `POST /public/notifications/{notificationId}/cancel` (aplicación, API Key con scope `notifications.cancel`) y `POST /management/notifications/{notificationId}/cancel` (usuario del panel, JWT). El endpoint solo traduce la autenticación al actor (`APPLICATION` o `USER`) y delega en el caso de uso, que aplica el tenant, las reglas de estado y la auditoría. No se duplica la lógica.
- Autorización en Management: membresía activa en la organización seleccionada y permiso de rol `notifications.cancel` (`organization_admin`; `operator` solo si se le concede; `viewer` nunca). `super_admin` cancela en cualquier organización, con auditoría ([rbac-matrix.md](../security/rbac-matrix.md)).
- Estados cancelables: entrega `PENDING`, `QUEUED` o `RETRY_PENDING` (público `ACCEPTED` o `QUEUED`), incluidas las notificaciones programadas que aún no se ejecutaron. La cancelación lleva la entrega a `CANCELLED` ([ADR-0012](../adr/0012-delivery-semantics.md)).
- Una entrega que ya salió hacia el proveedor (`IN_PROGRESS` o posterior: `PROVIDER_ACCEPTED`, `RECONCILIATION_REQUIRED`, `DELIVERED`, `READ`, `FAILED`, `DEAD_LETTER`) **no** se cancela ni se reescribe como `CANCELLED`: se responde `409 NOTIFICATION_ALREADY_DISPATCHED`.
- Resultados, **idénticos en ambas APIs** (TP-39):

  | Situación | HTTP | `code` |
  |---|---|---|
  | Cancelación correcta | `200` | — (`ActionResult`) |
  | Ya cancelada | `409` | `RESOURCE_STATE_CONFLICT` |
  | Ya despachada | `409` | `NOTIFICATION_ALREADY_DISPATCHED` |
  | Inexistente o fuera del ámbito autorizado | `404` | `RESOURCE_NOT_FOUND` |
  | Sin permiso (rol en Management, scope en Public) | `403` | `PERMISSION_DENIED` |

- La cancelación repetida **no** es idempotente: nunca responde `200`. Se mantiene la distinción entre "ya cancelada" y "ya despachada".

## Adjuntos (TP-28)

- `POST /public/attachments` exige `attachments.upload`; `GET /public/attachments/{attachmentId}` exige `attachments.read`. Son scopes independientes: subir no autoriza leer.
- La lectura se limita a la organización y la aplicación propietarias del adjunto; un adjunto ajeno responde `404`.
- `downloadUrl` es una URL firmada temporal (`expiresAt`) que no expone credenciales, endpoints internos, bucket ni clave de objeto de MinIO.

## Membresías y roles

- Los roles de organización (`organization_admin`, `operator`, `viewer`) se asignan **siempre** a una membresía. No existe asignación de roles directamente a un usuario. El catálogo de roles es de solo lectura (TP-20).
- El estado de una membresía (activa, suspendida, desactivada) controla el acceso del usuario a **esa** organización, sin afectar a sus otras membresías. El estado global del usuario (`PATCH /users/{id}/status`) afecta a todas sus organizaciones.
- Una organización no puede quedarse sin un `organization_admin` activo: suspender o desactivar la última membresía administradora, retirarle el rol o suspender al usuario → `422 LAST_ORGANIZATION_ADMIN_REQUIRED`, con validación transaccional y bloqueo (TP-35, [ADR-0011](../adr/0011-admin-identity.md)).
- `super_admin` es un privilegio de plataforma (`platform_role_assignments`), no un rol de membresía. El primero se crea por bootstrap ([super-admin-bootstrap.md](../operations/super-admin-bootstrap.md)); los siguientes, por la Platform API (TP-15).

## Diagnóstico de cuentas de proveedor

La prueba de conexión y la consulta de salud de una cuenta usan `IProviderAccountDiagnostics` (TP-17). Devuelven `200` con un resultado neutral también cuando el proveedor falla. Si el proveedor no admite el diagnóstico → `422 PROVIDER_DIAGNOSTICS_NOT_SUPPORTED`.

## Webhooks

Los payloads externos son específicos de cada versión y proveedor. Validar la autenticidad antes de procesar, persistir el evento antes de responder y deduplicar. El contrato de cada webhook se documenta en [webhook-contract-readiness.md](../providers/webhook-contract-readiness.md) después de capturar payloads reales.

## Códigos HTTP

Éxito:

- `200`: consulta o acción completada.
- `201`: recurso creado.
- `202`: trabajo aceptado para procesamiento asíncrono.
- `204`: acción sin cuerpo, solo en operaciones expresamente definidas.

Errores (semántica aprobada PD-04; catálogo de `code` en [error-codes.md](error-codes.md)):

- `400`: formato, sintaxis o estructura inválidos (esquema, campo requerido, tipo, cabecera obligatoria ausente).
- `401`: credencial ausente o inválida.
- `403`: autenticado sin permiso.
- `404`: recurso inexistente o fuera del ámbito visible.
- `409`: conflicto de estado del recurso o de idempotencia.
- `413`: archivo o cuerpo demasiado grande.
- `415`: tipo de contenido no soportado.
- `422`: solicitud bien formada que incumple una regla de negocio.
- `429`: cuota o rate limit excedido; incluir `Retry-After` cuando sea calculable.
- `500`: error inesperado (posible en cualquier operación; no se declara una por una en OpenAPI).
- `503`: dependencia temporalmente no disponible; incluir `Retry-After` cuando sea calculable.

En `openapi.yaml`, cada operación declara solo las respuestas que corresponden a su comportamiento (PD-08). Las respuestas reutilizables están en `components.responses`.

**Rate limiting (`429`):** se aplica a la Public API (límite de solicitudes por minuto por organización/aplicación según [quotas-retention.md](../operations/quotas-retention.md)) y a las operaciones de la Management API que consumen cuota (inicio de campaña). No hay rate limiting documentado para el resto de la Management API ni para la autenticación; si se añade, debe declararse en OpenAPI.

## Respuestas de error

RFC 9457 Problem Details con extensiones. Formato completo en [error-codes.md](error-codes.md); `{PROBLEM_TYPE_BASE_URI}` se fijará con el DNS definitivo:

```json
{
  "type": "{PROBLEM_TYPE_BASE_URI}/whatsapp-daily-quota-exceeded",
  "title": "Quota exceeded",
  "status": 429,
  "detail": "La organización alcanzó su cuota diaria de WhatsApp.",
  "instance": "/api/v1/public/notifications",
  "code": "WHATSAPP_DAILY_QUOTA_EXCEEDED",
  "traceId": "00-...",
  "retryAt": "2026-11-15T05:00:00Z"
}
```
