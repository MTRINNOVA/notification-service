# Registro de pendientes técnicos

**Registro operativo único** del proyecto (consolidado el 2026-11-15). Cualquier pendiente técnico, de validación externa o de producción se registra **aquí** y en ningún otro documento. Las listas que existían en `DECISIONS-v1.5.md`, `technical-readiness-v1.5.md`, `environment-decisions.md` y `docker.md` se migraron a este registro (TP-24 a TP-32).

No sustituye a los ADR ni a [DECISIONS-v1.5.md](../project/DECISIONS-v1.5.md). El historial de PD-01 a PD-08 está en [PENDING-DECISIONS.md](../project/PENDING-DECISIONS.md), que es solo un registro histórico. Los informes de auditoría de `docs/operations/*-report*.md` son evidencia histórica, no fuente operativa.

**Estados**

| Estado | Significado |
|---|---|
| `Abierto` | Sin decisión |
| `Requiere aprobación` | Hay propuesta, pero no decisión |
| `Aprobado — implementación pendiente` | Decisión documentada; falta código |
| `Planificado (fase N)` | Previsto para una fase concreta |
| `Corregido documentalmente` | Resuelto en la documentación, con evidencia |
| `Cerrado` | Resuelto con evidencia |

Documentar una solución **no** cierra un pendiente de implementación.

## Decisiones aprobadas con trabajo de implementación pendiente

| ID | Estado | Decisión aprobada | Documentos afectados | Evidencia de validación | Trabajo para implementación |
|---|---|---|---|---|---|
| TP-02 | **Aprobado — planificado (Fase 2)** | Testcontainers para .NET con **PostgreSQL 17** real; imagen fijada (patch explícito y digest), sin `latest`; local y CI; sin EF InMemory | [integration-testing-postgresql.md](../testing/integration-testing-postgresql.md), `AGENTS.md`, `README.md` | Documento creado; enlaces validados | Paquetes en `Directory.Packages.props`, fixture xUnit, imagen `postgres:17.<patch>`, migraciones antes de las pruebas, job de CI con Docker |
| TP-09 | **Aprobado — corrección de código pendiente (tarea independiente)** | `GlobalExceptionMiddleware` debe emitir `type: about:blank` | [error-codes.md](../api/error-codes.md), [openapi.yaml](../api/openapi.yaml) (`ProblemDetails.type` obligatorio) | Revisión del código: el middleware emite `status`, `title`, `detail`, `instance`, `code` y `traceId`, pero **no `type`** | Añadir `Type = "about:blank"` y una prueba. **Aceptación:** las respuestas 500 son `application/problem+json` con `type: about:blank`, `title`, `status`, `detail`, `instance`, `code = INTERNAL_SERVER_ERROR` y `traceId` |
| TP-15 | **Aprobado — implementación pendiente** | Bootstrap manual e idempotente (`SYSTEM_BOOTSTRAP`) más Platform API (`/platform/super-admins`); recuperación de emergencia; protección del último activo | [ADR-0011](../adr/0011-admin-identity.md), [super-admin-bootstrap.md](super-admin-bootstrap.md), [erd.md](../data/erd.md), [rbac-matrix.md](../security/rbac-matrix.md), [openapi.yaml](../api/openapi.yaml) | OpenAPI válido con 3 operaciones Platform separadas de membresías; ERD validado | Tarea de bootstrap, endpoints Platform, bloqueo transaccional del último activo, auditoría |
| TP-17 | **Aprobado — implementación pendiente** | Puerto opcional `IProviderAccountDiagnostics`, separado del envío, con resultados neutrales | [hexagonal.md](../architecture/hexagonal.md), [provider-adapters.md](../providers/provider-adapters.md), [openapi.yaml](../api/openapi.yaml) | OpenAPI con `ProviderTestResult`/`ProviderHealth` neutrales | Interfaz y adaptadores tras validar los contratos reales |
| TP-18 | **Aprobado — modelo lógico validado; implementación pendiente** | `notification_attachments` (muchos a muchos, mismo tenant y aplicación) | [erd.md](../data/erd.md), [criteria.md](../acceptance/criteria.md), [openapi.yaml](../api/openapi.yaml) | Validador ERD | Migración y restricciones (Fase 2); lifecycle de MinIO |
| TP-19 | **Aprobado — semántica documentada; implementación pendiente** | Reintento técnico ≠ reenvío manual ≠ conciliación; política de conciliación configurable por proveedor | [ADR-0012](../adr/0012-delivery-semantics.md), [erd.md](../data/erd.md), [rabbitmq-outbox.md](../messaging/rabbitmq-outbox.md), [openapi.yaml](../api/openapi.yaml) | Estados de PD-02 sin cambios | Valores de la política por proveedor; Scheduler de conciliación; operación de resolución manual de conciliación |

## Cerrados o corregidos documentalmente (trazabilidad)

| ID | Estado | Resolución | Evidencia |
|---|---|---|---|
| TP-13 | Cerrado | El usuario retiró del índice las referencias a documentos inexistentes | Índice sin enlaces rotos |
| TP-16 | Corregido documentalmente | Contrato OpenAPI normalizado (rutas, paginación, operaciones del inventario); `PUT /users/{userId}/roles` retirada | Validador OpenAPI |
| TP-20 | Corregido documentalmente | Catálogo cerrado de roles; sin roles personalizados en v1 | Validadores OpenAPI y ERD |
| TP-21 | **Cerrado** (2026-11-15) | **Opción A:** el reenvío manual (`POST /public/notifications/{notificationId}/retry`) lo solicita la **aplicación consumidora** con su API Key y el scope `notifications.retry`. El actor es la aplicación, nunca un usuario humano. La auditoría registra `actor_type = APPLICATION`, `actor_id` = aplicación y `api_key_id`; la notificación guarda `resend_requested_by_api_key_id`. No hay operación de reenvío para usuarios del panel | ADR-0012, ERD, RBAC, OpenAPI e inventario alineados; búsqueda de actores humanos en el flujo sin resultados vigentes ([documentation-consolidation-report.md](documentation-consolidation-report.md)) |
| TP-22 | **Cerrado** (2026-11-15) | Las 2 rutas y los 8 esquemas tienen una decisión fundamentada (ver el informe): se añadió `PATCH .../memberships/{membershipId}/status`; se descartó `PATCH /senders/{id}/default`, ya cubierta por `defaultSenderId`; se eliminaron 6 esquemas y 2 se fusionaron en `CampaignPage`/`ScheduledNotificationPage`, que estaban incompletos | Validador OpenAPI: 0 esquemas sin uso; inventario regenerado desde OpenAPI |
| TP-23 | Cerrado | Validación Mermaid con el parser oficial `@mermaid-js/mermaid-cli` 11.4.2 | 7/7 bloques renderizados; control negativo detectado |

## Pendientes técnicos abiertos

| ID | Pendiente | Impacto | Decisión requerida / propuesta | Estado |
|---|---|---|---|---|
| TP-01 | Imágenes Docker sin evidencia de build (daemon Docker no disponible el 2026-11-15) | Los Dockerfiles de `notification-api`, `notification-worker` y `notification-scheduler` no están verificados | Ejecutar `docker build -f src/NotificationService.<Host>/Dockerfile .` para los tres hosts antes de publicar en GHCR | Abierto |
| TP-03 | `dotnet-ef` y `Microsoft.EntityFrameworkCore.Design` no instalados | Necesarios para migraciones | Al autorizar la Fase 2: manifiesto local con `dotnet-ef` 10.0.x igual a EF Core, y `Design` (con `PrivateAssets=all`) | Planificado (Fase 2) |
| TP-04 | Readiness sin comprobaciones de dependencias | `/health/ready` responde *Healthy* sin dependencias implementadas | Registrar cada dependencia con la etiqueta `ready` en su fase (PostgreSQL en Fase 2) | Planificado (por fase) |
| TP-05 | Worker y Scheduler sin sonda de salud | Docker/Portainer no distingue liveness/readiness | Endpoint HTTP mínimo, archivo de heartbeat o `HEALTHCHECK` | Requiere aprobación |
| TP-06 | No existe `NotificationService.Scheduler.Tests` | Las pruebas del Scheduler viven en `Architecture.Tests` | Crear el proyecto cuando el Scheduler tenga lógica propia | Requiere aprobación |
| TP-07 | Sin CI/CD | Restore/build/test/vulnerabilidades sin verificación automática | GitHub Actions: build Release, test (con Docker para Testcontainers), `--vulnerable`, imágenes y publicación en GHCR | Requiere aprobación |
| TP-08 | Sin Docker Compose | Topología solo documentada | Compose de desarrollo y, después, de producción (Traefik externo, `NOTIFICATIONS_*_HOST`, PostgreSQL 17) | Planificado |
| TP-10 | Paquetes de prueba con versiones mayores nuevas | Ninguno funcional | Evaluar en un cambio dedicado | Abierto |
| TP-11 | Imágenes con tags flotantes (runtime `10.0`; PostgreSQL 17 sin patch/digest fijado) | Builds y entornos no reproducibles | Fijar tags patch y digests en el manifiesto de versiones de release ([docker.md](../deployment/docker.md)) | Abierto |
| TP-12 | `AllowedHosts: "*"` en la API | Sin filtrado de cabecera Host | Configurar por entorno a partir de `NOTIFICATIONS_*_HOST` cuando existan los DNS | Abierto |
| TP-14 | Posible divergencia entre `/openapi/v1.json` (generado) y `openapi.yaml` (normativo) | El cliente podría usar un contrato no normativo | Pruebas de contrato al implementar los endpoints | Planificado |
| TP-24 | Validación en ejecución de Evolution v2.3.7 y fijación de su digest | Sin evidencia de instancia, QR, persistencia, reconexión, webhooks, envío texto/media ni consulta de estado | Ejecutar las pruebas de [provider-contracts-v1.5.md](../providers/provider-contracts-v1.5.md) y verificar el digest desplegado | Abierto (antes de habilitar el canal) |
| TP-25 | SMSGate: release/digest, app Android, dispositivo/SIM y discrepancia Basic Auth vs token privado | Contrato del adaptador SMS sin validar | Fijar la versión y validar autenticación, endpoint y callbacks | Abierto (antes de habilitar el canal) |
| TP-26 | Postfix/SMTP: TLS, autenticación, DNS (SPF, DKIM, DMARC, PTR) y rebotes | Entregabilidad y reputación sin verificar | Pruebas SMTP y verificación DNS antes del envío productivo | Abierto (antes de habilitar el canal) |
| TP-27 | Infraestructura de producción: dominio raíz y DNS, nombre de la red Traefik y labels, ubicación de servicios (VPS compartido/separado; Evolution, Postfix, SMSGate), paths, filesystem, UID/GID, espacio y permisos | Bloquea el Compose de producción | Datos de infraestructura a confirmar por el responsable del VPS | Abierto |
| TP-28 | `GET /public/attachments/{attachmentId}` sin scope definido | No se sabe qué permiso exige la consulta de metadatos/URL firmada | Definir el scope (p.ej. `attachments.read` o reutilizar `attachments.upload`) | Requiere aprobación |
| TP-29 | Backups, RPO/RTO y prueba de restauración (PostgreSQL y MinIO) | Sin objetivo de recuperación aprobado | Definir RPO/RTO y documentar una restauración probada | Abierto |
| TP-30 | Límites de CPU/RAM/disco, observabilidad y dimensionamiento (organizaciones, números WhatsApp, dispositivos SMS, adjuntos, mezcla de canales) | Riesgo de saturación del VPS compartido | Medir con pruebas de carga y fijar límites y alertas | Abierto |
| TP-31 | Rotación de las credenciales del ZIP legado y limpieza del historial Git si se versionaron | Credenciales expuestas | Rotar/revocar y revisar el historial | Abierto |
| TP-32 | Pruebas de aislamiento multi-tenant, carga y recuperación | Sin evidencia de aislamiento ni de resiliencia | Ejecutarlas antes de producción ([criteria.md](../acceptance/criteria.md) §10) | Abierto |
| TP-33 | Ambigüedad del remitente predeterminado: `whatsapp_senders.is_default` (ERD) y `Sender.isDefault` (OpenAPI) frente al predeterminado **por aplicación** (`application_senders.is_default`, `defaultSenderId`) aprobado en DECISIONS §3 | Dos fuentes de "predeterminado" | Retirar `whatsapp_senders.is_default`/`Sender.isDefault` o definir su significado | Requiere aprobación |
| TP-34 | 9 esquemas `*Page` sin `page`/`pageSize`/`total`: `ProviderDefinitionPage`, `ProviderInstancePage`, `SenderPage`, `TemplateVersionPage`, `CampaignRecipientPage`, `MembershipPage`, `ApiKeyPage`, `ProviderHealthPage`, `AuditEventPage` | Colecciones paginadas sin metadatos de paginación en el contrato | Completar los esquemas en una revisión de contrato | Requiere aprobación |
| TP-35 | Protección del último `organization_admin` | Con `updateMembershipStatus` y `setMembershipRoles`, una organización podría quedarse sin administrador activo; no hay regla aprobada | Decidir si se rechaza (análogo a TP-15) o si se delega a `super_admin` | Requiere aprobación |
| TP-36 | Cancelación por usuarios del panel | La matriz RBAC concede "Cancelar notificaciones" a roles humanos, pero la única operación es `POST /public/notifications/{id}/cancel` (API Key) | Añadir una operación de Management o ajustar la matriz | Requiere aprobación |
| TP-37 | Autenticación por cookie no declarada en OpenAPI | `refreshSession` y `logout` usan la cookie `HttpOnly` del refresh token, pero declaran `security: []` | Declarar un `securityScheme` de tipo cookie en una revisión de contrato | Requiere aprobación |
