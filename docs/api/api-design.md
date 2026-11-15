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
- API Keys de aplicaciones en `X-API-Key`; usuarios del panel con access JWT Bearer.
- El contexto de organización se deriva de las credenciales y la membresía. Un `organizationId` en ruta o cuerpo nunca sustituye a la autorización.
- Paginación `page`/`pageSize` solo en colecciones; máximo 100; orden estable por fecha e ID. Las consultas de un recurso individual no se paginan.
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
- Para WhatsApp se resuelve el `senderId` explícito o el remitente predeterminado de la aplicación, que se fija con `defaultSenderId` en la asignación de remitentes de la aplicación. Sin predeterminado válido → `422 DEFAULT_SENDER_NOT_CONFIGURED`.
- El remitente debe estar activo y asignado a la aplicación (`422 SENDER_NOT_CONNECTED` / `422 SENDER_NOT_ASSIGNED_TO_APPLICATION`). Un remitente de otra organización → `404`.
- La cuota WhatsApp se reserva en la transacción de aceptación. Al superarla → `429 WHATSAPP_DAILY_QUOTA_EXCEEDED` con `Retry-After` y `retryAt`.
- La misma `Idempotency-Key` con el mismo hash devuelve el recurso original; con un cuerpo distinto → `409 IDEMPOTENCY_KEY_CONFLICT`.
- `attachmentIds[]`: adjuntos de la misma aplicación, asociados mediante `notification_attachments` sin duplicar el archivo. Un adjunto eliminado, expirado o bloqueado por el escaneo → `422 ATTACHMENT_NOT_AVAILABLE`; de otra aplicación u organización → `404` (TP-18).

## Reintento técnico, reenvío manual y conciliación

Semántica normativa en [ADR-0012](../adr/0012-delivery-semantics.md) (TP-19, TP-21):

- **Reintento técnico y conciliación:** son internos. No se exponen como operaciones ni consumen cuota.
- **Reenvío manual:** `POST /public/notifications/{notificationId}/retry`. Lo solicita la **aplicación consumidora** con su API Key y el scope `notifications.retry`; el actor es la aplicación, nunca un usuario humano. Crea una nueva notificación vinculada (`resendOfNotificationId`), exige una `Idempotency-Key` nueva y un motivo, y consume cuota una vez. No existe operación de reenvío para usuarios del panel en v1.

## Membresías y roles

- Los roles de organización (`organization_admin`, `operator`, `viewer`) se asignan **siempre** a una membresía. No existe asignación de roles directamente a un usuario. El catálogo de roles es de solo lectura (TP-20).
- El estado de una membresía (activa, suspendida, desactivada) controla el acceso del usuario a **esa** organización, sin afectar a sus otras membresías. El estado global del usuario (`PATCH /users/{id}/status`) afecta a todas sus organizaciones.
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
