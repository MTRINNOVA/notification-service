# ADR-0012: Semántica de entrega

- Estado: Aprobado (ampliado el 2026-11-15 con la máquina de estados de PD-02 y la semántica de TP-19)
- Decisión: persistencia transaccional + Outbox + consumo al menos una vez; idempotencia en el servicio y deduplicación de webhooks.
- No prometer exactly-once extremo a extremo.
- En timeout ambiguo consultar estado externo cuando sea posible y evitar reenvío ciego.

## Ampliación 2026-11-15 — Máquina de estados de tres niveles (PD-02, aprobada)

### Contexto

El flujo único `ACCEPTED → QUEUED → PROCESSING → SENT → DELIVERED → READ` de [rabbitmq-outbox.md](../messaging/rabbitmq-outbox.md) no tenía forma de representar el resultado ambiguo exigido por esta misma ADR. [provider-contracts-v1.5.md](../providers/provider-contracts-v1.5.md) lo nombraba `UNKNOWN/RECONCILIATION_REQUIRED`. Ese flujo único queda **sustituido** por los tres niveles siguientes. Historial: [PENDING-DECISIONS.md, PD-02](../project/PENDING-DECISIONS.md#pd-02-máquina-de-estados-de-notificación-entrega-e-intento).

### Decisión vigente

**1. Resultado de intento** (`notification_attempts.result_code`). Una fila inmutable por cada ejecución individual contra el proveedor:

| Valor               | Significado                                                                                    | Consecuencia                                                                             |
| ------------------- | ---------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------- |
| `PROVIDER_ACCEPTED` | El proveedor aceptó la solicitud                                                               | La entrega pasa a `PROVIDER_ACCEPTED`                                                    |
| `TRANSIENT_FAILURE` | Error recuperable (timeout antes de enviar, 5xx, 429 del proveedor)                            | Reintento con backoff exponencial, jitter y límite de intentos, respetando `Retry-After` |
| `PERMANENT_FAILURE` | Error no recuperable (destinatario o credencial inválidos)                                     | Sin reintento automático                                                                 |
| `AMBIGUOUS`         | No se sabe con certeza si la operación externa tuvo efecto (timeout o caída después de enviar) | La entrega pasa a `RECONCILIATION_REQUIRED`; **nunca** reenvío ciego                     |

**2. Estado de entrega** (`notification_deliveries.status`). Ciclo técnico frente al proveedor:

`PENDING`, `QUEUED`, `IN_PROGRESS`, `PROVIDER_ACCEPTED`, `RETRY_PENDING`, `RECONCILIATION_REQUIRED`, `DELIVERED`, `READ`, `FAILED`, `DEAD_LETTER`, `CANCELLED`.

```mermaid
stateDiagram-v2
  [*] --> PENDING
  PENDING --> QUEUED : outbox publicado
  QUEUED --> IN_PROGRESS : consumidor toma el mensaje
  IN_PROGRESS --> PROVIDER_ACCEPTED : intento PROVIDER_ACCEPTED
  IN_PROGRESS --> RETRY_PENDING : intento TRANSIENT_FAILURE con intentos restantes
  RETRY_PENDING --> QUEUED : vence el backoff
  IN_PROGRESS --> FAILED : PERMANENT_FAILURE o intentos agotados
  IN_PROGRESS --> RECONCILIATION_REQUIRED : intento AMBIGUOUS
  RECONCILIATION_REQUIRED --> PROVIDER_ACCEPTED : conciliación confirma la aceptación
  RECONCILIATION_REQUIRED --> FAILED : conciliación confirma que no se recibió
  PROVIDER_ACCEPTED --> DELIVERED : evidencia del proveedor
  DELIVERED --> READ : evidencia del proveedor
  PROVIDER_ACCEPTED --> FAILED : rebote o fallo informado por el proveedor
  FAILED --> DEAD_LETTER : mensaje no procesable o requiere intervención
  PENDING --> CANCELLED
  QUEUED --> CANCELLED
  RETRY_PENDING --> CANCELLED
```

Reglas:

- `RECONCILIATION_REQUIRED` **no** genera reintento automático. La conciliación consulta al proveedor cuando su contrato lo permite y confirma la aceptación o la no recepción.
- Los reenvíos manuales requieren autorización y quedan auditados. Nunca son automáticos ni reutilizan en silencio un intento ambiguo. Su semántica está en la sección TP-19 de esta ADR.
- `PROVIDER_ACCEPTED` (público: `SENT`) significa que el proveedor aceptó el mensaje, no que se entregó.
- `DELIVERED` y `READ` solo se registran con evidencia del proveedor y según sus capacidades declaradas. No todos los proveedores las ofrecen (ver la tabla de capacidades en [provider-adapters.md](../providers/provider-adapters.md)).
- Los reintentos técnicos no consumen cuota adicional.

**3. Estado público de notificación** (`notifications.status`, expuesto en la API). Resumen derivado de la entrega vigente de la notificación (en v1, una entrega por canal/destinatario):

| Estado de entrega                        | Estado público |
| ---------------------------------------- | -------------- |
| `PENDING`                                | `ACCEPTED`     |
| `QUEUED`, `RETRY_PENDING`                | `QUEUED`       |
| `IN_PROGRESS`, `RECONCILIATION_REQUIRED` | `PROCESSING`   |
| `PROVIDER_ACCEPTED`                      | `SENT`         |
| `DELIVERED`                              | `DELIVERED`    |
| `READ`                                   | `READ`         |
| `FAILED`, `DEAD_LETTER`                  | `FAILED`       |
| `CANCELLED`                              | `CANCELLED`    |

`RECONCILIATION_REQUIRED` **no** se expone como estado público: el cliente observa `PROCESSING` mientras dura la conciliación.

**4. Historiales.** `notification_status_history` registra los cambios del estado público. `delivery_status_history` registra cada transición de la entrega, con actor y `reason_code` ([erd.md](../data/erd.md)).

### Consecuencias

- El ERD, OpenAPI (`NotificationPublicStatus`), [rabbitmq-outbox.md](../messaging/rabbitmq-outbox.md) y los contratos de proveedores usan exclusivamente estos nombres.
- El término `UNKNOWN` queda retirado como estado. El resultado ambiguo se llama `AMBIGUOUS` en el intento y `RECONCILIATION_REQUIRED` en la entrega.
- El plazo y la periodicidad de la conciliación son configurables por proveedor (sección TP-19). Sus valores concretos se fijan al validar el contrato real de cada proveedor.

## Ampliación 2026-11-15 — Reintento técnico, reenvío manual y conciliación (TP-19, aprobada)

Tres operaciones distintas que **no** alteran la máquina de estados de PD-02:

|              | Reintento técnico                                                                     | Reenvío manual                                                                                                                                                              | Conciliación                                                        |
| ------------ | ------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------- |
| Qué es       | Nueva ejecución automática contra el proveedor                                        | Nuevo envío solicitado por la aplicación consumidora (API Key con scope `notifications.retry`)                                                                                                                              | Determinar el resultado real de una entrega ambigua                 |
| Notificación | La misma                                                                              | **Nueva**, con `resend_of_notification_id` → original                                                                                                                       | La misma                                                            |
| Entrega      | La misma (`RETRY_PENDING → QUEUED`)                                                   | **Nueva** entrega de la nueva notificación                                                                                                                                  | La misma (`RECONCILIATION_REQUIRED → PROVIDER_ACCEPTED` o `FAILED`) |
| Intento      | Nuevo `notification_attempt`                                                          | Intentos propios de la nueva entrega                                                                                                                                        | Ninguno de envío (solo consultas de estado)                         |
| Cuota        | No consume                                                                            | Consume **una vez** al aceptarse                                                                                                                                            | No consume                                                          |
| Idempotencia | Interna                                                                               | **Nueva** `Idempotency-Key`                                                                                                                                                 | Interna                                                             |
| Origen       | Automático (backoff exponencial, jitter, límite de intentos) tras `TRANSIENT_FAILURE` | `POST /public/notifications/{id}/retry` con motivo                                                                                                                          | Automático por el Scheduler o los webhooks del proveedor            |
| Prohibido    | Reintentar un intento `AMBIGUOUS` o un `PERMANENT_FAILURE`                            | Sobrescribir el historial de la original; reenviar si la original no está en `FAILED` o si su entrega sigue en `RECONCILIATION_REQUIRED` (`409 NOTIFICATION_NOT_RETRYABLE`) | Reenviar a ciegas                                                   |

**Reenvío manual (TP-21, aprobada 2026-11-15):** el actor es la **aplicación consumidora** autenticada con su API Key, nunca un usuario humano. Se registran la API Key solicitante (`resend_requested_by_api_key_id`, de la misma aplicación), el motivo (`resend_reason`), la fecha (`created_at`) y la referencia original. En `audit_logs` queda como `actor_type = APPLICATION`, con `actor_id` = aplicación y `api_key_id`. La notificación original queda intacta.

**Política de conciliación configurable por proveedor** (`provider_definitions` / configuración validada del adaptador; valores por definir al validar cada proveedor, sin plazo universal):

| Parámetro                 | Significado                                                                                                                                                                       |
| ------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `statusQueryCapability`   | Si el proveedor permite consultar el estado de un mensaje (`ProviderCapabilities`)                                                                                                |
| `maxReconciliationWindow` | Plazo máximo desde `reconciliation_started_at` (→ `reconciliation_deadline_at`)                                                                                                   |
| `reconciliationInterval`  | Intervalo entre consultas (→ `next_reconciliation_at`)                                                                                                                            |
| `maxReconciliationChecks` | Número máximo de consultas (`reconciliation_checks`)                                                                                                                              |
| `onDeadline`              | Acción al vencer: **siempre** marcar intervención operativa (`requires_manual_intervention = true`, `reason_code = RECONCILIATION_EXPIRED`) manteniendo `RECONCILIATION_REQUIRED` |

Reglas:

- Si el proveedor **no** permite consultar el estado, la conciliación depende de eventos (webhooks). Si no llegan antes del plazo, la entrega pasa a intervención manual. **Nunca se asume que el mensaje no se envió.**
- Cada consulta y cada evento se registran en `notification_events`, y cada cambio de estado en `delivery_status_history`.
- **Intervención manual:** un operador autorizado revisa la evidencia externa y resuelve, con motivo y auditoría:
  - aceptación confirmada → `PROVIDER_ACCEPTED`;
  - no recepción confirmada → `FAILED`, `reason_code = MANUAL_RESOLUTION`.
    Solo después puede solicitarse un reenvío manual. Mientras no se resuelva, la entrega sigue en `RECONCILIATION_REQUIRED` (estado público `PROCESSING`).
