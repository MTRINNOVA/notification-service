# Contrato de API — reglas definitivas v1.5

## Convenciones

- Base path: `/api/v1`.
- REST sobre HTTPS; JSON UTF-8 para operaciones de negocio.
- UUID para identificadores internos; fechas RFC 3339 en UTC (`Z`).
- `X-Correlation-Id` se acepta o genera y se devuelve en respuesta.
- `Idempotency-Key` obligatorio para crear envíos, lotes, campañas y programaciones.
- API Keys de aplicaciones en `X-API-Key`; usuarios del panel con access JWT Bearer.
- El contexto de organización se deriva de credenciales y membresía. Un `organizationId` en ruta/cuerpo nunca sustituye autorización.
- Paginación `page`/`pageSize` en v1, máximo 100; orden estable por fecha e ID.
- Errores RFC 9457 `application/problem+json` con `type`, `title`, `status`, `detail`, `instance`, `code` y `traceId`; `errors` y `retryAt` cuando correspondan. Valores de `code`: [error-codes.md](error-codes.md).
- `202 Accepted` confirma persistencia/aceptación, no envío ni entrega.
- Archivos mediante `multipart/form-data`; respuesta JSON con `attachmentId`.
- Zona horaria: las aplicaciones heredan la de su organización; en v1 no existe zona horaria por aplicación (PD-05). Las horas locales de programación se interpretan en la zona de la organización y se almacenan en UTC.

## Estado público de notificación

`ACCEPTED`, `QUEUED`, `PROCESSING`, `SENT`, `DELIVERED`, `READ`, `FAILED`, `CANCELLED` (esquema `NotificationPublicStatus` en OpenAPI). Es un resumen derivado del estado de entrega ([ADR-0012](../adr/0012-delivery-semantics.md)):

- `SENT` significa que el proveedor **aceptó** el mensaje, no que se entregó.
- `DELIVERED` y `READ` solo aparecen si el proveedor ofrece esa evidencia; muchos canales nunca los alcanzan (por ejemplo, SMTP no confirma entrega ni lectura).
- Mientras una entrega requiere conciliación por un resultado ambiguo, el estado público es `PROCESSING`. Los estados internos de entrega no se exponen.

## Seguridad por tipo de API

| API | Autenticación | Ámbito |
|---|---|---|
| Public API | `X-API-Key` | Una aplicación y su organización |
| Management API | JWT + membresía/rol | Organizaciones accesibles al usuario |
| Platform API | JWT + `super_admin` | Operaciones globales auditadas |
| Webhook API | Secreto/firma por proveedor y cuenta | Cuenta de proveedor resuelta en backend |

## Public API — aplicaciones consumidoras

Base `/api/v1/public`.

| Método y ruta | Descripción | Resultado principal |
|---|---|---|
| `POST /notifications` | Crear envío individual | `202` + `notificationId` |
| `POST /notifications/bulk` | Crear lote acotado | `202` + `batchId` y resumen |
| `GET /notifications` | Listar envíos propios | `200` paginado |
| `GET /notifications/{notificationId}` | Consultar estado | `200` |
| `GET /notifications/{notificationId}/events` | Timeline de eventos | `200` |
| `POST /notifications/{notificationId}/cancel` | Cancelar si aún no se despachó | `200` o `409` |
| `POST /notifications/{notificationId}/retry` | Solicitar reintento autorizado | `202` o `409` |
| `POST /attachments` | Subir adjunto | `201` |
| `GET /attachments/{attachmentId}` | Consultar metadatos/URL firmada | `200` |
| `GET /templates` | Listar plantillas publicadas de la app | `200` |
| `GET /templates/{code}` | Obtener plantilla/versiones disponibles | `200` |

### Crear notificación

`POST /public/notifications`

Headers: `X-API-Key`, `Idempotency-Key`, `X-Correlation-Id` opcional.

Campos: `channel` (`WHATSAPP`, `EMAIL`, `SMS`), `recipient`, `senderId` opcional, `template` o `content`, `attachmentIds[]` opcional, `scheduledAt` opcional y `metadata` no sensible.

Reglas:
- Debe especificarse exactamente una modalidad de contenido: plantilla o contenido directo, sujeto a canal.
- Para WhatsApp se resuelve `senderId` explícito o remitente default asignado a la aplicación; sin default válido → `422 DEFAULT_SENDER_NOT_CONFIGURED`.
- El remitente debe estar activo y asignado a la aplicación (`422 SENDER_NOT_CONNECTED` / `422 SENDER_NOT_ASSIGNED_TO_APPLICATION`; un remitente de otra organización → `404`).
- La cuota WhatsApp se reserva en la transacción de aceptación; al superarla → `429 WHATSAPP_DAILY_QUOTA_EXCEEDED` con `Retry-After` y `retryAt`.
- Un mismo `Idempotency-Key` y mismo hash devuelve el recurso original; mismo key con request distinto produce `409 IDEMPOTENCY_KEY_CONFLICT`.

## Management API — panel administrativo

Base `/api/v1/management`.

| Área | Operaciones |
|---|---|
| Organizaciones | `GET/POST /organizations`; `GET/PUT /organizations/{id}`; `PATCH /organizations/{id}/status` |
| Membresías | `GET/POST /organizations/{id}/memberships`; `PUT /organizations/{id}/memberships/{membershipId}/roles`; `PATCH .../status` |
| Usuarios | `GET/POST /users`; `GET/PUT /users/{id}`; `PATCH /users/{id}/status` |
| Roles | `GET/POST /roles`; `PUT /roles/{id}`; `PUT /roles/{id}/permissions` |
| Aplicaciones | `GET/POST /applications`; `GET/PUT /applications/{id}`; `PATCH /applications/{id}/status`; `PUT /applications/{id}/permissions`; `PUT /applications/{id}/limits` |
| API Keys | `GET/POST /applications/{id}/api-keys`; `POST /applications/{id}/api-keys/{keyId}/rotate`; `DELETE /applications/{id}/api-keys/{keyId}` |
| Proveedores | `GET /providers/definitions`; `GET/POST /providers/accounts`; `GET/PUT/DELETE /providers/accounts/{id}`; `POST .../{id}/test`; `GET .../{id}/health`; `POST .../{id}/enable`; `POST .../{id}/disable`; `POST .../{id}/credentials/rotate` |
| Instancias | `GET/POST /providers/accounts/{id}/instances`; `POST /providers/instances/{id}/connect`; `GET /providers/instances/{id}/qr`; `GET /providers/instances/{id}/status`; `POST .../{id}/disconnect`; `POST .../{id}/reconnect`; `DELETE .../{id}` |
| Remitentes | `GET /senders`; `GET /senders/{id}`; `PUT /applications/{id}/senders`; `PATCH /senders/{id}/default` |
| Plantillas | `GET/POST /templates`; `GET/PUT/DELETE /templates/{id}`; `POST /templates/{id}/versions`; `GET .../versions`; `POST .../preview`; `POST .../test` |
| Campañas | `GET/POST /campaigns`; `GET/PUT /campaigns/{id}`; `POST .../{id}/start`; `POST .../{id}/pause`; `POST .../{id}/cancel`; `GET .../{id}/statistics`; `GET .../{id}/recipients` |
| Programación | `GET/POST /scheduled-notifications`; `GET /scheduled-notifications/{id}`; `DELETE /scheduled-notifications/{id}` |
| Dashboard | `GET /dashboard/summary`; `GET /dashboard/deliveries`; `GET /dashboard/providers-health`; `GET /dashboard/consumption` |
| Auditoría | `GET /audit/events`; `GET /audit/notifications/{notificationId}` |

## Platform API

`super_admin` es un privilegio de plataforma (`platform_role_assignments`), no un rol de membresía ([ADR-0011](../adr/0011-admin-identity.md)). Las operaciones globales de `super_admin` deben estar separadas lógicamente del panel de organización y siempre auditadas. El conjunto inicial incluye gestión de organizaciones, catálogo de proveedores, límites globales, estado de plataforma y consulta de auditoría. No exponer endpoints globales a API Keys de aplicaciones.

## Webhook API

Base `/api/v1/webhooks`:

- `POST /whatsapp/evolution/{providerAccountId}`
- `POST /email/{providerAccountId}`
- `POST /sms/{providerAccountId}`

Los payloads externos son específicos de versión/proveedor. Validar autenticidad antes de procesar, persistir el evento antes de responder y deduplicar. El contrato exacto de cada webhook se documenta en `providers/webhook-contract-readiness.md` después de capturar payloads reales.

## Códigos HTTP

- `200`: consulta o acción completada.
- `201`: recurso creado.
- `202`: trabajo aceptado para procesamiento asíncrono.
- `204`: acción sin cuerpo solo para operaciones expresamente definidas.
Semántica aprobada (PD-04); catálogo de `code` en [error-codes.md](error-codes.md):

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

En `openapi.yaml`, cada operación declara solo las respuestas que corresponden a su comportamiento (PD-08); las respuestas reutilizables están en `components.responses`.

**Rate limiting (`429`):** se aplica a la Public API (límite de solicitudes por minuto por organización/aplicación según [quotas-retention.md](../operations/quotas-retention.md)) y a las operaciones de la Management API que consumen cuota (inicio de campaña). No hay rate limiting documentado para el resto de la Management API ni para la autenticación; si se añade, debe declararse en OpenAPI.

## Respuestas de error

Usar RFC 9457 Problem Details con extensiones (formato completo en [error-codes.md](error-codes.md); `{PROBLEM_TYPE_BASE_URI}` se fijará con el DNS definitivo):

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

## Nota de versionado

`openapi.yaml` es el contrato normativo de rutas y esquemas. Cualquier endpoint nuevo o cambio incompatible requiere actualización del YAML, revisión de seguridad, pruebas de contrato y changelog. El inventario de rutas no autoriza implementar payloads no definidos en OpenAPI.
