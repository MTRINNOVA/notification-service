# Contrato de autenticación del panel — `notification-admin`

**Para:** equipo del frontend `notification-admin` (repositorio independiente).
**Estado:** contrato aprobado (TP-37, TP-38; 2026-11-15). **Ni el backend ni el frontend lo implementan todavía.**

**Fuente normativa:** [openapi.yaml](openapi.yaml) (operaciones `login`, `refreshSession` y `logout`; esquemas `LoginResponse`, `RefreshCookie` y `CsrfHeader`). La justificación está en [ADR-0011](../adr/0011-admin-identity.md#token-csrf-sincronizado-con-la-sesión-tp-38-aprobada-2026-11-15). Si este documento difiere de OpenAPI, prevalece OpenAPI.

## Resumen

| Elemento | Dónde viaja | Quién lo guarda |
|---|---|---|
| Access token (JWT, ~15 min) | Cuerpo de login/refresh (`accessToken`); se envía en `Authorization: Bearer` | El panel, en memoria |
| Refresh token | Solo en la cookie `refresh` (`HttpOnly`, `Secure` en producción, `SameSite=Strict`, `Path=/api/v1/management/auth`) | El navegador. El panel **no** puede leerlo |
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
9. **Contraseña temporal (TP-46):** si `user.mustChangePassword` es `true` tras el login, el panel debe llevar al usuario a cambiarla (`POST /api/v1/management/auth/password` con `currentPassword` y `newPassword`, `Authorization: Bearer`) antes de cualquier otra pantalla. Hasta entonces, el resto de la API responde `403 PASSWORD_CHANGE_REQUIRED`. Un `422 PASSWORD_CHANGE_REJECTED` no indica si falló la contraseña actual o la política.

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

## Puntos abiertos que afectan al frontend

- **TP-41: recarga de página.** El token CSRF solo vive en memoria y el refresh lo exige. Por tanto, tras recargar la página o abrir una pestaña nueva, el panel no tiene `csrfToken` y no puede hacer refresh, lo que obliga a iniciar sesión de nuevo. Mientras no se decida otra cosa, ese es el comportamiento esperado.

Registro: [technical-pending-register.md](../operations/technical-pending-register.md).
