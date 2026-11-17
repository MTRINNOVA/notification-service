# Procedimiento: bootstrap y recuperación de `super_admin`

**Estado:** decisión aprobada (TP-15, 2026-11-15); **implementación pendiente** (Fase de identidad). Modelo: `platform_role_assignments` en [erd.md](../data/erd.md). Normativa: [ADR-0011](../adr/0011-admin-identity.md).

## 1. Mecanismos

| Mecanismo | Cuándo | Actor registrado | Canal |
|---|---|---|---|
| **Bootstrap inicial** | No existe ningún `super_admin` activo | `SYSTEM_BOOTSTRAP` (`granted_by = NULL`) | Tarea administrativa manual de ejecución única; **nunca** un endpoint HTTP |
| **Platform API** | Concesiones y revocaciones ordinarias, después del bootstrap | Usuario `super_admin` activo (`granted_by_actor_type = USER`) | `POST /platform/super-admins`, `POST /platform/super-admins/{assignmentId}/revoke` |
| **Recuperación de emergencia** | No queda ningún `super_admin` operativo (cuentas perdidas o bloqueadas) | `SYSTEM_BOOTSTRAP`, motivo `EMERGENCY_RECOVERY` | La misma tarea de bootstrap, en modo emergencia |

`organization_memberships` y `membership_roles` **no** intervienen en ningún mecanismo.

## 2. Bootstrap inicial

**Requisitos de ejecución**

- Ejecución manual por personal de operación autorizado, desde un entorno con acceso restringido a la base de datos de producción. Nunca se ejecuta automáticamente al arrancar los hosts.
- Entradas: email del usuario y motivo. Los secretos (credencial inicial) no se pasan por línea de comandos ni quedan en logs.

**Pasos**

1. Verificar en una transacción, con bloqueo de las asignaciones de `super_admin`, que no existe ninguna asignación activa. Si existe, **terminar sin cambios**: el bootstrap no sustituye a la Platform API.
2. Crear o localizar el usuario por email normalizado. Si se crea, la credencial inicial es una contraseña temporal (TP-46): se entrega por un canal seguro y el usuario queda con `must_change_password = true`, por lo que debe cambiarla en su primer acceso ([ADR-0011](../adr/0011-admin-identity.md#alta-de-usuarios-con-contraseña-temporal-tp-46-aprobada-2026-11-15)).
3. Insertar `platform_role_assignments` con `role_code = 'super_admin'`, `granted_by_actor_type = 'SYSTEM_BOOTSTRAP'`, `granted_by = NULL` y `grant_reason`.
4. Registrar `audit_logs` con `actor_type = SYSTEM_BOOTSTRAP`, acción de concesión, recurso, resultado y correlation id.
5. Confirmar la transacción y comunicar el resultado sin mostrar secretos.

**Idempotencia**

- Ejecutar el bootstrap de nuevo para el mismo usuario con una asignación activa es un **no-op** auditado, sin error.
- El índice UNIQUE parcial (`user_id`,`role_code`) WHERE `revoked_at IS NULL` impide asignaciones activas duplicadas aunque haya ejecuciones concurrentes.

## 3. Operación ordinaria (Platform API)

- Solo un `super_admin` activo puede conceder o revocar. La autorización se evalúa con el JWT y una consulta de asignación activa, nunca con datos de membresía.
- Motivo obligatorio. Se auditan actor, motivo, fecha y correlation id.
- Errores ([error-codes.md](../api/error-codes.md)):
  - `409 RESOURCE_ALREADY_EXISTS` si el usuario ya tiene una asignación activa;
  - `409 RESOURCE_STATE_CONFLICT` si la asignación ya está revocada;
  - `422 LAST_SUPER_ADMIN_REVOCATION_FORBIDDEN` si es la última activa.
- **Protección del último `super_admin`:** la revocación cuenta las asignaciones activas en la misma transacción, con bloqueo, y se rechaza si quedaría cero.

## 4. Recuperación de emergencia

1. Requiere aprobación de dos personas responsables, registrada fuera del sistema (ticket o acta) e identificada en el motivo.
2. Se ejecuta la tarea de bootstrap en modo emergencia. Este modo **solo** está permitido cuando no hay ningún `super_admin` activo y operativo; si existen asignaciones activas inutilizables, primero se revocan por acceso administrativo a la base de datos, con la misma aprobación y auditoría.
3. La concesión se registra con `SYSTEM_BOOTSTRAP` y `grant_reason = 'EMERGENCY_RECOVERY: <referencia>'`.
4. Después: revisar la auditoría, rotar las credenciales afectadas y documentar el incidente.

## 5. Criterios de aceptación (para la implementación)

- Sin ningún `super_admin`, el bootstrap crea exactamente una asignación `SYSTEM_BOOTSTRAP` y su evento de auditoría.
- Repetir el bootstrap no crea duplicados. Con un `super_admin` activo, el bootstrap no cambia nada.
- La Platform API rechaza la revocación del último `super_admin` activo con `422`.
- Ningún endpoint de membresías ni de roles de organización puede conceder `super_admin`.
