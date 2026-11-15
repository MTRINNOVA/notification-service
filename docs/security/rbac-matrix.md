# Matriz RBAC — v1.5

Roles de v1 según [ADR-0011](../adr/0011-admin-identity.md) (PD-01, aprobada el 2026-11-15): `super_admin` (privilegio de plataforma), `organization_admin`, `operator` y `viewer` (roles de membresía). `org_admin` deja de ser un nombre vigente, `application_admin` no existe en v1 y **no hay roles personalizados** (TP-20): la plataforma administra el catálogo de roles y sus permisos.

La autorización se evalúa por usuario + membresía activa + organización seleccionada. Los permisos de plataforma son independientes.

| Capacidad                               |               `super_admin` | `organization_admin` |             `operator` |         `viewer` |           API Key de aplicación |
| --------------------------------------- | --------------------------: | -------------------: | ---------------------: | ---------------: | ------------------------------: |
| Crear/suspender organizaciones          |                          Sí |                   No |                     No |               No |                              No |
| Administrar miembros de su organización |           Auditoría/soporte |                   Sí |                     No |               No |                              No |
| Administrar aplicaciones                |                      Global |                   Sí |                     No |               No |                              No |
| Crear/rotar API Keys                    |                      Global |                   Sí |                     No |               No |                              No |
| Configurar proveedores y credenciales   |                      Global |                   Sí |                     No |               No |                              No |
| Conectar/desconectar WhatsApp           |                      Global |                   Sí |          Según permiso |               No |                              No |
| Asignar remitentes a aplicaciones       |                      Global |                   Sí |          Según permiso |               No |                              No |
| Crear/editar plantillas                 |                      Global |                   Sí |      Sí, si se concede |               No |                              No |
| Enviar notificaciones                   |              No por defecto |        Según permiso |          Según permiso |               No | Sí, solo su aplicación y scopes |
| Consultar notificaciones                |        Global con auditoría |                   Sí | Sí, dentro de permisos | Sí, solo lectura |          Sí, solo su aplicación |
| Cancelar/reintentar                     |        Global con auditoría |                   Sí |          Según permiso |               No |                     Según scope |
| Administrar campañas                    |                      Global |                   Sí |          Según permiso |          Lectura |           Según scope explícito |
| Dashboard y consumo                     |                      Global |                   Sí |          Según permiso |      Sí, lectura |                              No |
| Consultar auditoría                     |                      Global |  Sí, su organización |          Según permiso |    Según permiso |                              No |
| Conceder/revocar `super_admin`          | Sí (nunca el último activo) |                   No |                     No |               No |                              No |
| Ajustar permisos de roles del sistema   |                Sí, auditado |                   No |                     No |               No |                              No |

## Definición de rol frente a asignación de privilegio

- **Definición de rol** (`roles` + `role_permissions`): catálogo **cerrado** del sistema con `organization_admin`, `operator` y `viewer`, compartido por todas las organizaciones. No se pueden crear ni editar roles. Solo `super_admin` puede ajustar los permisos de un rol del sistema (`PUT /management/roles/{roleId}/permissions`), y el cambio queda auditado.
- **Asignación de rol de organización** (`membership_roles`): vincula una membresía con uno de esos tres roles (`PUT /management/organizations/{organizationId}/memberships/{membershipId}/roles`). No existe asignación directa de roles a un usuario fuera de una membresía.
- **Asignación de privilegio de plataforma** (`platform_role_assignments`): vincula un usuario con `super_admin`. El primero se crea por bootstrap; después, con la Platform API (`/platform/super-admins`). No pasa por membresías ni por la tabla `roles`, por lo que es imposible asignar `super_admin` como rol de organización ([super-admin-bootstrap.md](../operations/super-admin-bootstrap.md)).

## Reglas

- El catálogo de roles es fijo en v1; los permisos granulares de cada rol los administra la plataforma.
- `super_admin` no puede asignarse como rol de organización. El intento se rechaza con `422 ROLE_NOT_ASSIGNABLE`. Su concesión y revocación se auditan.
- El usuario debe seleccionar una organización activa cuando pertenece a varias; el backend valida membresía activa.
- Las API Keys no heredan permisos humanos; sus scopes son propios y limitados a una aplicación.
- Acciones sensibles (secretos, API Keys, QR, desconexión, cambios de cuota, exportación) requieren permiso específico y auditoría.
- El frontend puede ocultar acciones no autorizadas, pero la API siempre aplica autorización.
