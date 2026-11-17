# Informe de cierre de decisiones TP-28, TP-33 a TP-37 y TP-05 a TP-07 — 2026-11-15

> **Documento histórico** (evidencia de la tarea del 2026-11-15). No es fuente operativa: el estado vigente está en [technical-pending-register.md](technical-pending-register.md) y las decisiones en [DECISIONS-v1.5.md](../project/DECISIONS-v1.5.md).

## 1. Resumen

Se incorporaron a la documentación normativa las nueve decisiones aprobadas por el usuario: TP-05, TP-06, TP-07, TP-28, TP-33, TP-34, TP-35, TP-36 y TP-37. **La tarea fue exclusivamente documental.** Su estado es **aprobado y especificado documentalmente**; ninguna está implementada ni verificada en código.

- No se modificaron archivos `.cs`, `.csproj`, `.props`, `.sln`, Dockerfiles, `appsettings*.json` ni `.env.example`.
- No se crearon workflows ni el proyecto `Scheduler.Tests`, y no se instalaron paquetes en el proyecto.
- No se hicieron commits ni push.
- **La Fase 2 no comenzó.**

Durante el trabajo aparecieron dos puntos nuevos que requieren aprobación: **TP-38** (mecanismo del token CSRF) y **TP-39** (cancelación repetida en la Public API).

## 2. Punto de partida

- Rama `main`, commit `f7b1f3a` ("Feat: Documentos").
- Árbol de trabajo limpio, sin cambios locales del usuario que proteger.

## 3. Decisiones aplicadas

| ID | Decisión | Estado final |
|---|---|---|
| TP-05 | `/health/live` y `/health/ready` internos en Worker y Scheduler | Aprobado — especificado documentalmente |
| TP-06 | `Scheduler.Tests` con la primera lógica propia del Scheduler; nunca vacío | Aprobado — especificado documentalmente |
| TP-07 | GitHub Actions: CI de PR y publicación controlada en GHCR; sin despliegue automático | Aprobado — especificado documentalmente |
| TP-28 | Scope independiente `attachments.read` para `getAttachment` | Aprobado — especificado documentalmente |
| TP-33 | Remitente predeterminado solo por aplicación | Aprobado — especificado documentalmente |
| TP-34 | Metadatos de paginación en 9 esquemas `*Page` | Aprobado — especificado documentalmente |
| TP-35 | Protección del último `organization_admin` | Aprobado — especificado documentalmente |
| TP-36 | Cancelación desde la Management API | Aprobado — especificado documentalmente |
| TP-37 | Cookie de refresh y CSRF declarados en OpenAPI | Aprobado — especificado documentalmente |

## 4. Archivos

**Creados (4):**

- `docs/deployment/health-checks.md`: especificación de TP-05.
- `docs/deployment/ci-cd.md`: especificación de TP-07.
- `docs/testing/scheduler-tests.md`: especificación de TP-06 y pruebas previstas.
- `docs/operations/decision-closure-report-tp28-tp37.md`: este informe.

**Modificados (17):**

| Archivo | Motivo |
|---|---|
| `docs/api/openapi.yaml` | TP-28, TP-33, TP-34, TP-35, TP-36 y TP-37 (detalle en §5) |
| `docs/api/endpoint-inventory.md` | 101 operaciones; scope de `getAttachment`; nueva operación de cancelación; autenticación de refresh/logout |
| `docs/api/api-design.md` | Paginación, remitente predeterminado, cancelación, adjuntos, último administrador y cookie |
| `docs/api/error-codes.md` | Nuevo `LAST_ORGANIZATION_ADMIN_REQUIRED`; precisiones en `AUTHENTICATION_REQUIRED`, `PERMISSION_DENIED`, `NOTIFICATION_ALREADY_DISPATCHED`, `RESOURCE_STATE_CONFLICT` y `SENDER_NOT_ASSIGNED_TO_APPLICATION` |
| `docs/security/rbac-matrix.md` | Catálogo de scopes de API Key; filas de cancelación y adjuntos; reglas TP-35 y TP-36 |
| `docs/security/security-model.md` | Cookie `refresh` + `X-CSRF-Token`; scope `attachments.read` |
| `docs/adr/0011-admin-identity.md` | Secciones TP-35 y TP-37; estado ampliado (sigue "Aceptado") |
| `docs/data/erd.md` | Retirado `whatsapp_senders.is_default`; regla de `application_senders`; regla de integridad 15 (TP-35) |
| `docs/project/DECISIONS-v1.5.md` | Precisión en §3 (TP-33) y tabla de las nueve decisiones |
| `docs/project/notification-architecture-index.md` | Nuevos documentos y este informe |
| `docs/operations/technical-pending-register.md` | Nueva sección de decisiones especificadas; TP-38 y TP-39; nota en TP-04 |
| `docs/deployment/docker.md`, `compose-topology.md` | Health checks de Worker/Scheduler y enlace a CI/CD |
| `docs/architecture/project-structure.md` | Estructura objetivo de `Scheduler.Tests` y de `.github/workflows/` |
| `docs/testing/integration-testing-postgresql.md` | Enlace de CI a TP-07 |
| `README.md` | Estado de TP-05 y TP-07 |
| `AGENTS.md` | §5, §13, §14, §17 y §19: las nueve decisiones pasan de "puntos abiertos" a "aprobadas, implementación pendiente" |

**No modificados deliberadamente:** todo `src/` y `tests/`; `CLAUDE.md`, que remite a §19 de `AGENTS.md`; los ADR distintos de ADR-0011; ADR-0012, cuyos estados se respetan; los informes históricos anteriores, `PENDING-DECISIONS.md` y `CHANGELOG-v1.5.md`; y los diagramas Mermaid.

## 5. Cambios por TP

### TP-28 — Scope de lectura de adjuntos

- `getAttachment` declara `x-required-scopes: [attachments.read]`. Su descripción limita la lectura a la organización y la aplicación propietarias (lo ajeno responde `404`). `downloadUrl` es una URL firmada temporal, sin credenciales ni datos internos de MinIO.
- Las 11 operaciones de la Public API declaran ahora su scope en la extensión `x-required-scopes`, que antes solo estaba en el inventario. Esto no cambia su comportamiento.
- Nuevo esquema `ApiKeyScope`, un catálogo cerrado de 7 scopes; `CreateApiKeyRequest.scopes` lo referencia. Antes no existía ningún catálogo de scopes.
- `rbac-matrix.md` incluye la tabla de scopes por operación.

### TP-33 — Remitente predeterminado

- ERD: se retiró `is_default` de `whatsapp_senders`, y `application_senders.is_default` queda como única fuente. Como máximo hay un predeterminado válido por aplicación y canal, con índice único parcial o validación transaccional y bloqueo de las filas de la aplicación. Debe ser de la misma organización y estar entre los asignados.
- OpenAPI:
  - se retiró `Sender.isDefault`;
  - se documentó `defaultSenderId`;
  - `assignApplicationSenders` describe la transacción y declara `422 SENDER_NOT_ASSIGNED_TO_APPLICATION` cuando el predeterminado no está en `senderIds`.
- No se introdujo un predeterminado global por organización. `provider_accounts.is_default` (cuenta de proveedor) no cambia.

### TP-34 — Paginación

- Los 9 esquemas requieren `items`, `page` (≥ 1), `pageSize` (1..100) y `total` (≥ 0). Cada `items` conserva su tipo, y la descripción de `total` indica que se cuenta antes de paginar.
- Las 10 operaciones que los devuelven ya tenían `page = 1` y `pageSize = 25` (1..100). `AuditEventPage` lo devuelven 2 operaciones. No hubo que añadir parámetros.
- Ninguna operación sin colección tiene parámetros de paginación.
- Las demás `*Page`, que ya tenían los tres campos, no se tocaron. Ver la inconsistencia I-3.

### TP-35 — Último `organization_admin`

- ADR-0011 contiene el invariante, las operaciones afectadas, la validación transaccional con `SELECT … FOR UPDATE`, el rechazo con `422 LAST_ORGANIZATION_ADMIN_REQUIRED`, la auditoría del intento y del resultado, y la recuperación excepcional por `super_admin`. Deja explícito que es independiente de TP-15.
- OpenAPI: `setMembershipRoles`, `updateMembershipStatus` y `updateUserStatus` describen la regla. Las dos últimas declaran ahora `422`.
- **Interpretación aplicada:** "administrador activo" significa membresía activa, de un usuario activo, con el rol. Por eso la regla cubre también `updateUserStatus`, porque suspender al usuario inhabilita todas sus membresías.

### TP-36 — Cancelación desde la Management API

- Nueva operación `POST /management/notifications/{notificationId}/cancel` (`cancelManagementNotification`), con la nueva etiqueta `Management Notifications`.
  - Seguridad `AdminBearer`; respuestas 200, 400, 401, 403, 404 y 409.
  - Mismo caso de uso que `cancelNotification`.
- Matriz de acceso:
  - `super_admin`: global, con auditoría;
  - `organization_admin`: dentro de su organización;
  - `operator`: según el permiso de rol `notifications.cancel`;
  - `viewer`: no;
  - API Key: sin cambios.
- Estados cancelables: entrega `PENDING`, `QUEUED` o `RETRY_PENDING`, según ADR-0012. Desde `IN_PROGRESS` → `409 NOTIFICATION_ALREADY_DISPATCHED`; ya cancelada → `409 RESOURCE_STATE_CONFLICT`; ajena o inexistente → `404`; sin permiso → `403`.
- La Public API no cambió: se verificó por diferencia estructural (§6).

### TP-37 — Cookie de refresh y CSRF

- Esquemas `RefreshCookie` (`apiKey`, `cookie`, `refresh`) y `CsrfHeader` (`apiKey`, `header`, `X-CSRF-Token`). Se aplican juntos, en un mismo requisito (AND), solo en `refreshSession` y `logout`. La seguridad global sigue siendo `security: []`.
- Propiedades documentadas en ADR-0011 y en los esquemas:
  - `HttpOnly`; `Secure` en producción;
  - `SameSite=Strict`;
  - `Path=/api/v1/management/auth`;
  - `Max-Age` de 7 días; eliminación con `Max-Age=0`;
  - CSRF inválido → `403`;
  - rotación de un solo uso; reutilización → revocación de la familia y `401`.
- **Nombre del header:** la documentación vigente no definía ningún nombre, solo "protección CSRF". Por eso no hay discrepancia y se adoptó `X-CSRF-Token`, el nombre de la decisión aprobada. El mecanismo de emisión del valor no estaba definido y queda en TP-38.

### TP-05, TP-06 y TP-07

- **TP-05:** `health-checks.md` define la semántica de liveness y readiness para el Worker (consumidor inicializado) y el Scheduler (planificador inicializado), la exposición interna sin Traefik y la consulta desde Docker/Portainer. También lista las implicaciones de implementación, sin aplicarlas: Kestrel/`Microsoft.AspNetCore.App` en los `.csproj`, la imagen `runtime` sin `curl` y el healthcheck en Compose.
- **TP-06:** `scheduler-tests.md` fija cuándo se crea el proyecto, qué pruebas lleva (vencimiento, zona horaria, duplicados, concurrencia, reintentos, recuperación y apagado) y qué no duplica de Domain y Application.
- **TP-07:** `ci-cd.md` separa la CI de PR (sin publicación, permisos de lectura) de la publicación controlada en GHCR (tres imágenes, tags inmutables, digest registrado, environment protegido). El despliegue automático al VPS queda fuera de la aprobación.

## 6. Validaciones ejecutadas (resultados reales)

Herramientas en un entorno virtual del scratchpad (PyYAML 6.0.3, `openapi-spec-validator` 0.9.0), fuera del repositorio, más scripts propios.

| # | Validación | Resultado |
|---|---|---|
| 1 | Sintaxis YAML | OK |
| 2 | OpenAPI 3.1 (`openapi-spec-validator`) | Válido. Control negativo: un esquema de seguridad `in: body` se detectó como `OpenAPIValidationError` |
| 3 | `operationId` | 101 operaciones (antes 100), 79 rutas, **0 duplicados** |
| 4 | `$ref` | 645 referencias, 98 distintas, **0 sin resolver**; 0 esquemas sin uso |
| 5 | Scopes Public API ↔ catálogo ↔ inventario ↔ RBAC | Las 11 operaciones públicas tienen `ApiKeyAuth` y un scope del catálogo. Los 7 scopes usados son exactamente el catálogo `ApiKeyScope`, y el inventario y la matriz RBAC los citan igual |
| 6 | Cancelación de Management | Presente en OpenAPI e inventario. `cancelNotification` conserva `ApiKeyAuth`. Inventario = OpenAPI (0 faltantes, 0 sobrantes; recuento 101) |
| 7 | `whatsapp_senders.is_default` / `Sender.isDefault` | El esquema `Sender` no tiene `isDefault`. Las únicas menciones están en el historial ("retirado") y en el registro de trazabilidad |
| 8 | Paginación TP-34 | Los 9 esquemas tienen los 4 campos requeridos y acotados. Las 10 operaciones tienen `page`/`pageSize` con valores por defecto 1/25 |
| 9 | Refresh/logout | Ambos con `RefreshCookie` AND `CsrfHeader`; ninguna otra operación usa la cookie; sin seguridad global |
| 10 | Mermaid | **Sin el parser oficial** (ver §7). Comprobado que los 7 bloques son idénticos a `HEAD`, donde se validaron con `@mermaid-js/mermaid-cli` 11.4.2 (TP-23). ERD: 40 entidades en el diagrama = 40 tablas en el diccionario |
| 11 | Enlaces relativos y anclas | 53 archivos Markdown, 425 enlaces y anclas, **0 rotos** (ejecución final, con este informe ya creado) |
| 12 | Contradicciones | Revisión con `grep` y lectura entre ADR, ERD, RBAC, OpenAPI, inventario, índice y `AGENTS.md`: sin contradicciones vigentes. Los códigos de error citados en OpenAPI están todos en el catálogo |
| 13 | PostgreSQL 17 | Sigue siendo la versión mayor aprobada. Las únicas menciones a PostgreSQL 16 son evidencia del ZIP legado |
| — | Public API sin cambios de comportamiento | Diferencia estructural contra el original: solo cambian `x-required-scopes` y la descripción de `getAttachment` |
| — | `dotnet build -c Release` | 0 advertencias, 0 errores |
| — | `dotnet test -c Release` | **63 superadas, 0 fallidas** (Domain 1, Application 2, Infrastructure 15, Api 14, Worker 5, Architecture 26) |
| — | `git status` | Cambios solo en `docs/`, `README.md` y `AGENTS.md` |

## 7. Validaciones que no pudieron ejecutarse

- **Render Mermaid con el parser oficial:** `npx --yes @mermaid-js/mermaid-cli@11.4.2` fue denegado por la política de permisos del entorno, y no se reintentó. Como ningún bloque Mermaid cambió, la validación oficial de TP-23 sigue aplicando. Para revalidar: `npx @mermaid-js/mermaid-cli@11.4.2 -i <bloque>.mmd -o out.svg` sobre los 7 bloques.
- **Formateo de tablas con Prettier:** no se ejecutó, por la misma restricción de descarga. Las filas editadas de `error-codes.md` no tienen el relleno de columnas de las demás. Se renderizan correctamente, pero el Markdown en crudo queda desalineado.
- **Docker build, CI y Testcontainers:** fuera del alcance documental (TP-01, TP-07, TP-02).

## 8. Inconsistencias detectadas

| # | Inconsistencia | Tratamiento |
|---|---|---|
| I-1 | No había nombre contractual de la cabecera CSRF ni mecanismo de emisión del valor | Se adoptó `X-CSRF-Token`, el nombre de la decisión. El mecanismo queda como **TP-38** (requiere aprobación) |
| I-2 | Al compartir el caso de uso, la cancelación de una notificación ya `CANCELLED` devuelve `409 RESOURCE_STATE_CONFLICT` en Management, pero la Public API no lo declara | No se modificó la Public API. Registrado como **TP-39** (requiere aprobación) |
| I-3 | Los demás esquemas `*Page` (`NotificationPage`, `EventPage`, `OrganizationPage`, etc.) tienen los tres campos, pero sin `required` ni límites | Fuera del alcance de los nueve. Se recomienda uniformarlos en una revisión de contrato |
| I-4 | No existe catálogo de códigos de **permisos de rol** (`permissions.code`); TP-36 introduce `notifications.cancel` | Documentado como permiso de rol, distinto del scope del mismo nombre. El catálogo de permisos se definirá al implementar RBAC |
| I-5 | La Management API no tiene operación para listar notificaciones, aunque la matriz concede "Consultar notificaciones" a roles humanos | Preexistente y fuera del alcance. Se señala para una revisión futura |
| I-6 | `SameSite=Strict` exige que panel y API compartan sitio registrable, y los DNS siguen sin fijar (TP-27) | Documentado: cambiar la política requiere aprobación y mantener CSRF |

## 9. Trabajo de implementación pendiente

- **TP-05:** servidor HTTP y checks en Worker/Scheduler; imagen y sonda de `HEALTHCHECK`; Compose (TP-08).
- **TP-06:** crear `Scheduler.Tests` con la primera lógica propia y trasladar `SchedulerHostCompositionTests`.
- **TP-07:** workflows de CI y de publicación, environment protegido, owner de GHCR y herramienta de validación de OpenAPI.
- **TP-28:** autorización por scope y URL firmada.
- **TP-33:** índice único parcial y caso de uso de asignación (Fase 2).
- **TP-34:** respuestas paginadas y pruebas de contrato (TP-14).
- **TP-35:** bloqueo transaccional en los casos de uso de membresías y estado de usuario; auditoría.
- **TP-36:** endpoint y caso de uso compartido; permiso de rol `notifications.cancel`.
- **TP-37:** autenticación del panel con cookie y CSRF, que depende de TP-38.
- **Requieren aprobación:** TP-38 y TP-39.

## 10. Confirmación

**La Fase 2 no comenzó.** No se crearon entidades, `DbContext`, repositorios, migraciones, endpoints, handlers, health checks, workflows ni proyectos. No se instalaron paquetes en el proyecto, no se modificaron Dockerfiles, Compose ni configuración de despliegue, y no se hicieron commits ni push.
