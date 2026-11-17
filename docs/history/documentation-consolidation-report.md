# Informe de consolidación documental — TP-21, TP-22 y fuente única de verdad (2026-11-15)

> **Documento histórico** (evidencia de esta auditoría). El estado vigente de los pendientes está en [technical-pending-register.md](../operations/technical-pending-register.md) y las decisiones, en [DECISIONS-v1.5.md](../project/DECISIONS-v1.5.md).

Rama `main`, commit inicial `78b6b42`, con el árbol de trabajo limpio al empezar. **Tarea exclusivamente documental:** no se modificó código, proyectos, configuración ejecutable, Compose, CI/CD, migraciones ni paquetes. **No se inició la Fase 2.** No se hicieron commits ni push.

## 1. Archivos revisados

`openapi.yaml`, `api-design.md`, `endpoint-inventory.md`, `error-codes.md`, `erd.md`, ADR-0011, ADR-0012, `rbac-matrix.md`, `security-model.md`, `criteria.md`, `DECISIONS-v1.5.md`, `PENDING-DECISIONS.md`, `technical-pending-register.md`, `technical-readiness-v1.5.md`, `environment-decisions.md`, `docker.md`, `compose-topology.md`, `provider-contracts-v1.5.md`, `legacy-source-assessment.md`, `integration-testing-postgresql.md`, el índice, los tres informes anteriores, `README.md` y `AGENTS.md`.

## 2. Archivos modificados (20) y creados (1)

| Archivo | Cambio |
|---|---|
| `docs/api/openapi.yaml` | TP-21: el actor de `retryNotification` es la aplicación; `ApiKeyAuth` describe los scopes. TP-22: nueva `updateMembershipStatus`; 6 esquemas eliminados; `CampaignPage` y `ScheduledNotificationPage` corregidos. Editado con cambios de texto para **conservar el formato Prettier** del usuario (diff de unas 150 líneas) |
| `docs/api/api-design.md` | Pasa a ser un documento de **criterios**: se eliminan las tablas de rutas que competían con OpenAPI y se añaden la sección "Fuente de verdad", las superficies de API y la regla de membresías. Se corrige un defecto de formato en "Códigos HTTP" |
| `docs/api/endpoint-inventory.md` | **Regenerado desde `openapi.yaml`** (100 operaciones), marcado como documento derivado |
| `docs/data/erd.md` | TP-21: `resend_requested_by_api_key_id`, `audit_logs.api_key_id`, catálogo `actor_type` con `APPLICATION`, 2 relaciones Mermaid y una fila en "Relaciones" |
| `docs/adr/0012-delivery-semantics.md` | TP-21: el actor del reenvío manual es la aplicación (API Key + scope) |
| `docs/security/rbac-matrix.md` | "Cancelar/reintentar" se divide en "Cancelar notificaciones" y "Reenvío manual (`retry`)"; nueva regla TP-21 |
| `docs/acceptance/criteria.md` | Criterios del estado de membresía y del actor del reenvío |
| `docs/operations/technical-pending-register.md` | **Registro único**: TP-21/TP-22 cerrados; migración de listas dispersas (TP-24..TP-32); hallazgos nuevos (TP-28, TP-33..TP-37) |
| `docs/project/DECISIONS-v1.5.md` | Sección "Base de datos" (PostgreSQL 17); filas TP-21/TP-22; la lista de pendientes se sustituye por su mapeo al registro |
| `docs/operations/technical-readiness-v1.5.md`, `docs/deployment/environment-decisions.md`, `docs/deployment/docker.md` | Las listas de pendientes se sustituyen por referencias a los TP del registro |
| `docs/providers/provider-contracts-v1.5.md`, `docs/testing/integration-testing-postgresql.md` | PostgreSQL 17 (patch y digest explícitos; nunca `latest`) |
| `docs/operations/repository-sanitization-report.md`, `decision-closure-report.md`, `decision-closure-report-tp15-tp20.md` | Aviso de **documento histórico** |
| `docs/project/notification-architecture-index.md` | Reescrito: normativos frente a históricos, con enlaces relativos |
| `README.md`, `AGENTS.md` | Fuente única de verdad, registro único, PostgreSQL 17, TP-21/TP-22 cerrados |
| **Creado:** `docs/operations/documentation-consolidation-report.md` | Este informe |

**Archivos eliminados o archivados:** ninguno. Los informes anteriores se conservan como evidencia histórica, marcados como tales, y `PENDING-DECISIONS.md` sigue siendo un historial. No se eliminó documentación porque nada quedó obsoleto sin valor de trazabilidad.

**Nota de formato:** las ediciones de Markdown dentro de tablas alineadas por Prettier pueden dejar columnas sin alinear. El contenido es correcto; basta volver a pasar Prettier.

## 3. Decisión final de TP-21

**Opción A (aprobada):** `POST /public/notifications/{notificationId}/retry` con API Key y scope `notifications.retry`.

| Aspecto | Resultado |
|---|---|
| Actor | La **aplicación consumidora**, nunca un usuario humano |
| Modelo | `notifications.resend_requested_by_api_key_id` (FK → `api_keys` de la misma aplicación) sustituye a `resend_requested_by_type`/`_id` |
| Auditoría | `audit_logs.actor_type = APPLICATION`, `actor_id` = aplicación y `api_key_id` obligatorio (CHECK). Una operación de API Key nunca se registra como `USER`. `delivery_status_history` usa `APPLICATION` en lugar de `API_KEY` |
| RBAC | Reenvío manual: solo API Key con scope; los roles humanos no tienen operación de reenvío en v1 |
| Sin cambios | Reintento técnico y conciliación; semántica de nueva notificación, nueva `Idempotency-Key`, motivo y consumo de cuota (TP-19) |

**TP-21: cerrado.**

## 4. Evaluación de rutas (TP-22)

| Ruta | Necesidad funcional | Pertenece a v1 | Decisión | Justificación |
|---|---|---|---|---|
| `PATCH /management/organizations/{organizationId}/memberships/{membershipId}/status` | Suspender, desactivar o reactivar el acceso de un usuario a **una** organización. Actor: `organization_admin` de esa organización o `super_admin` (auditado) | **Sí**: DECISIONS §1 ("cada organización administra sus administradores y operadores"); matriz RBAC "Administrar miembros: `organization_admin` Sí"; `organization_memberships.status` en el ERD; aceptación §1 (membresía activa) | **Mantener e incorporar** a OpenAPI (`updateMembershipStatus`) y al inventario | Sin ella, un administrador de organización no puede retirar el acceso de un miembro: no hay operación de baja de membresía. `PATCH /users/{id}/status` es global, afecta a todas las organizaciones del usuario y no corresponde a un `organization_admin`. `setMembershipRoles` con roles vacíos dejaría la membresía activa sin rol, que no equivale a suspenderla. Se añade por necesidad funcional, **no** por coincidir con `api-design.md`. Queda abierto TP-35 (proteger al último `organization_admin`) |
| `PATCH /management/senders/{id}/default` | Marcar un remitente como predeterminado | El predeterminado aprobado es **por aplicación** (DECISIONS §3: "remitente predeterminado de la aplicación") | **Descartar** | Ya está cubierto por `PUT /management/applications/{applicationId}/senders` con `AssignSendersRequest.defaultSenderId`. La ruta descartada no tiene contexto de aplicación, así que no puede expresar el modelo aprobado. La ambigüedad de `whatsapp_senders.is_default`/`Sender.isDefault` queda en TP-33 |

## 5. Evaluación de esquemas (TP-22)

| Esquema | Referencias | Necesidad en v1 | Decisión | Justificación |
|---|---|---|---|---|
| `UpdateScheduledNotificationRequest` | 0 (ni operación ni otros esquemas; ningún documento) | No: v1 crea, consulta y **cancela** programaciones; no hay reprogramación aprobada | **Eliminar** | Sin operación que lo use; no se crea una operación para justificarlo |
| `Organizations` | 0 | No | **Eliminar** | Duplicado exacto de `OrganizationPage`, que usa `listOrganizations` |
| `Applications` | 0 | No | **Eliminar** | Duplicado exacto de `ApplicationPage` |
| `Users` | 0 | No | **Eliminar** | Duplicado exacto de `UserPage` |
| `ProviderAccounts` | 0 | No | **Eliminar** | Duplicado exacto de `ProviderAccountPage` |
| `Templates` | 0 | No | **Eliminar** | Duplicado exacto de `TemplatePage` |
| `Campaigns` | 0 | La **estructura** sí: `listCampaigns` usa `CampaignPage`, que no tenía `page`/`pageSize`/`total` | **Corregir (fusionar):** su definición sustituye a `CampaignPage` y el duplicado desaparece | El contrato vigente de la colección paginada era incorrecto; se corrige sin crear operaciones |
| `ScheduledNotifications` | 0 | La **estructura** sí: `listScheduledNotifications` usa `ScheduledNotificationPage`, que era un objeto libre | **Corregir (fusionar):** su definición sustituye a `ScheduledNotificationPage` | Igual que `Campaigns` |

Resultado: **0 esquemas sin uso** en `openapi.yaml`. La auditoría encontró además 9 esquemas `*Page` con la paginación incompleta, fuera de estos ocho; se registraron en TP-34 sin modificarlos.

**TP-22: cerrado.** Cada ruta y cada esquema tienen una decisión fundamentada.

## 6. Fuente de verdad del API v1

| Documento | Papel |
|---|---|
| `docs/api/openapi.yaml` | **Fuente normativa única**: operaciones, rutas, métodos, autenticación, parámetros, respuestas y esquemas |
| `docs/api/endpoint-inventory.md` | Resumen **derivado**, generado desde OpenAPI; se comprobó que está alineado |
| `docs/api/api-design.md` | Criterios de diseño; ya no contiene tablas de rutas propias |
| `docs/api/error-codes.md` | Catálogo de `code` referenciado por OpenAPI |

Es coherente con la regla que ya existía ("`openapi.yaml` es el contrato normativo"), que ahora queda explícita en `api-design.md`, `AGENTS.md`, `README.md` y el índice.

## 7. Registro único de pendientes

`docs/operations/technical-pending-register.md` es el único registro operativo.

**Migraciones al registro:**

| Origen | Destino |
|---|---|
| `DECISIONS-v1.5.md`, 8 puntos | TP-24 (Evolution), TP-25 (SMSGate), TP-26 (SMTP/DNS), TP-27 (infraestructura), TP-29 (RPO/RTO), TP-32 (pruebas de aislamiento, carga y recuperación). El punto 5 (OpenAPI/ERD) se resolvió |
| `technical-readiness-v1.5.md`, 8 puntos | TP-27, TP-29, TP-30, TP-31 |
| `environment-decisions.md`, 7 puntos | TP-27, TP-29, TP-30 |
| `docker.md` | TP-11, TP-27, TP-29, TP-30 |

**Nuevos hallazgos (requieren aprobación):**

- **TP-28:** scope de `GET /public/attachments/{id}`.
- **TP-33:** ambigüedad del remitente predeterminado.
- **TP-34:** 9 esquemas `*Page` incompletos.
- **TP-35:** protección del último `organization_admin`.
- **TP-36:** la matriz RBAC concede "Cancelar" a usuarios del panel, pero no existe operación de Management para hacerlo.
- **TP-37:** la autenticación por cookie de `refresh`/`logout` no está declarada en OpenAPI.

`risk-register.md` se mantiene como registro de **riesgos**, que es un concepto distinto.

## 8. Pendientes que continúan abiertos

- **Implementación:** TP-02 (planificado, Fase 2), TP-09 (corrección de código independiente), TP-15, TP-17, TP-18 y TP-19.
- **Requieren aprobación:** TP-05, TP-06, TP-07, TP-28, TP-33, TP-34, TP-35, TP-36 y TP-37.
- **Abiertos o planificados:** TP-01, TP-03, TP-04, TP-08, TP-10, TP-11, TP-12, TP-14 y TP-24 a TP-32.

## 9. Actualizaciones del índice

El índice real es `docs/project/notification-architecture-index.md`; no existe `docs/README.md`. Se reescribió con:

- secciones de normativos (decisiones y los 12 ADR, contrato API con OpenAPI como fuente única, arquitectura y datos, seguridad, mensajería y proveedores, operación y despliegue con el registro único, pruebas y aceptación);
- una sección de **históricos** (`PENDING-DECISIONS.md`, `CHANGELOG-v1.5.md` y los cuatro informes de auditoría);
- el frontend como referencia a otro repositorio.

Se sustituyeron las rutas en texto plano por enlaces relativos comprobables.

## 10. PostgreSQL

**PostgreSQL 17** queda confirmado para desarrollo, pruebas y producción, y Testcontainers lo usará en las pruebas de integración. La imagen se fija con patch explícito y digest (`postgres:17.<patch>`), nunca `latest` (TP-11). Está registrado en `DECISIONS-v1.5.md`, `integration-testing-postgresql.md`, `provider-contracts-v1.5.md`, `docker.md`, `AGENTS.md` y `README.md`.

Las únicas menciones a PostgreSQL 16 son evidencia del ZIP legado (`legacy-source-assessment.md` y la columna "Evidencia del ZIP" de `provider-contracts-v1.5.md`), identificadas como tales. No se tocó ningún Compose ni configuración ejecutable.

## 11. Validaciones ejecutadas (resultados reales)

| # | Validación | Herramienta | Resultado |
|---|---|---|---|
| 1 | Sintaxis y estructura OpenAPI 3.1 | PyYAML 6.0.3 + `openapi-spec-validator` 0.9.0 | OK |
| 2 | `$ref` | Script propio | 635 referencias, 97 distintas, **0 sin resolver** |
| 3 | `operationId` únicos | Script propio | 100 operaciones, **0 duplicados** |
| 4 | Rutas duplicadas, dobles barras, placeholders, paginación individual | Script propio | 78 rutas: 0 duplicadas, 0 dobles barras, 0 placeholders incoherentes, 0 paginaciones en recursos individuales; 0 esquemas sin uso |
| 5 | OpenAPI frente al inventario | Generador en modo `--check` | **INVENTARIO ALINEADO** (100 operaciones) |
| 6 | Esquemas eliminados no referenciados | `grep` | 0 referencias a cada uno de los 8 |
| 7 | Actores humanos en el reenvío manual | `grep` (`persona autorizada`, `actor autorizado`, `resend_requested_by_type/_id`, `Reenvío manual autorizado`) | 0 en documentos vigentes; solo la nota de cambios del ERD |
| 8 | Versiones de PostgreSQL | `grep` | Solo 17 como versión vigente; 16 únicamente como evidencia legada |
| 9 | Enlaces internos e índice | Script propio (slugs estilo GitHub) | Ver la ejecución final (§12) |
| 10 | Mermaid (el ERD cambió) | `@mermaid-js/mermaid-cli` 11.4.2 (oficial) + chequeo estructural | **7/7 bloques renderizados**; estructural sin problemas |
| — | ERD | Script propio | 40 entidades en el diagrama y en el diccionario; 49 relaciones explicadas; checks de TP-21 (`resend_requested_by_api_key_id`, `api_key_id`, `APPLICATION`) OK |
| — | Código sin cambios | `git status` | 0 archivos fuera de `docs/`, `README.md` y `AGENTS.md` |

## 12. Ejecución final y confirmación

Ejecutado **después** de crear este informe:

- **Enlaces:** 49 archivos Markdown y 317 enlaces relativos y anclas, **0 rotos**.
- **`git status`:** 0 archivos modificados fuera de la documentación.
- **`dotnet build -c Release`:** Build succeeded, 0 advertencias, 0 errores.
- **`dotnet test -c Release`** (por proyecto): **63/63 superadas** (Domain 1, Application 2, Infrastructure 15, Api 14, Worker 5, Architecture 26).

**Confirmación:** no se implementó la Fase 2 y no se modificó código ejecutable (`src/`, `tests/`, `*.csproj`, `*.props`, configuración, Dockerfiles, Compose ni CI/CD).
