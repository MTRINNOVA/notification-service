# Informe de cierre de decisiones técnicas TP-02, TP-09 y TP-15 a TP-20 — 2026-11-15

> **Documento histórico** (evidencia de la auditoría del 2026-11-15). No es fuente operativa: el estado vigente de los pendientes está en [technical-pending-register.md](../operations/technical-pending-register.md) y las decisiones en [DECISIONS-v1.5.md](../project/DECISIONS-v1.5.md).

## 1. Resumen ejecutivo

Se incorporaron a la documentación las decisiones aprobadas por el usuario TP-02, TP-15, TP-16, TP-17, TP-18, TP-19 y TP-20, y se registró TP-09 como corrección de código independiente. **La tarea fue exclusivamente documental:** no se modificaron archivos `.cs`, `.csproj`, `.props` ni configuración ejecutable, y no se inició la Fase 2.

| Validación                             | Resultado                                                   |
| -------------------------------------- | ----------------------------------------------------------- |
| OpenAPI 3.1 (`openapi-spec-validator`) | Válido                                                      |
| ERD                                    | Coherente (40 entidades en el diagrama y en el diccionario) |
| Mermaid                                | Los 7 diagramas se renderizan con el **parser oficial**     |
| Enlaces relativos                      | Sin errores                                                 |

Durante la auditoría aparecieron **dos puntos nuevos que requieren aprobación**: TP-21 (actor del reenvío manual) y TP-22 (divergencias de contrato fuera del alcance de TP-16).

## 2. Commit inicial inspeccionado

`8c952ac` — "Feat: Se suben correcciones en los documentos docs."

## 3. Rama de trabajo

`main`. No se crearon ramas, commits ni push.

## 4. Estado inicial del repositorio

Árbol de trabajo limpio (sin cambios sin confirmar ni archivos sin seguimiento), así que no había modificaciones locales del usuario que proteger. No existía `docs/testing/`.

## 5. Decisiones incorporadas

| ID    | Decisión                                                                                                                 | Estado final                                         |
| ----- | ------------------------------------------------------------------------------------------------------------------------ | ---------------------------------------------------- |
| TP-02 | Testcontainers + PostgreSQL real para pruebas de persistencia                                                            | Aprobado — planificado (Fase 2)                      |
| TP-09 | `GlobalExceptionMiddleware` debe emitir `type: about:blank`                                                              | Corrección de código pendiente (tarea independiente) |
| TP-15 | Bootstrap `SYSTEM_BOOTSTRAP` + Platform API para `super_admin`; recuperación de emergencia; protección del último activo | Aprobado — implementación pendiente                  |
| TP-16 | Contrato OpenAPI normalizado                                                                                             | Corregido documentalmente                            |
| TP-17 | `IProviderAccountDiagnostics` (opcional, separado del envío)                                                             | Aprobado — implementación pendiente                  |
| TP-18 | `notification_attachments` (muchos a muchos)                                                                             | Modelo lógico validado — implementación pendiente    |
| TP-19 | Reintento técnico ≠ reenvío manual ≠ conciliación; conciliación configurable por proveedor                               | Semántica documentada — implementación pendiente     |
| TP-20 | Sin roles personalizados en v1                                                                                           | Corregido documentalmente                            |

## 6. Archivos creados

- `docs/operations/super-admin-bootstrap.md`: procedimiento de bootstrap, operación ordinaria y recuperación de emergencia (TP-15).
- `docs/testing/integration-testing-postgresql.md`: estrategia de Testcontainers (TP-02).
- `docs/operations/decision-closure-report-tp15-tp20.md`: este informe.

## 7. Archivos modificados (20)

| Área                     | Archivos                                                                                                                         |
| ------------------------ | -------------------------------------------------------------------------------------------------------------------------------- |
| ADR                      | `docs/adr/0011-admin-identity.md`, `docs/adr/0012-delivery-semantics.md`                                                         |
| Datos                    | `docs/data/erd.md`                                                                                                               |
| API                      | `docs/api/openapi.yaml`, `api-design.md`, `endpoint-inventory.md`, `error-codes.md`                                              |
| Seguridad                | `docs/security/rbac-matrix.md`                                                                                                   |
| Arquitectura             | `docs/architecture/hexagonal.md`, `project-structure.md`                                                                         |
| Mensajería y proveedores | `docs/messaging/rabbitmq-outbox.md`, `docs/providers/provider-adapters.md`, `provider-contracts-v1.5.md`                         |
| Aceptación               | `docs/acceptance/criteria.md`                                                                                                    |
| Proyecto                 | `docs/project/DECISIONS-v1.5.md`, `PENDING-DECISIONS.md` (solo la nota histórica de PD-03), `notification-architecture-index.md` |
| Operación                | `docs/operations/technical-pending-register.md`                                                                                  |
| Raíz                     | `README.md`, `AGENTS.md`                                                                                                         |

## 8. Archivos que deliberadamente no se modificaron

- Todo `src/` y `tests/`, incluido `GlobalExceptionMiddleware.cs` (TP-09 es una tarea de código independiente).
- `*.csproj`, `Directory.Build.props`, `Directory.Packages.props`, `global.json`, Dockerfiles, `.env.example` y `appsettings*.json`.
- `CLAUDE.md`: sigue siendo coherente; remite a §19 de `AGENTS.md`, que sí se actualizó.
- ADR-0004 y los demás ADR, sin relación con estas decisiones.
- `docs/operations/decision-closure-report.md` y `repository-sanitization-report.md`: informes históricos con fecha.
- Diagramas C4 y de topología: sin errores demostrables, así que no se tocaron.
- Los esquemas OpenAPI sin uso preexistentes se registraron en TP-22 y no se eliminaron.

## 9. Cambios en OpenAPI

| Cambio                         | Detalle                                                                                                                                                                                                                                                                                                                                                              |
| ------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Rutas normalizadas (TP-16 A)   | `/management//dashboard/{summary,deliveries,providers-health,consumption}` → `/management/dashboard/...`; `/management//audit/events` → `/management/audit/events`. Los `operationId` no cambian                                                                                                                                                                     |
| Paginación retirada (TP-16 B)  | 15 operaciones GET de recurso individual: `getNotificationById`, `getPublicTemplate`, `getOrganization`, `getApplication`, `getUser`, `getRole`, `getProviderAccount`, `getTemplate`, `getCampaign`, `getProviderHealth`, `getProviderInstanceQr`, `getProviderInstanceStatus`, `getSender`, `getCampaignStatistics`, `getScheduledNotification`                     |
| Operaciones añadidas (TP-16 C) | `deleteProviderAccount` y `deleteTemplate` (eliminación lógica); `updateApplicationStatus` y `updateUserStatus` (PATCH status); `setSystemRolePermissions` (`PUT /management/roles/{roleId}/permissions`, solo `super_admin`, auditada)                                                                                                                              |
| Retirada (TP-16 C/D)           | `PUT /management/users/{userId}/roles` se eliminó del inventario (no existía en OpenAPI)                                                                                                                                                                                                                                                                             |
| Roles (TP-20)                  | Retirados `createRole`, `updateRole`, `CreateRoleRequest`, `UpdateRoleRequest` y el esquema huérfano `Roles`. `Role.code` es un enum del catálogo. Se mantienen `listRoles`/`getRole` como catálogo de solo lectura                                                                                                                                                  |
| Platform API (TP-15)           | Nueva etiqueta `Platform`. `GET/POST /platform/super-admins` (`listSuperAdminAssignments`, `grantSuperAdmin`) y `POST /platform/super-admins/{assignmentId}/revoke` (`revokeSuperAdmin`). Esquemas `PlatformRoleAssignment` (con `grantedByActorType` `USER`/`SYSTEM_BOOTSTRAP`), `PlatformRoleAssignmentPage`, `GrantSuperAdminRequest` y `RevokeSuperAdminRequest` |
| Diagnóstico (TP-17)            | `ProviderTestResult` con `outcome` neutral (`SUCCEEDED`, `AUTHENTICATION_FAILED`, `CONFIGURATION_INVALID`, `UNREACHABLE`, `TIMEOUT`) y `ProviderHealth` con `status` (`HEALTHY`, `DEGRADED`, `UNHEALTHY`, `NOT_CHECKED`), ambos sin secretos. `422 PROVIDER_DIAGNOSTICS_NOT_SUPPORTED` en las dos operaciones                                                        |
| Adjuntos (TP-18)               | `attachmentIds` con `uniqueItems` y semántica de reutilización; `Attachment.expiresAt`; `422 ATTACHMENT_NOT_AVAILABLE` en las operaciones de creación                                                                                                                                                                                                                |
| Reenvío manual (TP-19)         | `retryNotification` pasa a ser reenvío manual: cuerpo `ResendNotificationRequest` (`reason`), `Idempotency-Key`, `202 AcceptedNotification` con `resendOfNotificationId`. También se añadió `resendOfNotificationId` a `NotificationStatus`                                                                                                                          |

Resultado: de 93 a **99 operaciones** (−2 + 8) y **77 rutas**.

## 10. Cambios en el ERD

- **Nueva `notification_attachments`:** PK (`notification_id`,`attachment_id`), `created_at`, FK a ambas tablas, misma organización y aplicación, índice por `attachment_id`.
- **`attachments`:** nuevo `deleted_at`; reglas de reutilización, eliminación lógica y purga en MinIO por lifecycle (archivo único, sin copias).
- **`platform_role_assignments`:**
  - nuevos `granted_by_actor_type` (`USER`/`SYSTEM_BOOTSTRAP`, con CHECK de coherencia con `granted_by`), `grant_reason` y `revocation_reason`;
  - idempotencia del bootstrap mediante el UNIQUE parcial;
  - protección del último activo.
- **`notifications`:** `resend_of_notification_id` (autorreferencia de la misma organización/aplicación), `resend_reason` y `resend_requested_by_type`/`_id`.
- **`notification_deliveries`:** campos de conciliación `reconciliation_started_at`, `reconciliation_deadline_at`, `next_reconciliation_at`, `reconciliation_checks` y `requires_manual_intervention`. Una entrega por notificación; los reintentos técnicos son intentos de la misma entrega.
- **`roles`:** pasa a ser un catálogo del sistema (`id`, `code`, `name`, `description`) con CHECK `code IN ('organization_admin','operator','viewer')`. Se retiran `organization_id` e `is_system`.
- **Mermaid:** se retira `ORGANIZATIONS → ROLES`. Se añaden `NOTIFICATIONS → NOTIFICATION_ATTACHMENTS`, `ATTACHMENTS → NOTIFICATION_ATTACHMENTS`, `ORGANIZATIONS → ATTACHMENTS` y la autorrelación `resend_of`. Desaparece la nota "vínculo pendiente (TP-18)".
- **Reglas de integridad:**
  - la 5 incluye `notification_attachments` entre las tablas que heredan el tenant;
  - nuevas reglas 12 (adjuntos del mismo tenant y aplicación) y 13 (reenvío y conciliación frente a la cuota);
  - `audit_logs` recoge `SYSTEM_BOOTSTRAP`, los permisos de roles y las resoluciones manuales.
- Los estados de PD-02 **no cambian**.

## 11. Cambios en RBAC

- Catálogo cerrado; sin roles personalizados.
- Nuevas filas: "Conceder/revocar `super_admin`" y "Ajustar permisos de roles del sistema", ambas solo para `super_admin`.
- Asignación de roles exclusivamente por membresía.
- Bootstrap y Platform API para `super_admin`.
- Los permisos de los roles existentes no cambiaron.

## 12. Cambios en ADR

- **ADR-0011:**
  - regla de catálogo cerrado (TP-20);
  - nueva sección "Administración de `super_admin`" (TP-15);
  - historial ampliado con la retirada de roles personalizados;
  - consecuencias ajustadas.
  - Se mantiene el estado "Aceptado".
- **ADR-0012:** nueva sección "Reintento técnico, reenvío manual y conciliación" (TP-19), con una tabla comparativa, la política de conciliación configurable y la intervención manual. Se sustituyó la frase que dejaba TP-19 abierto. Los estados y el diagrama de PD-02 no cambiaron.

## 13. Cambios en los contratos de proveedores

- **`hexagonal.md`:** `IProviderAccountDiagnostics` en la tabla de puertos, con sus reglas: opcional, separado del envío, resultados neutrales, timeouts.
- **`provider-adapters.md`:** tabla de diagnóstico y conciliación por proveedor (Evolution, SMTP, SMSGate), con las capacidades marcadas "por validar" donde no hay evidencia.
- **`provider-contracts-v1.5.md`:** puntos de validación de Evolution para la conciliación y el diagnóstico.

## 14. Resultados de validación (reales)

| Validación                               | Herramienta                                                  | Resultado                                                                                                                                                                                                                                                                                               |
| ---------------------------------------- | ------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| YAML parse                               | PyYAML 6.0.3                                                 | OK                                                                                                                                                                                                                                                                                                      |
| OpenAPI 3.1                              | `openapi-spec-validator` 0.9.0                               | OK                                                                                                                                                                                                                                                                                                      |
| `$ref`                                   | Script propio                                                | 635 referencias, 97 distintas, **0 sin resolver**                                                                                                                                                                                                                                                       |
| `operationId`                            | Script propio                                                | 99 operaciones, **0 duplicados**                                                                                                                                                                                                                                                                        |
| Rutas                                    | Script propio                                                | 77, **0 duplicadas**, **0 con doble barra**                                                                                                                                                                                                                                                             |
| Parámetros de ruta                       | Script propio                                                | **0** placeholders incoherentes                                                                                                                                                                                                                                                                         |
| Paginación                               | Script propio                                                | **0** operaciones de recurso individual con `page`/`pageSize`                                                                                                                                                                                                                                           |
| Respuestas de error                      | Script propio                                                | Todas apuntan a su componente RFC 9457: 400×99, 401×99, 403×99, 404×94, 409×25, 413×4, 415×4, 422×14, 429×12, 503×8                                                                                                                                                                                     |
| `Idempotency-Key`                        | Script propio                                                | Presente en `createNotification`, `createBulkNotifications`, `createCampaign`, `createScheduledNotification` y `retryNotification`                                                                                                                                                                      |
| Roles / Platform                         | Script propio                                                | Sin `createRole`/`updateRole`/`CreateRoleRequest`/`UpdateRoleRequest`; sin asignación de roles a usuario; operaciones Platform con etiqueta `Platform`, `AdminBearer` y fuera de membresías; sin `RECONCILIATION_REQUIRED` público                                                                      |
| ERD                                      | Script propio                                                | 40 entidades en Mermaid = 40 tablas en el diccionario; 47 relaciones explicadas; `notification_attachments` con PK y relaciones; `platform_role_assignments` con `SYSTEM_BOOTSTRAP` y protección; `roles` sin `organization_id` y con CHECK de catálogo; relación `resend_of`; estados de PD-02 exactos |
| Mermaid (oficial)                        | `@mermaid-js/mermaid-cli` 11.4.2 vía `npx`, en el scratchpad | **7 de 7 bloques renderizados.** Control negativo: un diagrama roto devolvió `Parse error on line 2`, como se esperaba                                                                                                                                                                                  |
| Mermaid (estructural)                    | Script propio                                                | 7 bloques, 0 problemas                                                                                                                                                                                                                                                                                  |
| Términos obsoletos                       | `grep`                                                       | `personalizad`, `createRole`, `updateRole`, `is_system`, `users/{userId}/roles`, `plantillas iniciales` solo aparecen como "retirado"/"no existen" o en historial; `management//` y `CreateRoleRequest`: 0                                                                                              |
| Enlaces relativos y anclas               | Script propio                                                | Ver §16 (ejecución final)                                                                                                                                                                                                                                                                               |
| Código sin cambios                       | `git status`                                                 | 0 archivos fuera de `docs/`, `README.md` y `AGENTS.md`                                                                                                                                                                                                                                                  |
| `dotnet build` / `dotnet test` (Release) | .NET SDK 10.0.404                                            | Ver §16 (ejecución final)                                                                                                                                                                                                                                                                               |

## 15. Errores encontrados y cómo se resolvieron

| Error                                                                                             | Resolución                                                                        |
| ------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------- |
| Heredoc de bash con script Python largo no parseable                                              | El script se escribió como archivo en el scratchpad                               |
| `npx --no-install mmdc` no localizaba el ejecutable (error de herramienta, no de los diagramas)   | Se invocó `npx --yes @mermaid-js/mermaid-cli@11.4.2` (cacheado)                   |
| El validador del ERD exigía el CHECK antiguo `code <> 'super_admin'`                              | Se actualizó al CHECK de catálogo, más estricto                                   |
| Dos falsos positivos del validador ("Sin `organization_id`"; "(TP-18) \|" en la fila de relación) | Las comprobaciones se limitaron a la columna de campos y a la palabra "pendiente" |
| La nota histórica de PD-03 decía que el diagnóstico quedaba "abierto en TP-17"                    | Anotada como resuelta                                                             |

## 16. Validaciones no ejecutadas y ejecución final

- **Docker build** de las imágenes: no ejecutado (TP-01; fuera del alcance documental).
- **Pruebas de Testcontainers:** no existen todavía (TP-02 planificado para la Fase 2).
- **Ejecución final, con este informe ya creado:**
  - enlaces: 48 archivos Markdown y 267 enlaces relativos y anclas, **0 rotos**;
  - `git status`: 0 archivos modificados fuera de la documentación;
  - `dotnet build -c Release`: Build succeeded, **0 advertencias, 0 errores**;
  - `dotnet test -c Release`: **63 superadas, 0 fallidas** (Domain 1, Application 2, Infrastructure 15, Api 14, Worker 5, Architecture 26). La línea de `Application.Tests` se perdió al intercalarse la salida en paralelo; se volvió a ejecutar por separado y dio 2/2.

## 17. Pendientes restantes

- **Implementación:** TP-02, TP-09, TP-15, TP-17, TP-18 y TP-19 (ver el [registro](../operations/technical-pending-register.md)).
- **Requieren aprobación:**
  - **TP-21:** TP-19 habla de "persona autorizada", pero `retryNotification` usa API Key y la matriz RBAC permite el reintento a la API Key "según scope". El contrato conserva `ApiKeyAuth` hasta que se decida.
  - **TP-22:** `api-design.md` lista `PATCH .../memberships/{membershipId}/status` y `PATCH /senders/{id}/default`, que no están en el inventario ni en OpenAPI. Además hay 8 esquemas OpenAPI sin uso.
  - TP-05, TP-06 y TP-07.
- **Abiertos sin cambio:** TP-01, TP-03, TP-04, TP-08, TP-10, TP-11, TP-12 y TP-14.
- **Cerrados:** TP-13; y TP-23 (validación Mermaid oficial), con evidencia.

## 18. Riesgos identificados

- **TP-09:** mientras no se corrija, la respuesta 500 real no cumple el `ProblemDetails` del contrato (falta `type`).
- **TP-21:** si el reenvío manual sigue en la Public API, el "actor autorizado" es una aplicación, no una persona. Hay que decidirlo antes de implementar `retryNotification`.
- **Capacidades de proveedores sin validar:** la consulta de estado de mensajes, de la que depende la conciliación automática, sigue "por validar" en Evolution y SMSGate. Sin ella, toda entrega ambigua acabará en intervención manual.
- **Versión de PostgreSQL de producción sin fijar:** condiciona la imagen de Testcontainers (TP-02).

## 19. Recomendación sobre la autorización de la Fase 2

**Preparado para solicitar autorización de la Fase 2.** El modelo de datos que necesita la persistencia ya está decidido y validado: roles, `super_admin`, estados, adjuntos, reenvío y conciliación. La herramienta de pruebas de integración también está aprobada (Testcontainers).

Antes de las tareas concretas conviene decidir:

- **TP-21**, porque afecta a `retryNotification` y no al esquema;
- la **versión de PostgreSQL** de producción, que define la imagen de Testcontainers.

Ninguno de los dos bloquea las migraciones del modelo. **La Fase 2 sigue bloqueada hasta que el usuario la autorice expresamente.**
