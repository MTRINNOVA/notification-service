# Informe de cierre documental TP-41, TP-42 y TP-54 a TP-56 — 2026-11-16

> **Documento histórico** (evidencia de la tarea). No es fuente operativa: el estado vigente está en [technical-pending-register.md](../operations/technical-pending-register.md) y las decisiones en [DECISIONS-v1.5.md](../project/DECISIONS-v1.5.md).

**La Fase 2 no ha comenzado. No se ha modificado código.**

## 1. Decisiones aprobadas por el usuario

| ID | Decisión | Estado documental | Implementación |
|---|---|---|---|
| TP-41 | `POST /management/auth/session` (`resumeSession`): recupera la sesión tras recargar el panel con la cookie `refresh` y un `Origin` obligatorio, sin `X-CSRF-Token`. El panel serializa la rotación entre pestañas (Web Locks) y comparte los tokens en memoria (BroadcastChannel) | Cerrado | Pendiente |
| TP-42 | Se mantiene la distinción `401` (sesión) / `403` (CSRF u `Origin`); riesgo aceptado | Cerrado | Sin trabajo adicional |
| TP-54 | El secreto ZIP se purga cuando la entrega pasa a `PROVIDER_ACCEPTED`, `FAILED`, `DEAD_LETTER` o `CANCELLED`; un reenvío de `ZIP_AES256` responde `409 NOTIFICATION_NOT_RETRYABLE` | Cerrado | Pendiente |
| TP-55 | Política NIST SP 800-63B: entre 12 y 128 caracteres; se rechazan las contraseñas comunes o filtradas, las iguales al email y la actual. Para recuperar el acceso, un administrador reasigna una contraseña temporal (`resetUserTemporaryPassword`); no hay autoservicio | Cerrado | Pendiente |
| TP-56 | Un gateway SMS por cuenta (UNIQUE `sms_gateways.provider_account_id`) y `notification_deliveries.sms_gateway_id` | Cerrado | Pendiente |

## 2. Reglas derivadas, que conviene revisar

- **TP-41:** si dos pestañas recuperan la sesión a la vez, la segunda presentaría un refresh token ya rotado y se revocaría la familia. Por eso el contrato del panel exige serializar `resumeSession`, `refreshSession` y `logout` entre pestañas.
- **TP-55:** quien tiene `members.manage` solo puede reasignar la contraseña de un usuario que pertenezca exclusivamente a organizaciones donde el actor tiene ese permiso y que no sea `super_admin`. Como la cuenta es global, de lo contrario podría escalar su acceso a otra organización.
- **TP-55:** la lista de contraseñas comunes y filtradas es local. Consultar un servicio externo requiere aprobación previa.
- **TP-54:** mientras la entrega está en `RECONCILIATION_REQUIRED`, el secreto se conserva; se purga cuando la conciliación la resuelve.
- **Nuevo código de error:** `422 PASSWORD_POLICY_VIOLATION`, en `createUser` y `resetUserTemporaryPassword`. `changePassword` sigue usando el genérico `PASSWORD_CHANGE_REJECTED`.

## 3. Cambios documentales

| Archivo | Cambio |
|---|---|
| `docs/api/openapi.yaml` | Operaciones nuevas `resumeSession` y `resetUserTemporaryPassword` (104 → 106) y esquema `ResetTemporaryPasswordRequest`. Longitudes 12..128 en las contraseñas. `422` en `createUser`. Reenvío ZIP en `retryNotification`. `RefreshCookie` y la lista de operaciones admitidas con `mustChangePassword` actualizadas |
| `docs/data/erd.md` | Regla de purga del secreto ZIP; UNIQUE en `sms_gateways`; `notification_deliveries.sms_gateway_id`; diagrama: cuenta → gateway 1 a 0..1 y gateway → entregas |
| `docs/adr/0011-admin-identity.md` | TP-41, TP-42 y TP-55 (política y recuperación) |
| `docs/adr/0012-delivery-semantics.md` | Reenvío de notificaciones cifradas (TP-54) |
| `docs/api/error-codes.md` | `PASSWORD_POLICY_VIOLATION`; precisiones en `NOTIFICATION_NOT_RETRYABLE`, `PASSWORD_CHANGE_REQUIRED` y `AUTHENTICATION_REQUIRED` |
| `docs/api/admin-panel-auth-contract.md` | Secciones «Recarga de página y varias pestañas» y «Contraseña olvidada». Se retiró «Puntos abiertos» |
| `docs/api/api-design.md`, `docs/api/endpoint-inventory.md`, `docs/security/rbac-matrix.md`, `docs/acceptance/criteria.md` | Reglas, operaciones y criterios nuevos |
| `docs/project/DECISIONS-v1.5.md`, `AGENTS.md`, `docs/operations/technical-pending-register.md`, `docs/project/notification-architecture-index.md` | Decisiones, reglas de agente, registro (sin puntos en `Requiere aprobación`) e índice |

## 4. Validaciones ejecutadas

| Validación | Resultado |
|---|---|
| Batería de contrato (YAML, OpenAPI 3.1, `operationId`, `$ref`, inventario, scopes, seguridad) | **181 OK, 0 fallos**: 106 operaciones, 83 rutas; `RefreshCookie` solo en refresh, logout y `resumeSession`; `CsrfHeader` solo en refresh y logout |
| Cruce ERD ↔ OpenAPI | **66 OK, 0 fallos**: 42 entidades = 42 tablas; enums alineados; registro coherente |
| Comprobaciones específicas de TP-41, TP-42 y TP-54 a TP-56 | **16 OK, 0 fallos**, entre ellas que el registro no tenga ningún punto en `Requiere aprobación` |
| Mermaid (ERD, modificado) | Estructural: 55 relaciones válidas. La cardinalidad nueva `\|\|--o\|` ya se usaba en el diagrama. **El parser oficial no se ejecutó**, porque la descarga con `npx` está bloqueada en el entorno |
| Enlaces relativos y anclas | 57 archivos Markdown, 510 enlaces y anclas, **0 rotos** (con este informe ya creado) |
| Código | `git status`: sin cambios fuera de `docs/`, `AGENTS.md` y `README.md` |

## 5. Estado resultante

**No queda ninguna decisión de diseño pendiente de aprobación.** Los puntos abiertos del registro son:

- validaciones de proveedores (TP-24 a TP-26);
- infraestructura y operación de producción (TP-01, TP-08, TP-11, TP-12, TP-27, TP-29 a TP-31);
- pruebas previas a producción (TP-32);
- tareas de código (TP-10, TP-14).

Ninguno impide la Fase 2. La Fase 2 sigue requiriendo la autorización expresa del usuario (AGENTS.md §18.10).

## Anexo — TP-57 (2026-11-17)

**Decisión aprobada por el usuario:** la sesión del panel dura como máximo **60 minutos desde el login**, aunque el usuario esté activo, y después se pide un nuevo login. Sustituye a la propuesta inicial de 7 días de ADR-0011.

**Diseño:**

- Se mantienen el access token de 15 minutos y la rotación del refresh token, necesarios para CSRF y para la detección de reutilización.
- Todos los refresh tokens de una familia comparten `expires_at` = login + 60 minutos, de modo que rotar no alarga la sesión.
- Al vencer, refresh y `resumeSession` responden `401` y el panel muestra el login.
- No se añadieron columnas ni operaciones.

**Documentos:** ADR-0011, `openapi.yaml` (`RefreshCookie`, `refreshSession`, `resumeSession`), ERD (`refresh_tokens`), contrato del panel, `security-model.md`, criterios, DECISIONS, registro y AGENTS.md.

**Validación:**

- Batería de contrato: 0 fallos.
- Cruce ERD ↔ OpenAPI: 0 fallos.
- Enlaces: 512, 0 rotos.
- La única mención restante a "7 días" es la nota histórica de ADR-0011 que indica qué se sustituyó.

## Anexo — TP-58 (2026-11-17)

**Decisión aprobada por el usuario:** el panel avisa **5 minutos** antes del cierre de la sesión, con una cuenta atrás, y desde el mismo aviso el usuario puede **reautenticarse con su contraseña** sin perder la página.

**Diseño:**

- `LoginResponse` incluye `sessionExpiresAt` (UTC) y `sessionExpiresIn` (segundos), que devuelven login, refresh y `resumeSession`.
- La reautenticación es un login normal: no hay operaciones nuevas.
- **Regla derivada, que conviene revisar:** el login revoca la sesión de la cookie `refresh` que trae la petición, para que no queden dos sesiones vivas.
- La sesión nunca se alarga sin contraseña, conforme al criterio 2.2.1 de WCAG 2.2.

**Documentos:** `openapi.yaml`, ADR-0011, contrato del panel, inventario, criterios, DECISIONS, registro y AGENTS.md.
