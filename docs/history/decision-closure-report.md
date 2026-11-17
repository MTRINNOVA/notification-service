# Informe de cierre de decisiones PD-01 a PD-08 — 2026-11-15

> **Documento histórico** (evidencia de la auditoría del 2026-11-15). No es fuente operativa: el estado vigente de los pendientes está en [technical-pending-register.md](../operations/technical-pending-register.md) y las decisiones en [DECISIONS-v1.5.md](../project/DECISIONS-v1.5.md).

## 1. Resumen ejecutivo

Las ocho decisiones PD-01 a PD-08, **aprobadas expresamente por el usuario**, quedaron incorporadas a sus documentos normativos:

- tres ADR actualizados (0004, 0011 y 0012), conservando su número, contexto e historial;
- ERD reescrito como **modelo lógico aprobado para v1**;
- matriz RBAC;
- `openapi.yaml` con respuestas `409`/`422`/`429` por operación y componentes RFC 9457 reutilizables;
- catálogo cerrado de códigos de error (nuevo);
- documentación de mensajería, proveedores, aceptación y despliegue, `AGENTS.md` y `CLAUDE.md`.

La tarea fue **solo documental**. No se modificó código, proyectos ni configuración ejecutable. La auditoría posterior no encontró contradicciones vigentes entre ADR, ERD, RBAC y OpenAPI. Sí encontró **seis puntos abiertos** que estas decisiones no resuelven y que no se inventaron: TP-15 a TP-20, que requieren aprobación.

## 2. Decisiones aprobadas

| ID | Estado | Resultado |
|---|---|---|
| PD-01 | **Aprobada** | Roles v1: `super_admin` (privilegio de plataforma, `platform_role_assignments`), `organization_admin`, `operator`, `viewer`. Se retiran `org_admin` y `application_admin` |
| PD-02 | **Aprobada** | Estados en tres niveles (público, entrega e intento). `RECONCILIATION_REQUIRED` solo en la entrega. Nuevo historial `delivery_status_history` |
| PD-03 | **Aprobada** | Puertos especializados por canal; `INotificationProvider` e `IProviderConnectionManager` retirados |
| PD-04 | **Aprobada** | `400` formato · `409` conflicto · `422` regla de negocio. RFC 9457 con catálogo cerrado de `code` |
| PD-05 | **Aprobada** | Sin `applications.timezone_override` en v1 |
| PD-06 | **Aprobada** | Hosts por variables `NOTIFICATIONS_{API,ADMIN,WEBHOOKS}_HOST`; sin DNS reales; Traefik externo |
| PD-07 | **Aprobada** | Una imagen por host: `notification-api`, `notification-worker`, `notification-scheduler` |
| PD-08 | **Aprobada** | `409`/`422`/`429` (y `413`/`415`/`503` donde corresponde) declarados por operación |

## 3. Archivos modificados

**Creados:** `docs/api/error-codes.md` (catálogo de errores) y este informe.

**Modificados (25):**

| Área | Archivos |
|---|---|
| ADR | `docs/adr/0004-whatsapp-adapters.md`, `0011-admin-identity.md`, `0012-delivery-semantics.md` |
| Datos | `docs/data/erd.md` |
| API | `docs/api/openapi.yaml`, `docs/api/api-design.md` |
| Seguridad | `docs/security/rbac-matrix.md` |
| Arquitectura | `docs/architecture/hexagonal.md`, `docs/architecture/project-structure.md` |
| Mensajería y proveedores | `docs/messaging/rabbitmq-outbox.md`, `docs/providers/provider-adapters.md`, `docs/providers/provider-contracts-v1.5.md` |
| Aceptación | `docs/acceptance/criteria.md` |
| Despliegue | `docs/deployment/docker.md`, `compose-topology.md`, `environment-decisions.md`, `.env.example` |
| Proyecto | `docs/project/DECISIONS-v1.5.md`, `PENDING-DECISIONS.md`, `notification-architecture-index.md` |
| Operación | `docs/operations/repository-sanitization-report.md`, `technical-pending-register.md` |
| Raíz | `README.md`, `AGENTS.md`, `CLAUDE.md` |

**Sin cambios:** todo `src/`, `tests/`, `*.csproj`, `Directory.*.props`, `global.json`, Dockerfiles y los ADR no relacionados.

## 4. ADR actualizados

| ADR | Cambio | Historial conservado |
|---|---|---|
| ADR-0011 (identidad) | Nueva sección "Roles de v1" con tabla rol/tipo/asignación; regla de `422 ROLE_NOT_ASSIGNABLE` | Sección "Historial" con la lista original de roles sugeridos y el motivo de cada cambio |
| ADR-0012 (semántica de entrega) | "Ampliación 2026-11-15": tres niveles de estado, diagrama de transiciones, correspondencia entrega→público e historiales | Se mantienen las cuatro decisiones originales; se indica qué flujo queda sustituido y que `UNKNOWN` se retira |
| ADR-0004 (WhatsApp) | La consecuencia 1 se concreta en los puertos aprobados por canal | Texto original intacto, con la concreción añadida |

No se crearon ADR nuevos ni se cambió el estado de ninguno. No existe un ADR específico de zona horaria/cuotas: PD-05 se registró en el ERD y en `DECISIONS-v1.5.md`.

## 5. Cambios en el ERD

- **Estado:** de "modelo lógico propuesto" a **"modelo lógico aprobado para v1"**.
- **Nueva `platform_role_assignments`:** `id`, `user_id`, `role_code`, `granted_by`, `created_at`, `revoked_at`, `revoked_by`.
  - FK a `users`; CHECK `role_code IN ('super_admin')`.
  - UNIQUE parcial de asignación activa e índice por usuario activo.
  - Revocación lógica y auditoría de la concesión y la revocación.
- **Nueva `delivery_status_history`:** `id`, `delivery_id`, `previous_status`, `new_status`, `actor_type`, `actor_id`, `reason_code`, `changed_at`. Append-only, con índice (`delivery_id`,`changed_at`).
- **Eliminada:** `applications.timezone_override`. `scheduled_notifications.timezone` y `notifications.timezone_snapshot` se conservan para auditoría.
- **`roles`:** la definición de rol queda separada de la asignación de privilegio.
  - `organization_id = NULL` solo para los roles del sistema `organization_admin`, `operator` y `viewer`.
  - CHECK `code <> 'super_admin'`, de modo que es imposible asignar `super_admin` mediante `membership_roles`.
- **Estados:**
  - nueva sección con los dominios de valores de `notifications.status`, `notification_deliveries.status` y `notification_attempts.result_code`;
  - `notification_status_history` queda limitada al estado público.
- **Diagrama:** dos tablas nuevas, la entidad `INBOX_MESSAGES` y relaciones que ya existían en el diccionario pero faltaban en Mermaid. Se añadió una sección "Relaciones" con la clave foránea y la explicación de cada relación.
- **Reglas de integridad:**
  - nueva regla 5: herencia de tenant en tablas hijas;
  - regla 8 reescrita: zona horaria única de la organización;
  - nueva regla 10: `super_admin`;
  - nueva regla 11: historiales en la misma transacción.

## 6. Cambios en OpenAPI

Hecho con un script sobre el YAML. El *round-trip* previo fue idéntico byte a byte, así que no hubo reformateo ajeno. **No se modificó ninguna ruta.**

- **Componentes nuevos:**
  - `responses`: `BadRequest`, `Unauthorized`, `Forbidden`, `NotFound`, `Conflict`, `PayloadTooLarge`, `UnsupportedMediaType`, `UnprocessableContent`, `TooManyRequests` y `ServiceUnavailable`. Todos usan `application/problem+json`; `429` y `503` incluyen la cabecera `Retry-After`.
  - `headers.RetryAfter`.
  - `parameters.IdempotencyKey`.
  - Sustituyen al componente genérico `Problem`, que se eliminó.
- **`ProblemDetails`:** obligatorios `type`, `title`, `status`, `detail`, `instance`, `code` y `traceId`; `errors` y `retryAt` opcionales.
- **Respuestas añadidas según el comportamiento de cada operación:** `409` en 24 operaciones, `422` en 11, `429` en 12, `413` en 4, `415` en 4 y `503` en 8.
  - El `429` aplica a la Public API, por su rate limiting documentado, y a `startCampaign`, por la cuota.
  - Ninguna operación de consulta de la Management API ni de autenticación recibió `429`, porque no tienen rate limiting documentado.
  - 36 operaciones describen sus códigos de error específicos.
- **`Idempotency-Key`:** se añadió a `createCampaign` y `createScheduledNotification`. `api-design.md` ya lo exigía y el contrato no lo declaraba.
- **Estados:** nuevo esquema `NotificationPublicStatus` con 8 valores, sin `RECONCILIATION_REQUIRED`. Las descripciones aclaran que `DELIVERED`/`READ` dependen de la evidencia del proveedor.
- **Zona horaria:**
  - se eliminó `timezoneOverride` de `CreateApplicationRequest` y `UpdateApplicationRequest`;
  - `Application.timezone` pasa a ser de solo lectura y heredada;
  - se eliminó `timezone` de `CreateScheduledNotificationRequest`, porque la zona es siempre la de la organización;
  - `ScheduledNotification.timezone` queda de solo lectura, para auditoría.
- **Roles:** descripciones de los roles asignables en membresías y nuevo campo `CurrentUser.platformRoles` (`super_admin`).

## 7. Cambios en RBAC

Columna `org_admin` → `organization_admin`. Nueva sección "Definición de rol frente a asignación de privilegio". La regla de `super_admin` pasa a incluir el código `422 ROLE_NOT_ASSIGNABLE` y la auditoría. Las capacidades por rol no cambiaron.

## 8. Cambios en estados

| Nivel | Valores | Documentos |
|---|---|---|
| Público | `ACCEPTED`, `QUEUED`, `PROCESSING`, `SENT`, `DELIVERED`, `READ`, `FAILED`, `CANCELLED` | ADR-0012, ERD, OpenAPI, api-design, criteria |
| Entrega | `PENDING`, `QUEUED`, `IN_PROGRESS`, `PROVIDER_ACCEPTED`, `RETRY_PENDING`, `RECONCILIATION_REQUIRED`, `DELIVERED`, `READ`, `FAILED`, `DEAD_LETTER`, `CANCELLED` | ADR-0012, ERD, rabbitmq-outbox |
| Intento | `PROVIDER_ACCEPTED`, `TRANSIENT_FAILURE`, `PERMANENT_FAILURE`, `AMBIGUOUS` | ADR-0012, ERD, hexagonal (`ProviderSendResult`) |

En [provider-adapters.md](../providers/provider-adapters.md) se añadió una tabla de capacidades de estado por proveedor (Evolution, SMTP y SMSGate) que no promete `DELIVERED`/`READ` sin evidencia.

## 9. Cambios en configuración y despliegue

- `docker.md`, `compose-topology.md`, `environment-decisions.md`: PD-06 y PD-07 marcadas como aprobadas, con la tabla de imágenes por host y GHCR (`ghcr.io/<owner>/notification-<host>`).
- Las variables `NOTIFICATIONS_*_HOST` se movieron a "variables de despliegue": las lee el Compose, no la aplicación.
- `.env.example`: placeholders `NOTIFICATIONS_*_HOST`. No se inventaron dominios.
- Sin Docker Compose ni despliegues.

## 10. Búsqueda de términos obsoletos

Búsqueda en `docs/`, `README.md`, `AGENTS.md`, `CLAUDE.md` y `.env.example`, excluidos los dos registros históricos (`PENDING-DECISIONS.md` y el informe de saneamiento):

| Término | Resultado |
|---|---|
| `org_admin` | Solo en el historial de ADR-0011 y en frases que lo declaran retirado (criteria, DECISIONS, RBAC, AGENTS) |
| `application_admin` | Igual que `org_admin` |
| `INotificationProvider` | Solo como "retirado/sustituido" (hexagonal, ADR-0004, DECISIONS, provider-adapters, AGENTS) |
| `IProviderConnectionManager` | Solo como retirado en `hexagonal.md` |
| `timezone_override` / `timezoneOverride` | Solo en la nota de cambios del ERD y en DECISIONS ("sin..."); 0 apariciones en OpenAPI |
| `UNKNOWN` | Solo como término retirado (ADR-0012, provider-contracts) |
| `RECONCILIATION_REQUIRED` | Solo como estado de entrega; nunca como estado público |
| "422 si se adopta" | 0. La única coincidencia de "si se adopta" se refiere a RLS de PostgreSQL y no tiene relación |
| PD-01..PD-08 como pendientes | 0 en documentos vigentes. En `PENDING-DECISIONS.md`, las tres frases originales de "pendiente" quedaron marcadas como *texto histórico* |
| `SMTP_ACCEPTED` | Sustituido en `AGENTS.md` y provider-contracts por `PROVIDER_ACCEPTED`/`SENT` |

## 11. Validaciones ejecutadas (resultados reales)

| Validación | Herramienta | Resultado |
|---|---|---|
| YAML parse | PyYAML 6.0.3 | OK |
| OpenAPI 3.1 | `openapi-spec-validator` 0.9.0 (venv aislado en el scratchpad) | OK (el contrato original también era válido) |
| `$ref` locales | Script propio | 596 referencias, 93 distintas, 0 sin resolver |
| `operationId` únicos | Script propio | 93 operaciones, 0 duplicados |
| Respuestas de error | Script propio | Cada código 4xx/5xx apunta a su componente RFC 9457; 0 componentes sin uso; `Retry-After` en 429/503; `Idempotency-Key` en las 4 creaciones idempotentes |
| ERD | Script propio | 39 entidades en Mermaid = 39 tablas en el diccionario; 45 relaciones, todas explicadas; estados de los 3 niveles exactos; `timezone_override` ausente del modelo; `super_admin` solo en `platform_role_assignments` |
| Enlaces y anclas | Script propio (slugs estilo GitHub) | 45 archivos Markdown, 193 enlaces relativos; 0 rotos (ejecución final, con este informe ya creado) |
| Mermaid | Chequeo estructural propio | 7 bloques sin problemas. **El parser oficial no se ejecutó:** la descarga de `@mermaid-js/mermaid-cli` mediante `npx` fue denegada por el modo automático y no se intentó otra vía |
| Arquitectura | Revisión manual | Coherente con Clean/Hexagonal (puertos en Application, adaptadores en Infrastructure), multi-tenancy (tenant derivado del principal), RabbitMQ + Outbox/Inbox, Redis solo como caché, Traefik externo y PostgreSQL como fuente transaccional |
| `dotnet build -c Release` | .NET SDK 10.0.404 | Build succeeded, 0 advertencias, 0 errores (sin cambios de código) |
| `dotnet test -c Release` | xUnit | 63 superadas, 0 fallidas |

## 12. Pendientes técnicos conservados

Se conservan en [technical-pending-register.md](../operations/technical-pending-register.md):

- TP-01 a TP-12 y TP-14, con su estado original. TP-02 lleva ahora la recomendación de Testcontainers.
- TP-09 se actualizó: el catálogo ya está documentado, pero el middleware aún no emite `type`.

Se añadieron como **puntos abiertos que requieren aprobación**:

- **TP-15:** alta y gestión de `super_admin`. No hay operación para concederlo o revocarlo, ni procedimiento de alta inicial.
- **TP-16:** defectos del contrato no relacionados con estas decisiones:
  - rutas con doble barra (`/management//dashboard/*`, `/management//audit/events`);
  - `page`/`pageSize` en operaciones GET de un recurso individual;
  - 6 rutas del inventario ausentes en OpenAPI;
  - `PUT /management/users/{userId}/roles`, que contradice PD-01.
- **TP-17:** puerto para `testProviderAccount`/`getProviderHealth`, que no está en la lista aprobada en PD-03.
- **TP-18:** vínculo notificación–adjunto no modelado.
- **TP-19:** mecanismo del reenvío manual, su efecto en la cuota y el plazo de conciliación.
- **TP-20:** roles personalizados por organización. Se conservan porque PD-01 no los retira.

## 13. Riesgos identificados

- **Código desalineado:** `GlobalExceptionMiddleware` no emite `type`, que ahora es obligatorio en `ProblemDetails` (TP-09). Esta tarea prohibía modificar código.
- **Contrato con defectos previos:** las rutas con doble barra y las 6 rutas que faltan pueden confundir a los clientes generados desde OpenAPI (TP-16).
- **Bloqueantes de la Fase 2:**
  - TP-18 y TP-20 afectan a tablas;
  - TP-02 es necesario para validar persistencia contra PostgreSQL.
- **Mermaid sin parser oficial:** el chequeo fue estructural.
- **Proveedores sin validar en ejecución:** las capacidades `DELIVERED`/`READ` de Evolution y SMSGate siguen "por validar".

## 14. Confirmación

**No se inició la Fase 2.** No se crearon entidades, `DbSet`, Fluent API, repositorios, migraciones, seeds, autenticación, endpoints, integraciones, Docker Compose ni despliegues. No se modificó código ni se hicieron commits ni push.
