# ADR-0011: Identidad administrativa mediante JWT propio

- Estado: Aceptado (roles de v1 actualizados el 2026-11-15: PD-01, TP-15, TP-20)
- Decisión: El panel administrativo autentica usuarios mediante un mecanismo JWT gestionado por notification-service. No se adopta OIDC como requisito de la primera versión.

## Contexto

El panel es un cliente independiente. Las aplicaciones consumidoras no son usuarios administrativos y no deben reutilizar el mecanismo de autenticación del panel.

## Decisión

- Administradores: access token JWT firmado por el backend y refresh token opaco, rotativo y revocable.
- Aplicaciones consumidoras: API Key por aplicación, almacenada como hash y mostrada únicamente al crearla o rotarla.
- Comunicación entre componentes internos: identidad de servicio y secretos de entorno/secret manager; no reutilizar API Keys públicas.
- Webhooks: autenticación específica por proveedor (firma HMAC, token o mecanismo documentado por el proveedor).
- Proveedores salientes: credenciales aisladas por cuenta de proveedor y cifradas en reposo.

## Parámetros iniciales recomendados

- Access token: 15 minutos.
- Refresh token: 7 días, rotación en cada uso y detección de reutilización.
- Contraseñas: Argon2id.
- Autorización: RBAC y comprobación obligatoria de `organization_id` y `application_id`.
- Cookies del refresh token: `HttpOnly`, `Secure`, `SameSite` apropiado; protección CSRF si el flujo usa cookies.
- No guardar refresh tokens ni secretos persistentes en `localStorage`.
- MFA: previsto para una iteración posterior; dejar el modelo preparado.

## Roles de v1 (vigente desde 2026-11-15, PD-01 aprobada)

| Rol                  | Tipo                         | Asignación                                                                                                |
| -------------------- | ---------------------------- | --------------------------------------------------------------------------------------------------------- |
| `super_admin`        | Privilegio de **plataforma** | `platform_role_assignments`. Independiente de las membresías; nunca se asigna mediante `membership_roles` |
| `organization_admin` | Rol de organización          | Dentro de una membresía (`membership_roles`)                                                              |
| `operator`           | Rol de organización          | Dentro de una membresía                                                                                   |
| `viewer`             | Rol de organización          | Dentro de una membresía                                                                                   |

Reglas:

- Un usuario puede pertenecer a varias organizaciones y tener roles distintos en cada membresía.
- Las API Keys de aplicaciones no heredan roles humanos: tienen scopes propios limitados a su aplicación.
- La autorización sigue evaluándose por organización y aplicación en cada operación. `super_admin` opera globalmente y siempre con auditoría.
- La concesión y la revocación de `super_admin` se auditan.
- **Catálogo cerrado (TP-20, 2026-11-15):** en v1 no existen roles personalizados por organización. `organization_admin`, `operator` y `viewer` son roles del sistema compartidos por todas las organizaciones, y la plataforma administra el catálogo y sus permisos. Solo un `super_admin` puede modificar los permisos de un rol del sistema (`PUT /management/roles/{roleId}/permissions`, auditado). No hay operaciones para crear ni editar roles.
- Detalle de capacidades por rol: [rbac-matrix.md](../security/rbac-matrix.md). Modelo de datos: [erd.md](../data/erd.md).

## Administración de `super_admin` (TP-15, aprobada 2026-11-15)

- **Bootstrap inicial:** el primer `super_admin` se crea con una tarea administrativa manual, restringida e idempotente, nunca expuesta por HTTP. La asignación queda con `granted_by_actor_type = SYSTEM_BOOTSTRAP` y `granted_by = NULL`, de modo que no depende de un `super_admin` previo y sigue siendo trazable. La recuperación de emergencia usa la misma tarea con aprobación doble. Procedimiento: [super-admin-bootstrap.md](../operations/super-admin-bootstrap.md).
- **Platform API:** después del bootstrap, la concesión y la revocación se hacen con `GET/POST /platform/super-admins` y `POST /platform/super-admins/{assignmentId}/revoke`. Solo un `super_admin` activo puede usarlas; exigen motivo y quedan auditadas. No usan membresías.
- **Protección:** no se puede revocar la última asignación activa (`422 LAST_SUPER_ADMIN_REVOCATION_FORBIDDEN`). Tampoco se admiten asignaciones activas duplicadas (`409 RESOURCE_ALREADY_EXISTS`).

### Historial

La versión original de esta ADR sugería: `super_admin`, `organization_admin`, `application_admin`, `operator`, `viewer`, como "plantillas iniciales". La matriz RBAC usaba `org_admin` para el mismo concepto. Con PD-01 ([PENDING-DECISIONS.md](../project/PENDING-DECISIONS.md#pd-01-nomenclatura-y-alcance-de-roles)):

- `org_admin` se sustituye por el nombre canónico `organization_admin`; no se conserva como alias.
- `application_admin` **no** forma parte de v1. Requeriría roles con ámbito de aplicación, que el modelo de v1 no tiene.
- `super_admin` pasa de "rol" a privilegio de plataforma con almacenamiento propio.

Con TP-20 (2026-11-15) se retira la posibilidad, heredada de la versión anterior del modelo, de crear **roles personalizados por organización** (`roles.organization_id`, operaciones `createRole`/`updateRole`). La frase original "Los roles son plantillas iniciales" queda sustituida por el catálogo cerrado descrito arriba.

## Consecuencias

- Se implementan endpoints de login, refresh, logout, revocación de sesiones, gestión de usuarios, asignación de roles **por membresía** y administración de `super_admin` por la Platform API.
- La API pública usa `X-API-Key`; el panel usa `Authorization: Bearer <access_token>`.
- JWT no reemplaza autorización: cada request debe validar firma, emisor, audiencia, expiración y permisos/ámbito.
- Rotación de claves de firma debe estar soportada mediante `kid` y un período de transición.
- Asignar `super_admin` mediante `PUT /management/organizations/{organizationId}/memberships/{membershipId}/roles` se rechaza con `422 ROLE_NOT_ASSIGNABLE`.
