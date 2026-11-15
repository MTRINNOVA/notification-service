# Matriz de cierre de contratos de webhooks

## Principio de diseño

Cada proveedor conserva su contrato externo y su adaptador propio. El adaptador valida autenticidad, identifica la cuenta/instancia, persiste el evento recibido y lo transforma a un evento interno canónico. No asumir que los proveedores entregan los mismos estados ni la misma semántica.

## Estado por proveedor

| Proveedor         | Decisión/evidencia                                                                                                                                          | Pendiente                                                                                                                                   |
| ----------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------- |
| Evolution API     | `evoapicloud/evolution-api:v2.3.7` fijado en `Notificaciones-master/docker-compose.yml`; envío de texto/media implementado en `WhatsAppEvolutionAdapter.ts` | Digest de imagen realmente desplegado; eventos habilitados; URL/header de webhook; payloads anonimizados; semántica de reintentos y estados |
| SMSGate Server    | `ghcr.io/android-sms-gateway/server:latest` en Compose; MariaDB 11; modo privado; túnel Cloudflare                                                          | Fijar release/digest; versión app Android; alinear autenticación; confirmar endpoint y callbacks en esa versión                             |
| Docker Mailserver | `ghcr.io/docker-mailserver/docker-mailserver:17.0.0` en Compose                                                                                             | Digest desplegado, configuración DNS y mecanismo de bounces/DSN que se adoptará                                                             |

Meta WhatsApp Cloud API no se implementa en esta etapa, pero queda como proveedor futuro previsto por ADR-0004. No es un bloqueo del lanzamiento inicial.

## A. Evolution API v2.3.7

1. El Compose confirma tag `v2.3.7`; obtener digest con `docker image inspect` en el entorno desplegado.
2. Confirmar nombre de cada instancia y relación con `provider_account_id`.
3. Revisar la configuración de webhook de la instancia y habilitar únicamente eventos que el servicio consuma: conexión, QR, mensajes entrantes y actualizaciones de mensaje/estado según disponibilidad de esa release.
4. Confirmar autenticación del webhook y si la URL se expone por red privada o proxy.
5. Capturar ejemplos JSON reales anonimizados para cada evento habilitado.
6. Determinar comportamiento de timeout, reintentos, códigos HTTP y orden de eventos.
7. La fuente anterior no implementa receptor webhook: `src/api/v1/webhook.controller.ts` está vacío. El nuevo contrato debe diseñarse y probarse, no darse por existente.

## B. SMSGate / Android SMS Gateway

1. Fijar versión del servidor (reemplazar `latest`) y registrar digest.
2. Consultar versión de la app Android y modelo/dispositivo/SIM disponibles.
3. Resolver la discrepancia del ZIP: Compose configura `GATEWAY__PRIVATE_TOKEN`, mientras `SmsGateAdapter.ts` envía Basic Auth con usuario/contraseña.
4. Validar endpoint `/message` y payload legado `{ message, phoneNumbers, simNumber }` contra la versión elegida; actualizar contrato según documentación de esa versión.
5. Confirmar si se habilitará webhook, polling o ambos, su autenticación y política de reintentos.
6. Obtener ejemplos anonimizados de SMS enviado, fallido y delivery report si la combinación dispositivo/operador los proporciona.
7. Confirmar comportamiento offline, cambio de SIM, permisos Android, batería y falta de saldo.

## Contrato interno normalizado

El modelo canónico debe contemplar, como mínimo:

- `provider`: `evolution`, `android_sms_gateway`, `postfix_smtp`.
- `provider_account_id` y, si aplica, `provider_instance_id`.
- `event_type` interno normalizado.
- `provider_event_id` o clave de deduplicación calculada cuando el proveedor no la entrega.
- `provider_message_id`.
- `occurred_at` del proveedor y `received_at` del servicio.
- `recipient` y `channel`, con datos personales minimizados.
- `raw_payload_reference` o payload cifrado/restringido según política de retención.
- `signature_valid` / resultado de autenticación.
- `processing_status`, `processed_at` y `failure_reason`.

## Reglas de recepción

1. Validar autenticidad antes de procesar el evento.
2. Limitar tamaño del cuerpo y validar content type.
3. Persistir el evento y su clave de deduplicación antes de responder éxito.
4. Responder rápido; procesar efectos secundarios de forma asíncrona.
5. Soportar duplicados y eventos fuera de orden.
6. No convertir ausencia de un evento de entrega en estado `delivered`.
7. Mantener el payload original protegido y con retención limitada.
8. No registrar tokens, firmas, cuerpos completos ni contenido sensible en logs.
9. La semántica de reintentos y códigos HTTP se fijará por proveedor/versión después de revisar documentación oficial.

## Criterio para cerrar cada contrato

Un contrato se considera cerrado cuando se cuenta con:

- versión exacta/digest fijado;
- documentación oficial de esa versión;
- configuración real de webhook o mecanismo alternativo de consulta;
- mecanismo de autenticación definido;
- payloads de ejemplo para todos los eventos del alcance;
- catálogo de errores y política de reintentos;
- pruebas de autenticación, duplicados, eventos fuera de orden y payload inválido.
