# Contrato de autenticación del panel — `notification-admin`

**Para:** equipo del frontend `notification-admin` (repositorio independiente).
**Estado:** contrato aprobado (TP-37, TP-38; 2026-11-15). **Ni el backend ni el frontend lo implementan todavía.**

**Fuente normativa:** [openapi.yaml](openapi.yaml) (operaciones `login`, `refreshSession` y `logout`; esquemas `LoginResponse`, `RefreshCookie` y `CsrfHeader`). La justificación está en [ADR-0011](../adr/0011-admin-identity.md#token-csrf-sincronizado-con-la-sesión-tp-38-aprobada-2026-11-15). Si este documento difiere de OpenAPI, prevalece OpenAPI.

## Resumen

| Elemento | Dónde viaja | Quién lo guarda |
|---|---|---|
| Access token (JWT, ~15 min) | Cuerpo de login/refresh (`accessToken`); se envía en `Authorization: Bearer` | El panel, en memoria |
| Refresh token | Solo en la cookie `refresh` (`HttpOnly`, `Secure` en producción, `SameSite=Strict`, `Path=/api/v1/management/auth`) | El navegador. El panel **no** puede leerlo |

La sesión del panel dura como máximo **60 minutos desde el login** (TP-57): al vencer, refresh y `resumeSession` responden `401 AUTHENTICATION_REQUIRED` y el panel vuelve al login, aunque el usuario esté activo. Las rotaciones no alargan ese plazo. El panel debe tratar ese `401` como "sesión expirada" y mostrar el login, sin reintentos.
| Token CSRF | Cuerpo de login/refresh (`csrfToken`); se envía en la cabecera `X-CSRF-Token` | El panel, **solo en memoria** |

El token CSRF está **sincronizado con la sesión**: el backend guarda su hash asociado a la sesión. No hay cookie CSRF legible ni doble envío.

## Reglas para el frontend

1. **Login** (`POST /api/v1/management/auth/login`): enviar las credenciales. Del cuerpo de la respuesta, guardar en memoria `accessToken` y `csrfToken`. El refresh token llega en `Set-Cookie` y el navegador lo gestiona.
2. **Llamadas de negocio** (Management y Platform API): `Authorization: Bearer <accessToken>`. **No** necesitan `X-CSRF-Token`, porque no se autentican por cookie.
3. **Refresh** (`POST /api/v1/management/auth/refresh`): enviar la petición con credenciales (para que el navegador adjunte la cookie) y la cabecera `X-CSRF-Token: <csrfToken vigente>`. Si la respuesta es correcta, **sustituir** en memoria `accessToken` y `csrfToken` por los nuevos. El token CSRF anterior deja de ser válido.
4. **Logout** (`POST /api/v1/management/auth/logout`): enviar con credenciales y `X-CSRF-Token: <csrfToken vigente>`. Después, descartar de memoria `accessToken` y `csrfToken`. El backend elimina la cookie.
5. **Almacenamiento:** `csrfToken` y `accessToken` nunca se guardan en `localStorage`, `sessionStorage`, cookies, IndexedDB ni URLs, y nunca se registran en logs ni en herramientas de telemetría.
6. **Errores:**

   | Respuesta | Significado para el panel | Acción |
   |---|---|---|
   | `401 AUTHENTICATION_REQUIRED` en refresh/logout | La sesión no es válida (cookie ausente, expirada, revocada o reutilizada) | Descartar los tokens y llevar al login |
   | `403 PERMISSION_DENIED` en refresh/logout | Fallo CSRF: token ausente o que no coincide, u `Origin` no permitido | **No** reintentar la misma solicitud con el mismo token. Descartar los tokens y llevar al login |
   | `401` en una llamada de negocio | El access token caducó | Como máximo **un** refresh y un único reintento de esa llamada. Si el refresh falla, ir al login |

   El panel no debe reintentar en bucle ninguna solicitud rechazada por CSRF o autenticación. El `detail` de estos errores es genérico a propósito; el panel decide solo según `status` y `code`.
7. **Origen:** el panel debe servirse desde el origen configurado en `NOTIFICATIONS_ADMIN_HOST` (PD-06). El backend **exige** `Origin` en login, refresh y logout, y rechaza la petición si falta o es otro (TP-40). Los navegadores lo envían automáticamente en estas peticiones `POST`.
8. **Concurrencia:** no lanzar dos refresh en paralelo. Si un refresh rota el token, el otro llegaría con un refresh token ya usado, lo que provoca la revocación de la familia. El panel debe serializar el refresh (un único refresh en curso que compartan las llamadas que esperan).
9. **Contraseña temporal (TP-46):** si `user.mustChangePassword` es `true` tras el login, el panel debe llevar al usuario a cambiarla (`POST /api/v1/management/auth/password` con `currentPassword` y `newPassword`, `Authorization: Bearer`) antes de cualquier otra pantalla. Hasta entonces, el resto de la API responde `403 PASSWORD_CHANGE_REQUIRED`; solo se admiten `getCurrentUser`, `changePassword`, `refreshSession`, `resumeSession` y `logout`. Un `422 PASSWORD_CHANGE_REJECTED` no indica si falló la contraseña actual o la política.

## Ejemplo de respuesta de login o refresh

```json
{
  "csrfToken": "<valor opaco>",
  "accessToken": "<JWT>",
  "tokenType": "Bearer",
  "expiresIn": 900,
  "user": { "...": "CurrentUser" }
}
```

`csrfToken` es obligatorio en el esquema `LoginResponse`. El formato y la longitud del valor no forman parte del contrato: el panel lo trata como cadena opaca.

## Aviso de cierre de sesión (TP-57, TP-58)

La especificación de pantalla (diálogo, botones **Continuar trabajando** y **Cerrar sesión**, textos y accesibilidad) está en el repositorio del panel: `notification-admin/docs/security/session-management.md`. Aquí se fija solo el contrato.

La sesión dura como máximo 60 minutos desde el login. Login, refresh y `resumeSession` devuelven `sessionExpiresAt` (UTC) y `sessionExpiresIn` (segundos que faltan).

1. **Cuenta atrás:** al recibir cualquiera de esas respuestas, el panel programa la cuenta atrás con `sessionExpiresIn`. No usa el reloj del equipo, que puede estar desfasado.
2. **Aviso a los 5 minutos del vencimiento:** un diálogo accesible (con foco y anunciado por lectores de pantalla) muestra "Tu sesión se cerrará en m:ss" y ofrece dos acciones:
   - **Continuar trabajando:** pide la contraseña, con el email del usuario ya fijado, y llama a `POST /api/v1/management/auth/login` con credenciales. Con `200`, sustituye en memoria `accessToken`, `csrfToken` y la cuenta atrás, cierra el aviso y conserva el estado de la página. El backend revoca la sesión anterior. Si responde `401`, el diálogo muestra el error y permite reintentar.
   - **Cerrar sesión:** llama a `logout`.
3. **Al llegar a cero:** descarta los tokens y muestra el login. Si el usuario tenía un formulario sin guardar, el panel lo advierte antes, en el aviso ("Guarda tus cambios"). No guarda borradores en el almacenamiento del navegador.
4. **Varias pestañas:** el aviso, la reautenticación y el cierre se sincronizan por `BroadcastChannel`. Si una pestaña se reautentica, las demás reciben los tokens nuevos y cierran su aviso.
5. **Sin renovación silenciosa:** el panel no debe intentar alargar la sesión sin contraseña; refresh y `resumeSession` no cambian el vencimiento.

## Recarga de página y varias pestañas (TP-41)

1. **Al cargar el panel** (recarga o pestaña nueva) no hay tokens en memoria. Si la pestaña no recibe tokens de otra pestaña abierta, llamar a `POST /api/v1/management/auth/session` (`resumeSession`) con credenciales y **sin** `X-CSRF-Token`. Con `200`, guardar en memoria `accessToken` y `csrfToken`; con `401`, ir al login.
2. **Serializar la rotación entre pestañas:** `resumeSession`, `refreshSession` y `logout` rotan o revocan la sesión. Si dos pestañas los lanzan a la vez, la segunda presenta un refresh token ya usado y el backend revoca toda la sesión por reutilización. El panel debe ejecutarlos bajo un bloqueo compartido entre pestañas (Web Locks API, `navigator.locks.request`).
3. **Compartir los tokens nuevos** con las demás pestañas por `BroadcastChannel`, solo en memoria; nunca por `localStorage`, `sessionStorage` ni cookies. Una pestaña que reciba tokens nuevos sustituye los suyos.
4. **Logout:** avisar a las demás pestañas por el mismo canal para que descarten sus tokens.

## Contraseña olvidada (TP-55)

No hay autoservicio en v1. El panel muestra "contacte con su administrador". El administrador usa `POST /api/v1/management/users/{userId}/temporary-password` y el usuario entra con la contraseña temporal; se aplica la regla 9. La política de contraseñas es de 12 a 128 caracteres, sin reglas de composición, rechazando las comunes o filtradas. El panel puede validar la longitud, pero la decisión final es del backend.

Registro: [technical-pending-register.md](../operations/technical-pending-register.md).
