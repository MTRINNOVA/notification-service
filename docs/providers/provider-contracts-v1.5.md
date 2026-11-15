# Contratos de proveedores y plan de validación — v1.5

Este documento separa evidencia del ZIP `Notificaciones-master.zip`, decisiones de arquitectura y pruebas que aún deben ejecutarse. Los valores de credenciales del ZIP se consideran expuestos y no se reproducen aquí.

## Estado resumido

| Proveedor | Evidencia del ZIP | Contrato objetivo | Prueba pendiente |
|---|---|---|---|
| Evolution | Imagen `evoapicloud/evolution-api:v2.3.7`; adaptador usa `apikey`, `/message/sendText/{instance}`, `/message/sendMedia/{instance}` | Una instancia/sesión por número y organización, centralizada | Crear instancia, QR, conexión, persistencia, reconexión, texto/media, webhooks, errores y límites |
| SMSGate | Imagen `ghcr.io/android-sms-gateway/server:latest`; MariaDB 11; Compose usa token privado; adaptador usa Basic Auth y `POST /message` | Gateway y dispositivo Android propiedad de cada cliente | Fijar release/digest; confirmar versión Android; resolver auth; validar ruta/payload/estado y SIM |
| Docker Mailserver | Imagen `ghcr.io/docker-mailserver/docker-mailserver:17.0.0`; DKIM/DMARC/SPF policy | SMTP centralizado más SMTP externo por organización | Prueba TLS/auth, entrega, rebotes, DNS SPF/DKIM/DMARC/PTR y reputación |
| PostgreSQL | Imagen `postgres:16-alpine` | PostgreSQL administrado por plataforma | Fijar digest, migraciones, backup/restore, conexiones y RLS si se adopta |
| Redis | Imagen `redis:7-alpine` | Cache, rate limiting y datos temporales; no broker | Fijar digest, política memoria/evicción y disponibilidad |
| MinIO | Imagen `chainguard/minio:latest`; integración incompleta en legado | Bucket privado para adjuntos | Fijar versión/digest, API/SDK, políticas, URL firmada, backup/restore |
| RabbitMQ | No está en Compose legado | Broker principal | Seleccionar imagen/version, quorum/colas, DLX, confirms, persistencia y prueba de reinicio |

## Evolution API — evidencia y contrato interno

### Evidencia identificada

- `WHATSAPP_API_URL` configura la URL base; el adaptador legado utiliza como default `http://127.0.0.1:8081`.
- Header `apikey` con secreto configurado.
- Instancia por `envelope.whatsappInstance` o `WHATSAPP_INSTANCE_NAME`.
- Texto: `POST /message/sendText/{instance}`.
- Media: `POST /message/sendMedia/{instance}` con `number`, `mediatype`, `mimetype`, `caption`, `media` y `fileName`.
- El adaptador obtiene `data.key.id` como identificador externo.
- El controlador de webhook legado está vacío; no hay receptor implementado en el ZIP.

### Reglas de la nueva integración

- No usar `localhost` entre contenedores. La URL interna debe apuntar al nombre DNS del servicio y al puerto de contenedor correcto, confirmado en el Compose final.
- El backend crea el identificador de instancia; el cliente no elige nombres arbitrarios de instancia ni conoce la API Key de Evolution.
- QR efímero, protegido por autorización y nunca almacenado en logs/caché persistente.
- Guardar estado de conexión, `last_seen_at`, identificador externo y referencia protegida a la sesión.
- Validar tenant al resolver webhook: `providerAccountId` debe mapear a una cuenta interna activa.
- El evento externo se guarda antes de responder éxito; después se transforma a evento interno.
- `data.key.id` prueba aceptación/identificación del mensaje en el proveedor, no entrega final.
- Resultado de envío ambiguo se marca `UNKNOWN/RECONCILIATION_REQUIRED`; no reintentar ciegamente. (Nombre del estado y su encaje en la máquina de estados: pendiente, [PD-02](../project/PENDING-DECISIONS.md#pd-02-máquina-de-estados-de-notificación-entrega-e-intento).)

### Pruebas de aceptación

1. Crear instancia para organización A; verificar que organización B no puede verla ni consultarla.
2. Generar QR y verificar expiración/no persistencia del contenido QR.
3. Vincular un número de prueba y recibir estado conectado.
4. Reiniciar Evolution y comprobar recuperación de sesión desde volumen persistente.
5. Enviar texto y media y registrar el identificador externo.
6. Simular timeout después del envío y verificar que no se duplica automáticamente.
7. Recibir evento de conexión, envío y estado; probar duplicados y autenticación inválida.
8. Desconectar/revocar instancia y comprobar que nuevas solicitudes se rechazan.

## SMSGate — discrepancia que debe resolverse

El ZIP declara `GATEWAY__PRIVATE_TOKEN` en Compose, pero el adaptador legado implementa HTTP Basic y payload `{ message, phoneNumbers, simNumber }` hacia `POST /message`. No hay evidencia suficiente para afirmar que ambos contratos correspondan a la misma versión. No copiar el contrato legado a producción hasta validar la versión fijada y documentación oficial de esa release.

## SMTP

- Distinguir `SMTP_ACCEPTED` de `DELIVERED`; una respuesta SMTP positiva no demuestra bandeja de entrada ni lectura.
- Rebotes/DSN y tracking dependen del servidor y configuración; no asumir tracking de apertura/clic universal.
- El perfil central Postfix y perfiles SMTP externos comparten el puerto interno de aplicación, pero no necesariamente capacidades idénticas.

## Política de versiones

No desplegar imágenes `latest` en producción. Registrar tag y digest inmutable de cada imagen en un manifiesto de versiones; actualizar solo mediante cambio revisado, pruebas de regresión y rollback documentado.
