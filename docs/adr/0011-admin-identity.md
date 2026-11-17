# ADR-0011: Identidad administrativa mediante JWT propio

- Estado: Aceptado (roles de v1 actualizados el 2026-11-15: PD-01, TP-15, TP-20; ampliado con TP-35, TP-37, TP-38, TP-40 y TP-46)
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
- Cookies del refresh token: `HttpOnly`, `Secure`, `SameSite=Strict` y protección CSRF obligatoria (detalle en «Sesión del panel: cookie de refresh y CSRF»).
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

## Protección del último `organization_admin` (TP-35, aprobada 2026-11-15)

- **Invariante:** una organización con administrador no puede quedarse sin al menos una membresía **activa**, de un usuario **activo**, que tenga el rol `organization_admin`.
- **Operaciones afectadas:** suspender o desactivar una membresía (`updateMembershipStatus`); retirar `organization_admin` o sustituir los roles de una membresía (`setMembershipRoles`); suspender o desactivar al usuario globalmente (`updateUserStatus`), que inhabilita todas sus membresías; y cualquier operación futura que pueda reducir a cero los administradores activos.
- **Validación transaccional:** dentro de la misma transacción que aplica el cambio se bloquean (`SELECT … FOR UPDATE`, o bloqueo equivalente por organización) las membresías activas con `organization_admin` de la organización afectada. Después se cuenta cuántas quedarían tras el cambio. Así, dos solicitudes concurrentes que retiran a dos administradores distintos se serializan y la segunda ve el estado real.
- **Rechazo:** si quedarían cero, la operación se rechaza sin cambios con `422 LAST_ORGANIZATION_ADMIN_REQUIRED` ([error-codes.md](../api/error-codes.md)). La regla se aplica también cuando quien actúa es `super_admin`.
- **Auditoría:** se registran el intento y su resultado (aceptado o rechazado), con actor, organización, membresía y motivo del rechazo.
- **Recuperación excepcional:** corresponde solo a `super_admin`, con auditoría explícita y motivo obligatorio. Primero asigna `organization_admin` a otra membresía, existente o nueva (`createMembership` o `setMembershipRoles`), y después retira o suspende al administrador anterior si procede. Por ejemplo, cuando el único administrador ya no es accesible. Se usa también para el primer administrador de una organización recién creada. Nunca se hace modificando la base de datos a mano.
- **Independencia de TP-15:** esta regla protege a las organizaciones. La protección del último `super_admin` (`422 LAST_SUPER_ADMIN_REVOCATION_FORBIDDEN`) protege a la plataforma y sigue siendo independiente.

## Sesión del panel: cookie de refresh y CSRF (TP-37, aprobada 2026-11-15)

Declarada en [openapi.yaml](../api/openapi.yaml) con los esquemas `RefreshCookie` (`apiKey`, `in: cookie`, `name: refresh`) y `CsrfHeader` (`apiKey`, `in: header`, `name: X-CSRF-Token`). Ambos se exigen **a la vez** solo en `POST /management/auth/refresh` y `POST /management/auth/logout`; no hay autenticación global. El resto de la Management API usa `AdminBearer`.

| Propiedad | Regla |
|---|---|
| Cookie | Nombre `refresh`; contiene solo el refresh token opaco; `HttpOnly` |
| `Secure` | Obligatorio en producción y en cualquier entorno expuesto. Solo puede omitirse en desarrollo local sobre `http://localhost` |
| `SameSite` | `Strict`. Exige que el panel y la API compartan sitio registrable. Si la infraestructura definitiva (TP-27) no lo permite, el cambio de política requiere aprobación previa y mantener CSRF |
| Ámbito | `Path=/api/v1/management/auth`, de modo que el navegador solo la envía a refresh y logout; sin `Domain` salvo necesidad justificada |
| Expiración | `Max-Age` igual a la vigencia restante del refresh token (7 días); se reemite en cada rotación |
| Eliminación | En logout y al detectar reutilización: `Max-Age=0` con la misma ruta y atributos |
| CSRF | `X-CSRF-Token` obligatorio en refresh y logout, con el token sincronizado con la sesión de TP-38: se obtiene la sesión a partir de la cookie y se compara el token con el asociado a ella. Ausente o que no coincide → `403 PERMISSION_DENIED` |
| Rotación | Cada refresh consume el token (un solo uso), emite uno nuevo de la misma familia y reemite la cookie |
| Reutilización | Presentar un token ya usado revoca toda la familia, elimina la cookie, genera auditoría y responde `401 AUTHENTICATION_REQUIRED` |

El nombre `X-CSRF-Token` no existía antes en la documentación (solo se exigía "protección CSRF"), así que no hay discrepancia con un nombre anterior. El código todavía no implementa la autenticación del panel. El mecanismo del token se define en la sección siguiente (TP-38).

## Token CSRF sincronizado con la sesión (TP-38, aprobada 2026-11-15)

**Mecanismo:** token CSRF *sincronizado con la sesión* (synchronizer token). **No** se usa el doble envío con cookie legible. La **sesión** es la familia de refresh tokens (`refresh_tokens.family_id`) que crea el login. Cada rotación produce una sesión resultante de la misma familia, con un token CSRF nuevo.

### Flujos

**Login** (`POST /management/auth/login`):

1. El usuario envía sus credenciales.
2. El backend las valida.
3. Crea la sesión.
4. Genera el refresh token y el token CSRF.
5. Entrega el refresh token solo en la cookie `refresh` (`HttpOnly`).
6. Devuelve el token CSRF en el cuerpo JSON (`csrfToken`).
7. El panel conserva el token CSRF en memoria.

**Solicitudes protegidas contra CSRF** (las autenticadas por cookie: hoy `refresh` y `logout`):

1. El navegador envía la cookie `refresh`.
2. El panel añade `X-CSRF-Token`.
3. El backend valida `Origin` y obtiene la sesión a partir de la cookie.
4. Compara el token recibido con el token CSRF asociado a esa sesión.
5. Si falta o no coincide, rechaza la solicitud.

Las operaciones autenticadas con `Authorization: Bearer` no usan la cookie, y por eso no requieren `X-CSRF-Token`.

**Refresh** (`POST /management/auth/refresh`):

1. El panel envía la cookie y el token CSRF vigente.
2. El backend valida ambos.
3. Rota el refresh token.
4. Genera un token CSRF nuevo asociado a la sesión resultante.
5. Lo devuelve en `csrfToken`.
6. El panel sustituye el anterior en memoria.

**Logout** (`POST /management/auth/logout`):

1. El panel envía `X-CSRF-Token`, y el navegador la cookie.
2. El backend valida la sesión y el token CSRF.
3. Revoca la sesión.
4. Invalida el refresh token y el token CSRF.
5. Elimina la cookie (`Max-Age=0`).

### Reglas de seguridad

- El token CSRF se genera con un generador criptográficamente seguro y con entropía suficiente.
- El backend guarda solo el **hash** del token, nunca su valor en claro, vinculado a una sesión concreta.
- Toda rotación de sesión produce un token CSRF nuevo. Tras una rotación correcta, el token anterior deja de ser válido, y también al hacer logout, al revocar la sesión o al detectar la reutilización del refresh token.
- El panel conserva el token **solo en memoria**: nunca en `localStorage`, `sessionStorage` ni cookies. El token nunca se incluye en URLs, logs ni mensajes de error.
- Toda operación autenticada por cookie aplica esta protección.
- En login, refresh y logout el encabezado `Origin` es **obligatorio** y se valida contra el origen del panel configurado (`NOTIFICATIONS_ADMIN_HOST`, PD-06). Si falta o no está permitido, se rechaza como fallo CSRF (TP-40).
- La cookie `refresh` mantiene `HttpOnly`, `Secure` en producción y `SameSite=Strict` (tabla anterior).
- **Errores:** sesión o cookie ausente, expirada, revocada o reutilizada → `401 AUTHENTICATION_REQUIRED`; token CSRF ausente o que no coincide, u `Origin` no permitido → `403 PERMISSION_DENIED` ([error-codes.md](../api/error-codes.md)). El `detail` es genérico y no indica qué comprobación concreta falló.

**Parámetros (TP-40, aprobada 2026-11-15):**

| Parámetro | Valor |
|---|---|
| Generación | 32 bytes (256 bits) de `RandomNumberGenerator`, codificados como texto opaco |
| Almacenamiento | Solo SHA-256, en `refresh_tokens.csrf_token_hash` de la fila activa de la sesión ([erd.md](../data/erd.md)). SHA-256 basta porque el token es aleatorio y de alta entropía; no se necesita un hash lento |
| Comparación | En tiempo constante |
| Refresh token | Mismo criterio: 32 bytes aleatorios guardados como SHA-256 (`refresh_tokens.token_hash`) |
| `Origin` | Obligatorio en login, refresh y logout |

El contrato que consume el panel está en [admin-panel-auth-contract.md](../api/admin-panel-auth-contract.md).

## Alta de usuarios con contraseña temporal (TP-46, aprobada 2026-11-15)

- `createUser` (`super_admin` o permiso `members.manage`) crea el usuario global con una **contraseña temporal** fijada por el administrador. Se guarda como hash Argon2id, nunca se devuelve ni se registra, y el usuario queda con `must_change_password = true`. El bootstrap de `super_admin` usa el mismo mecanismo.
- Mientras `must_change_password` sea `true`, el usuario puede iniciar sesión, pero solo puede usar `getCurrentUser`, `changePassword`, `refreshSession` y `logout`. El resto responde `403 PASSWORD_CHANGE_REQUIRED`. `CurrentUser.mustChangePassword` se lo indica al panel.
- `changePassword` (`POST /management/auth/password`) exige la contraseña actual. Guarda el nuevo hash, pone `must_change_password = false` y revoca las demás sesiones del usuario. Si la contraseña actual es incorrecta o la nueva no cumple la política → `422 PASSWORD_CHANGE_REJECTED`, sin indicar cuál de las dos.
- `createMembership` solo vincula usuarios **existentes** por email. Si no existe → `422 MEMBERSHIP_USER_NOT_FOUND`; la membresía nunca crea usuarios.
- **Pendiente** (no bloquea la Fase 2): política de contraseñas (longitud y complejidad) y restablecimiento de contraseña olvidada (TP-55).

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
- Las operaciones de membresías y de estado de usuario implementan la protección del último `organization_admin` (TP-35) con bloqueo transaccional y auditoría.
- Refresh y logout se autentican con la cookie `refresh` más la cabecera `X-CSRF-Token` (TP-37), con un token CSRF sincronizado con la sesión, devuelto en `csrfToken` por login y refresh (TP-38).
