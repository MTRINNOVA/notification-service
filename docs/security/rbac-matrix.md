# Matriz RBAC — v1.5

> **Pendiente de aprobación ([PD-01](../project/PENDING-DECISIONS.md#pd-01-nomenclatura-y-alcance-de-roles)):** esta matriz usa `org_admin` y omite `application_admin`; ADR-0011 usa `organization_admin` e incluye `application_admin`. Ninguna nomenclatura es definitiva hasta resolver PD-01. El almacenamiento de `super_admin` tampoco está definido en el ERD.

La autorización se evalúa por usuario + membresía activa + organización seleccionada. Los permisos de plataforma son independientes.

| Capacidad | `super_admin` | `org_admin` | `operator` | `viewer` | API Key de aplicación |
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

## Reglas

- Los roles base son sugeridos; se pueden configurar permisos granulares.
- `super_admin` no debe ser asignado como rol ordinario de organización.
- El usuario debe seleccionar una organización activa cuando pertenece a varias; el backend valida membresía activa.
- Las API Keys no heredan permisos humanos; sus scopes son propios y limitados a una aplicación.
- Acciones sensibles (secretos, API Keys, QR, desconexión, cambios de cuota, exportación) requieren permiso específico y auditoría.
- El frontend puede ocultar acciones no autorizadas, pero la API siempre aplica autorización.
