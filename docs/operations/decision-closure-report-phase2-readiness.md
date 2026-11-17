# Informe de preparación documental de la Fase 2 — 2026-11-15/16

> **Documento histórico** (evidencia de la tarea). No es fuente operativa: el estado vigente está en [technical-pending-register.md](technical-pending-register.md) y las decisiones en [DECISIONS-v1.5.md](../project/DECISIONS-v1.5.md).

**La Fase 2 no ha comenzado. No se ha modificado código.** La tarea fue exclusivamente documental.

## 1. Objetivo

Corregir en la documentación todo lo que impedía iniciar la Fase 2 (persistencia y modelo de datos): huecos del ERD, contradicciones entre el ERD y OpenAPI, y decisiones sin tomar. Se usó como fuente complementaria el sistema anterior `Notificaciones-master` (documentación y `database/migrations/001_audit_schema.sql`). No se abrieron los archivos con credenciales (`token.js`, `postfix-accounts.cf`, `.env.example`).

## 2. Diagnóstico inicial

Al cruzar el ERD con `openapi.yaml` y con la fuente anterior aparecieron estos bloqueos:

| # | Bloqueo | Por qué impedía migrar |
|---|---|---|
| 1 | Columnas de estado sin dominio (15 tablas) | No se podían escribir los CHECK |
| 2 | `createTemplate`, `createCampaign` y `createScheduledNotification` no indicaban la aplicación | `application_id` NOT NULL sin origen |
| 3 | `AcceptedBatch.batchId` sin tabla ni semántica | Lote no persistible |
| 4 | Cuota de envíos programados ambigua | Regla de reserva indefinida |
| 5 | Límite de organización sin almacenamiento | Solo existía `application_limits` |
| 6 | Alta de usuarios sin mecanismo de contraseña | `password_hash` sin origen |
| 7 | TP-40: el hash CSRF no tenía columna ni parámetros | `refresh_tokens` incompleta |
| 8 | `application_permissions` frente a los scopes de API Key | Dos fuentes sin regla |
| 9 | `provider_accounts.is_default` sin significado | Índice indefinido |
| 10 | Catálogo `permissions` sin códigos | No se podían cargar los datos de referencia |
| 11 | Rotación de API Key indefinida | Modelo de `api_keys` incompleto |
| 12 | Columnas del contrato ausentes en el ERD, y viceversa | `api_keys.name`, `provider_instances.name`, `template_versions.status`, eliminación lógica de plantillas y cuentas, `metadata`; `notifications.priority` sin contrato |
| 13 | Funciones del sistema anterior sin decisión de alcance | PGP/ZIP, supresión, consentimiento, metadatos de origen |

## 3. Decisiones aprobadas por el usuario

| ID | Decisión | Estado documental | Implementación |
|---|---|---|---|
| TP-40 | Token CSRF de 32 bytes guardado como SHA-256 en `refresh_tokens.csrf_token_hash`; comparación en tiempo constante; `Origin` obligatorio; mismo criterio para el refresh token | Cerrado | Pendiente |
| TP-43 | Lotes con aceptación parcial (`notification_batches`) | Cerrado | Pendiente |
| TP-44 | Cuota de envíos programados al ejecutarse | Cerrado | Pendiente |
| TP-45 | `organization_limits` + Platform API (`super_admin`) | Cerrado | Pendiente |
| TP-46 | Contraseña temporal y cambio obligatorio; membresía solo de usuarios existentes | Cerrado | Pendiente |
| TP-47 | Cifrado PGP / ZIP AES-256 en v1 | Cerrado | Pendiente |
| TP-48 | `application_permissions` como techo de scopes | Cerrado | Pendiente |
| TP-49 | Cuenta de proveedor predeterminada por organización y canal | Cerrado | Pendiente |
| TP-50 | Catálogo de estados | Cerrado | Pendiente |
| TP-51 | Catálogo de permisos de rol | Cerrado | Pendiente |
| TP-52 | Rotación de API Key con fila nueva y sin solape | Cerrado | Pendiente |
| TP-53 | Alineación ERD ↔ OpenAPI (derivada del contrato aprobado) | Cerrado | Pendiente |

**Fuera de v1, por decisión del usuario:**

- lista de supresión;
- registro de consentimiento (el opt-in es responsabilidad de la organización cliente);
- metadatos de origen normalizados (se usa `metadata`);
- prioridad.

## 4. Cambios documentales

| Archivo | Cambios |
|---|---|
| `docs/data/erd.md` | **Tablas nuevas:** `organization_limits` y `notification_batches` (42 entidades). **Columnas nuevas** en `users`, `refresh_tokens`, `api_keys`, `provider_accounts`, `provider_instances`, `notifications` (lote, plantilla, `metadata_json`, cifrado), `templates`, `template_versions` y `scheduled_notifications`. **Retirado:** `notifications.priority`. **Añadidos:** 5 relaciones en el diagrama y 4 en la tabla; dominios cerrados de estados; reglas 16 a 19 |
| `docs/api/openapi.yaml` | **Operaciones nuevas** (101 → 104): `changePassword`, `getOrganizationLimits` y `setOrganizationLimits`. **Esquemas nuevos:** `EncryptionOptions`, `BatchItemResult`, `OrganizationLimits`, `OrganizationChannelLimit`, `SetOrganizationLimitsRequest`, `ChangePasswordRequest` y `RolePermissionCode`. **Esquemas modificados:** enums de estado y canal en los esquemas; `applicationId` en plantillas, campañas y programaciones; `temporaryPassword` y `mustChangePassword`; `AcceptedBatch` con resultado por ítem; `isDefault` en las cuentas; `name`, `scopes` y `rotatedFromApiKeyId` en las API Keys. **Operaciones modificadas:** descripciones y respuestas de lote, programación, alta de usuario y membresía, API Keys, permisos de rol, login, refresh y logout |
| `docs/api/error-codes.md` | Nuevos `PASSWORD_CHANGE_REQUIRED` (403), `MEMBERSHIP_USER_NOT_FOUND`, `PASSWORD_CHANGE_REJECTED`, `PERMISSION_NOT_ASSIGNABLE`, `API_KEY_SCOPE_NOT_PERMITTED` y `ENCRYPTION_OPTIONS_INVALID` (422) |
| `docs/security/rbac-matrix.md` | Catálogo de permisos de rol con asignaciones iniciales; filas de límites de organización, alta de usuarios y cambio de contraseña; regla del techo de scopes |
| `docs/adr/0011-admin-identity.md` | Parámetros de TP-40; nueva sección de contraseña temporal (TP-46) |
| `docs/api/api-design.md` | Secciones sobre lotes, programación, cifrado, metadatos, límites y API Keys |
| `docs/api/endpoint-inventory.md` | 104 operaciones; notas |
| `docs/api/admin-panel-auth-contract.md` | `Origin` obligatorio; flujo de contraseña temporal |
| `docs/architecture/hexagonal.md` | Puerto `IContentEncryptionService` |
| `docs/operations/quotas-retention.md` | Modelo de cuotas de v1 |
| `docs/acceptance/criteria.md` | Cuota de programadas y lotes, límites, contraseña temporal, cifrado y consentimiento fuera de v1 |
| `docs/operations/super-admin-bootstrap.md` | Credencial inicial = contraseña temporal |
| `docs/providers/legacy-source-assessment.md` | Tabla de funciones del sistema anterior frente a v1 |
| `docs/testing/scheduler-tests.md` | Prueba de cuota al ejecutar |
| `docs/project/DECISIONS-v1.5.md`, `AGENTS.md`, `docs/operations/technical-pending-register.md`, `docs/project/notification-architecture-index.md` | Decisiones, reglas de agente, registro (TP-40 y TP-43 a TP-53 aprobados; TP-54 a TP-56 nuevos) e índice |

## 5. Validaciones ejecutadas

Herramientas: PyYAML 6.0.3 y `openapi-spec-validator` 0.9.0 en un entorno virtual del scratchpad, más scripts propios.

| Validación | Resultado |
|---|---|
| Batería de contrato (YAML, OpenAPI 3.1, `operationId`, `$ref`, paginación, scopes, inventario, seguridad) | **178 OK, 0 fallos**: 104 operaciones, 81 rutas, 0 duplicados, 668 `$ref` (105 distintos) sin ninguno roto, 0 esquemas sin uso, inventario = OpenAPI |
| Validación cruzada ERD ↔ OpenAPI (preparación de la Fase 2) | **66 OK, 0 fallos**: 42 entidades en el diagrama = 42 en el diccionario; columnas nuevas presentes; `priority` retirado; los 14 enums de OpenAPI coinciden con los dominios del ERD; toda columna de estado tiene dominio; `RolePermissionCode` = catálogo RBAC (14 códigos); códigos de error nuevos en el catálogo y en OpenAPI; registro coherente; sin contradicciones vigentes |
| Mermaid (diagrama del ERD, **modificado**) | Comprobación **estructural**: 54 relaciones con sintaxis válida y bloque cerrado. Las 5 relaciones nuevas usan cardinalidades ya presentes y validadas por el parser oficial en TP-23, incluida la autorrelación. **El parser oficial no se ejecutó** (ver §7) |
| Enlaces relativos y anclas | 56 archivos Markdown, 495 enlaces y anclas, **0 rotos** |
| Código | `git status`: sin cambios fuera de `docs/`, `AGENTS.md` y `README.md` |

Durante la validación, el comprobador dio tres falsos positivos, que se corrigieron en el script y no en la documentación:

- un permiso de rol tomado por scope;
- valores de enum tomados por códigos de error;
- un patrón que no admitía dígitos (`ZIP_AES256`).

## 6. Pendientes que no bloquean la Fase 2

- **TP-41:** comportamiento del panel al recargar la página.
- **TP-42:** distinción `401`/`403` en refresh/logout.
- **TP-54:** purga del secreto ZIP y reenvío de notificaciones cifradas. La columna ya está modelada.
- **TP-55:** política de contraseñas y restablecimiento.
- **TP-56:** selección del gateway SMS. Se añadiría con una migración expand.

La propia Fase 2 incluye TP-02 (Testcontainers), TP-03 (`dotnet-ef`) y la fijación del patch y digest de PostgreSQL 17 (TP-11). Implementar TP-47 exige autorizar paquetes NuGet de OpenPGP y ZIP AES.

## 7. Riesgos y observaciones

- **Mermaid sin el parser oficial:** la descarga de `@mermaid-js/mermaid-cli` con `npx` fue denegada por la política de permisos del entorno. Para revalidar: `npx @mermaid-js/mermaid-cli@11.4.2 -i erd.mmd -o erd.svg` con el bloque del ERD.
- **`MEMBERSHIP_USER_NOT_FOUND`** permite a quien tenga `members.manage` comprobar si un email existe en la plataforma. Se acepta como consecuencia de la decisión TP-46.
- **Reglas derivadas, no preguntadas expresamente:**
  - índices `UNIQUE (refresh_tokens.token_hash)` y `UNIQUE (api_keys.key_prefix)`;
  - `changePassword` revoca las demás sesiones;
  - el límite efectivo es el menor entre aplicación y organización, sin rechazar al bajar el de la organización;
  - el lote responde `202` aunque se rechacen todos los ítems;
  - SMS solo admite cifrado `NONE`, porque el sistema anterior no cifraba SMS;
  - el error `422 PERMISSION_NOT_ASSIGNABLE`.

  Conviene revisarlas antes de implementar.
- `decision-closure-report-tp28-tp37.md` y `decision-closure-report-tp38-tp39.md` son históricos y no se modificaron.

**Ejecución final** (con este informe ya creado):

- enlaces relativos y anclas: 56 archivos Markdown y 495 enlaces, **0 rotos**;
- batería de contrato y validación cruzada: 0 fallos;
- `git status`: sin cambios fuera de `docs/`, `AGENTS.md` y `README.md`.
