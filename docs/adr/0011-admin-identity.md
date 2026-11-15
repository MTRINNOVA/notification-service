# ADR-0011: Identidad administrativa mediante JWT propio

- Estado: Aceptado
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

## Roles iniciales sugeridos
`super_admin`, `organization_admin`, `application_admin`, `operator`, `viewer`.
Los roles son plantillas iniciales; los permisos efectivos se aplican por recurso y ámbito de organización/aplicación.

## Consecuencias
- Se implementan endpoints de login, refresh, logout, revocación de sesiones y gestión de usuarios/roles.
- La API pública usa `X-API-Key`; el panel usa `Authorization: Bearer <access_token>`.
- JWT no reemplaza autorización: cada request debe validar firma, emisor, audiencia, expiración y permisos/ámbito.
- Rotación de claves de firma debe estar soportada mediante `kid` y un período de transición.
