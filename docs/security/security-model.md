# Modelo de seguridad

## Ámbitos de identidad

- `Organization`: límite empresarial y propietario de recursos.
- `Application`: sistema cliente dentro de una organización.
- `User`: identidad humana que accede al panel.
- `ProviderAccount`: cuenta de proveedor propiedad de una organización; su uso por aplicaciones se controla mediante asignaciones/permisos.

## Autenticación

- Aplicaciones: `X-API-Key`; guardar hash seguro, prefijo identificable, scopes, expiración, revocación y última utilización.
- Panel: JWT de corta duración, refresh controlado y RBAC.
- Webhooks: firma HMAC/proveedor o secreto de endpoint; validar timestamp/replay cuando aplique.
- Servicios internos: secretos de servicio/red privada; no reutilizar API keys de clientes.

## Autorización y aislamiento

Todas las consultas se filtran por contexto autenticado (`organization_id`, `application_id`). Nunca aceptar tenant scope del body como fuente de autoridad. Un super-admin puede cruzar organizaciones con auditoría explícita. RLS en PostgreSQL es defensa adicional, no sustituto de autorización de aplicación.

## Secretos

- No almacenar tokens en claro.
- Preferir secret manager; alternativa cifrado autenticado (p.ej. AES-GCM) con clave fuera de PostgreSQL y versionado.
- Rotación y revocación.
- Redactar headers y payloads sensibles en logs.
- QR de WhatsApp es secreto temporal.
- No incluir secretos en mensajes RabbitMQ.

## API y archivos

- Rate limit por organización/aplicación/canal.
- Límites de tamaño, MIME real, extensión y antivirus/escaneo si se requiere.
- URLs de proveedor con protección SSRF: bloquear loopback, link-local, metadata cloud y redes internas no autorizadas; allowlist cuando sea viable.
- URLs MinIO firmadas y de corta duración.
- CORS restrictivo para Admin; CORS no sustituye autenticación.
- TLS en todos los saltos sensibles cuando la topología lo permita.

## Auditoría y privacidad

Registrar actor, organización, aplicación, acción, recurso, timestamp, resultado y correlation id. No registrar tokens, contraseñas, contenido sensible completo ni QR. Definir retención, borrado y anonimización por política legal/operativa.
