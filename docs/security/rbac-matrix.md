# Matriz RBAC — v1.5

Roles de v1 según [ADR-0011](../adr/0011-admin-identity.md) (PD-01, aprobada el 2026-11-15): `super_admin` (privilegio de plataforma), `organization_admin`, `operator` y `viewer` (roles de membresía). `org_admin` deja de ser un nombre vigente y `application_admin` no existe en v1.

La autorización se evalúa por usuario + membresía activa + organización seleccionada. Los permisos de plataforma son independientes.

| Capacidad | `super_admin` | `organization_admin` | `operator` | `viewer` | API Key de aplicación |
|---|---:|---:|---:|---:|---:|
| Crear/suspender organizaciones | Sí | No | No | No | No |
| Administrar miembros de su organización | Auditoría/soporte | Sí | No | No | No |
| Administrar aplicaciones | Global | Sí | No | No | No |
| Crear/rotar API Keys | Global | Sí | No | No | No |
| Configurar proveedores y credenciales | Global | Sí | No | No | No |
| Conectar/desconectar WhatsApp | Global | Sí | Según permiso | No | No |
| Asignar remitentes a aplicaciones | Global | Sí | Según permiso | No | No |
| Crear/editar plantillas | Global | Sí | Sí, si se concede | No | No |
| Enviar notificaciones | No por defecto | Según permiso | Según permiso | No | Sí, solo su aplicación y scopes |
| Consultar notificaciones | Global con auditoría | Sí | Sí, dentro de permisos | Sí, solo lectura | Sí, solo su aplicación |
| Cancelar/reintentar | Global con auditoría | Sí | Según permiso | No | Según scope |
| Administrar campañas | Global | Sí | Según permiso | Lectura | Según scope explícito |
| Dashboard y consumo | Global | Sí | Según permiso | Sí, lectura | No |
| Consultar auditoría | Global | Sí, su organización | Según permiso | Según permiso | No |

## Definición de rol frente a asignación de privilegio

- **Definición de rol** (`roles` + `role_permissions`): catálogo de roles de **organización**. Los roles del sistema `organization_admin`, `operator` y `viewer` tienen `organization_id = NULL` e `is_system = true`, y sirven a todas las organizaciones. Los roles personalizados de una organización llevan su `organization_id`.
- **Asignación de rol de organización** (`membership_roles`): vincula una membresía con un rol del sistema o con un rol personalizado de **la misma** organización.
- **Asignación de privilegio de plataforma** (`platform_role_assignments`): vincula un usuario con `super_admin`. No pasa por membresías ni por la tabla `roles`, por lo que es imposible asignar `super_admin` como rol de organización.

## Reglas

- Los roles base son plantillas; se pueden configurar permisos granulares.
- `super_admin` no puede asignarse como rol de organización. El intento se rechaza con `422 ROLE_NOT_ASSIGNABLE`. Su concesión y revocación se auditan.
- El usuario debe seleccionar una organización activa cuando pertenece a varias; el backend valida membresía activa.
- Las API Keys no heredan permisos humanos; sus scopes son propios y limitados a una aplicación.
- Acciones sensibles (secretos, API Keys, QR, desconexión, cambios de cuota, exportación) requieren permiso específico y auditoría.
- El frontend puede ocultar acciones no autorizadas, pero la API siempre aplica autorización.
