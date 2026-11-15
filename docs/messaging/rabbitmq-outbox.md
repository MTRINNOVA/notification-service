# Mensajería, Outbox, reintentos y estados

## Exchanges y colas iniciales

| Exchange | Routing key / cola | Propósito |
|---|---|---|
| `notification.commands` (direct/topic) | `notification.email` | Entregas email |
| `notification.commands` | `notification.whatsapp` | Entregas WhatsApp |
| `notification.commands` | `notification.sms` | Entregas SMS |
| `notification.retry` | colas de retry por canal | Reintentos diferidos |
| `notification.events` | `notification.status` | Eventos de estado |
| `notification.dlx` | `notification.dead-letter` | Mensajes no procesables |

Usar colas durables, mensajes persistentes, publisher confirms, acknowledgements manuales, prefetch limitado y DLX. Definir TTL/retry queues de acuerdo con política operativa; no implementar bucles inmediatos.

## Outbox

En una misma transacción PostgreSQL:
1. Insertar `notifications`.
2. Insertar `notification_deliveries`.
3. Insertar `outbox_messages`.
4. Confirmar transacción.

Un publicador lee filas pendientes con locking seguro (`FOR UPDATE SKIP LOCKED` o mecanismo equivalente), publica con confirmación y marca `published_at`. El proceso debe tolerar publicación repetida; el consumidor usa `inbox_messages` y una clave de deduplicación.

## Reintentos

- Clasificar errores transitorios y permanentes.
- Backoff exponencial con jitter y límite de intentos.
- Respetar `Retry-After` del proveedor.
- No reintentar destinatario inválido o credencial inválida sin intervención.
- Timeout después de enviar puede ser resultado ambiguo: consultar estado del proveedor si existe, no reenviar ciegamente.
- Aplicar concurrencia y rate limit por proveedor/cuenta/canal.

## Estados

`ACCEPTED -> QUEUED -> PROCESSING -> SENT -> DELIVERED -> READ`

Ramas:
- `PROCESSING -> RETRY_PENDING -> QUEUED`
- `PROCESSING -> FAILED -> DEAD_LETTER`
- `ACCEPTED/QUEUED/RETRY_PENDING -> CANCELLED`

`SENT` significa que el proveedor aceptó la solicitud, no que el destinatario la recibió. `DELIVERED` y `READ` solo se establecen con evidencia del proveedor. No todos los canales/proveedores soportan todos los estados.

## Garantías

Se busca entrega de procesamiento al menos una vez (at-least-once) y efectos idempotentes cuando el proveedor lo permita. No prometer exactly-once extremo a extremo.
