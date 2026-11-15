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

Máquina de estados de tres niveles aprobada (PD-02); definición normativa en [ADR-0012](../adr/0012-delivery-semantics.md). Sustituye al flujo único anterior (`ACCEPTED → QUEUED → PROCESSING → SENT → DELIVERED → READ`).

| Nivel | Valores | Uso en mensajería |
|---|---|---|
| Intento (`notification_attempts.result_code`) | `PROVIDER_ACCEPTED`, `TRANSIENT_FAILURE`, `PERMANENT_FAILURE`, `AMBIGUOUS` | Resultado de cada ejecución del consumidor contra el proveedor |
| Entrega (`notification_deliveries.status`) | `PENDING`, `QUEUED`, `IN_PROGRESS`, `PROVIDER_ACCEPTED`, `RETRY_PENDING`, `RECONCILIATION_REQUIRED`, `DELIVERED`, `READ`, `FAILED`, `DEAD_LETTER`, `CANCELLED` | Estado técnico que gobiernan Outbox, consumidores, colas de retry y DLQ |
| Público (`notifications.status`) | `ACCEPTED`, `QUEUED`, `PROCESSING`, `SENT`, `DELIVERED`, `READ`, `FAILED`, `CANCELLED` | Resumen derivado de la entrega que ve la API |

Correspondencia con el flujo de colas:

- Outbox publicado → entrega `QUEUED`. El consumidor la toma → `IN_PROGRESS`.
- `TRANSIENT_FAILURE` con intentos restantes → `RETRY_PENDING` y mensaje a `notification.retry` con backoff exponencial y jitter → `QUEUED` al vencer.
- `PERMANENT_FAILURE` o intentos agotados → `FAILED`. Si el mensaje no es procesable o requiere intervención → `DEAD_LETTER` (`notification.dlx`).
- `AMBIGUOUS` (timeout o caída después de enviar) → `RECONCILIATION_REQUIRED`: **no** se publica en la cola de retry ni se reenvía. La conciliación consulta al proveedor y lleva la entrega a `PROVIDER_ACCEPTED` o a `FAILED`. Mientras tanto, el estado público es `PROCESSING`.
- Cada transición de entrega se registra en `delivery_status_history`.

`SENT` (entrega `PROVIDER_ACCEPTED`) significa que el proveedor aceptó la solicitud, no que el destinatario la recibió. `DELIVERED` y `READ` solo se establecen con evidencia del proveedor. No todos los canales/proveedores los soportan.

## Garantías

Se busca entrega de procesamiento al menos una vez (at-least-once) y efectos idempotentes cuando el proveedor lo permita. No prometer exactly-once extremo a extremo.
