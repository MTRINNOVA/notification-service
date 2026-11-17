# Matriz RBAC — v1.5

Roles de v1 según [ADR-0011](../adr/0011-admin-identity.md) (PD-01, aprobada el 2026-11-15): `super_admin` (privilegio de plataforma), `organization_admin`, `operator` y `viewer` (roles de membresía). `org_admin` deja de ser un nombre vigente, `application_admin` no existe en v1 y **no hay roles personalizados** (TP-20): la plataforma administra el catálogo de roles y sus permisos.

La autorización se evalúa por usuario + membresía activa + organización seleccionada. Los permisos de plataforma son independientes.

| Capacidad                               |               `super_admin` | `organization_admin` |             `operator` |         `viewer` |           API Key de aplicación |
| --------------------------------------- | --------------------------: | -------------------: | ---------------------: | ---------------: | ------------------------------: |
| Crear/suspender organizaciones          |                          Sí |                   No |                     No |               No |                              No |
| Administrar miembros de su organización |           Auditoría/soporte |                   Sí (nunca dejar la organización sin `organization_admin` activo, TP-35) |                     No |               No |                              No |
| Administrar aplicaciones                |                      Global |                   Sí |                     No |               No |                              No |
| Crear/rotar API Keys                    |                      Global |                   Sí |                     No |               No |                              No |
| Configurar proveedores y credenciales   |                      Global |                   Sí |                     No |               No |                              No |
| Conectar/desconectar WhatsApp           |                      Global |                   Sí |          Según permiso |               No |                              No |
| Asignar remitentes a aplicaciones       |                      Global |                   Sí |          Según permiso |               No |                              No |
| Crear/editar plantillas                 |                      Global |                   Sí |      Sí, si se concede |               No |                              No |
| Enviar notificaciones                   |              No por defecto |        Según permiso |          Según permiso |               No | Sí, solo su aplicación y scopes |
| Consultar notificaciones                |        Global con auditoría |                   Sí | Sí, dentro de permisos | Sí, solo lectura |          Sí, solo su aplicación |
| Cancelar notificaciones (TP-36) | Global con auditoría (Management API) | Sí, su organización (Management API) | Según permiso `notifications.cancel` (Management API) | No | Scope `notifications.cancel`, solo su aplicación (Public API) |
| Subir adjuntos | No (sin operación de Management) | No (sin operación de Management) | No (sin operación de Management) | No | Scope `attachments.upload`, solo su aplicación |
| Consultar adjuntos (metadatos y URL firmada) | No (sin operación de Management) | No (sin operación de Management) | No (sin operación de Management) | No | Scope `attachments.read` (TP-28), solo adjuntos de su organización y aplicación |
| Reenvío manual (`retry`) | No (solo Public API) | No (solo Public API) | No (solo Public API) | No | Sí: scope `notifications.retry`, solo su aplicación; el actor auditado es la aplicación (TP-21) |
| Administrar campañas                    |                      Global |                   Sí |          Según permiso |          Lectura |           Según scope explícito |
| Dashboard y consumo                     |                      Global |                   Sí |          Según permiso |      Sí, lectura |                              No |
| Consultar auditoría                     |                      Global |  Sí, su organización |          Según permiso |    Según permiso |                              No |
| Configurar límites de cuota de una organización (TP-45) | Sí, auditado (Platform API) | No | No | No | No |
| Crear usuarios con contraseña temporal (TP-46) | Sí | Con `members.manage` | No | No | No |
| Cambiar la propia contraseña | Sí | Sí | Sí | Sí | No |
| Conceder/revocar `super_admin`          | Sí (nunca el último activo) |                   No |                     No |               No |                              No |
| Ajustar permisos de roles del sistema   |                Sí, auditado |                   No |                     No |               No |                              No |

## Scopes de API Key (Public API)

Catálogo **cerrado** de scopes (esquema `ApiKeyScope` de [openapi.yaml](../api/openapi.yaml)). Cada operación pública declara el suyo en `x-required-scopes`; el [inventario](../api/endpoint-inventory.md) lo resume. Los scopes son un catálogo distinto de los permisos de rol humanos, aunque algún código coincida (`notifications.cancel`).

| Scope | Operaciones | Ámbito |
|---|---|---|
| `notifications.send` | `createNotification`, `createBulkNotifications` | Su aplicación |
| `notifications.read` | `listNotifications`, `getNotificationById`, `listNotificationEvents` | Su aplicación |
| `notifications.cancel` | `cancelNotification` | Su aplicación |
| `notifications.retry` | `retryNotification` (TP-21) | Su aplicación; actor `APPLICATION` |
| `attachments.upload` | `uploadAttachment` | Su aplicación |
| `attachments.read` | `getAttachment` (TP-28) | Solo adjuntos de la organización y la aplicación propietarias; otro tenant o aplicación → `404`. La `downloadUrl` es una URL firmada temporal, sin credenciales ni datos internos de MinIO |
| `templates.read` | `listPublicTemplates`, `getPublicTemplate` | Su aplicación |

`attachments.upload` no autoriza la lectura: una API Key que solo sube adjuntos y luego los referencia por `attachmentId` en un envío no necesita `attachments.read`.

## Catálogo de permisos de rol (TP-51)

Catálogo **cerrado** de la tabla `permissions`, cargado como dato de referencia, con las asignaciones iniciales de `role_permissions`. `super_admin` no usa este catálogo: es un privilegio de plataforma con acceso global.

| Código | Cubre | `organization_admin` | `operator` | `viewer` |
|---|---|:---:|:---:|:---:|
| `members.manage` | Usuarios, membresías y sus roles y estados | Por defecto | — | — |
| `applications.manage` | Aplicaciones, permisos, límites y estado | Por defecto | — | — |
| `api_keys.manage` | Crear, rotar y revocar API Keys | Por defecto | — | — |
| `providers.manage` | Cuentas de proveedor, credenciales y diagnóstico | Por defecto | — | — |
| `whatsapp.connect` | Instancias, QR, conexión y desconexión | Por defecto | Concedible | — |
| `senders.assign` | Asignar remitentes a aplicaciones | Por defecto | Concedible | — |
| `templates.manage` | Plantillas y versiones; vista previa y prueba | Por defecto | Concedible | — |
| `notifications.read` | Consultar notificaciones y su auditoría | Por defecto | Por defecto | Por defecto |
| `notifications.cancel` | Cancelar notificaciones (TP-36) | Por defecto | Concedible | — |
| `campaigns.manage` | Crear, editar, iniciar, pausar y cancelar campañas | Por defecto | Concedible | — |
| `campaigns.read` | Consultar campañas, destinatarios y estadísticas | Por defecto | Por defecto | Por defecto |
| `scheduling.manage` | Crear y cancelar programaciones | Por defecto | Concedible | — |
| `dashboard.read` | Dashboard y consumo | Por defecto | Por defecto | Por defecto |
| `audit.read` | Eventos de auditoría | Por defecto | Concedible | Concedible |

- **Por defecto:** incluido en la carga inicial.
- **Concedible:** solo `super_admin` puede añadirlo, con `PUT /management/roles/{roleId}/permissions`, auditado.
- **—:** no asignable a ese rol: `setSystemRolePermissions` lo rechaza con `422 PERMISSION_NOT_ASSIGNABLE`, porque ampliaría el rol fuera de su definición.

Las capacidades marcadas "Según permiso" en la matriz superior corresponden a los permisos *Concedible*.

## Definición de rol frente a asignación de privilegio

- **Definición de rol** (`roles` + `role_permissions`): catálogo **cerrado** del sistema con `organization_admin`, `operator` y `viewer`, compartido por todas las organizaciones. No se pueden crear ni editar roles. Solo `super_admin` puede ajustar los permisos de un rol del sistema (`PUT /management/roles/{roleId}/permissions`), y el cambio queda auditado.
- **Asignación de rol de organización** (`membership_roles`): vincula una membresía con uno de esos tres roles (`PUT /management/organizations/{organizationId}/memberships/{membershipId}/roles`). No existe asignación directa de roles a un usuario fuera de una membresía.
- **Asignación de privilegio de plataforma** (`platform_role_assignments`): vincula un usuario con `super_admin`. El primero se crea por bootstrap; después, con la Platform API (`/platform/super-admins`). No pasa por membresías ni por la tabla `roles`, por lo que es imposible asignar `super_admin` como rol de organización ([super-admin-bootstrap.md](../operations/super-admin-bootstrap.md)).

## Reglas

- El catálogo de roles es fijo en v1; los permisos granulares de cada rol los administra la plataforma.
- `super_admin` no puede asignarse como rol de organización. El intento se rechaza con `422 ROLE_NOT_ASSIGNABLE`. Su concesión y revocación se auditan.
- El usuario debe seleccionar una organización activa cuando pertenece a varias; el backend valida membresía activa.
- Las API Keys no heredan permisos humanos; sus scopes son propios y limitados a una aplicación. Los permisos de la aplicación (`setApplicationPermissions`) son el **techo** de esos scopes: una API Key no puede tener un scope fuera de ellos (`422 API_KEY_SCOPE_NOT_PERMITTED`), y sus scopes efectivos son la intersección (TP-48).
- **Reenvío manual (TP-21):** solo lo solicita una aplicación por la Public API (`POST /public/notifications/{notificationId}/retry`) con el scope `notifications.retry`. No existe una operación de reenvío para usuarios del panel en v1. La auditoría registra `actor_type = APPLICATION` y la API Key de origen.
- **Cancelación (TP-36):** la Public API (`POST /public/notifications/{notificationId}/cancel`, scope `notifications.cancel`) y la Management API (`POST /management/notifications/{notificationId}/cancel`, JWT) invocan el **mismo caso de uso**. En Management se exige membresía activa en la organización seleccionada y el permiso de rol `notifications.cancel`: lo tiene `organization_admin`; `operator` solo si se le concede; `viewer` nunca. `super_admin` puede cancelar en cualquier organización, siempre con auditoría. El actor auditado es `USER` en Management y `APPLICATION` en la Public API. Solo se cancelan entregas que aún no salieron hacia el proveedor ([ADR-0012](../adr/0012-delivery-semantics.md)).
- **Último administrador de organización (TP-35):** una organización no puede quedarse sin al menos una membresía activa, de un usuario activo, con el rol `organization_admin`. Se rechaza con `422 LAST_ORGANIZATION_ADMIN_REQUIRED` cualquier operación que la dejaría sin él: suspender o desactivar la membresía (`updateMembershipStatus`), retirar o sustituir sus roles (`setMembershipRoles`) o suspender o desactivar al usuario (`updateUserStatus`). La regla vale también para `super_admin`. La validación es transaccional y con bloqueo, y el intento y su resultado se auditan. La recuperación de una organización sin administrador la hace `super_admin` con el procedimiento excepcional de [ADR-0011](../adr/0011-admin-identity.md#protección-del-último-organization_admin-tp-35-aprobada-2026-11-15). Esta regla es independiente de la protección del último `super_admin` (TP-15).
- Acciones sensibles (secretos, API Keys, QR, desconexión, cambios de cuota, exportación) requieren permiso específico y auditoría.
- El frontend puede ocultar acciones no autorizadas, pero la API siempre aplica autorización.
