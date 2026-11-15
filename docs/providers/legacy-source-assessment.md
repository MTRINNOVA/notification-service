# Evaluación de la fuente anterior: `Notificaciones-master.zip`

**Propósito:** registrar qué demuestra el proyecto anterior y qué no debe asumirse para el nuevo `notification-service`.

## Inventario verificado

| Archivo | Hallazgo | Uso para el nuevo diseño |
|---|---|---|
| `docker-compose.yml` | Docker Mailserver 17.0.0, SMSGate `latest`, Evolution API v2.3.7, MariaDB 11, PostgreSQL 16, Redis 7, MinIO y Cloudflared | Evidencia de versiones/tags y topología histórica, no configuración de producción aprobada |
| `src/adapters/WhatsAppEvolutionAdapter.ts` | Envío de texto y media por Evolution; `apikey`; rutas `sendText` y `sendMedia`; instancia configurable | Referencia funcional para el adaptador .NET |
| `src/queues/workers/whatsapp.worker.ts` | BullMQ sobre Redis, concurrencia 1, límite 1 cada 4 segundos, auditoría y registro del proveedor `EvolutionAPI-v2.3.7` | Referencia de comportamiento legado; no sustituye la decisión nueva de RabbitMQ |
| `src/adapters/SmsGateAdapter.ts` | `POST /message`, Basic Auth, `message`, `phoneNumbers`, `simNumber`, respuesta `id` | Contrato legado a validar contra la versión real de SMSGate |
| `src/adapters/SmtpPostfixAdapter.ts` | Nodemailer, perfiles SMTP y adjuntos; cifrado ZIP_AES256/PGP | Referencia de capacidades, no garantía de entrega ni de tracking |
| `src/api/v1/webhook.controller.ts` | Archivo vacío | No existe receptor webhook implementado en la fuente inspeccionada |

## Detalles de Evolution confirmados por el código

- Tag Docker en Compose: `evoapicloud/evolution-api:v2.3.7`.
- Header de autenticación para envío: `apikey`.
- Texto: `POST /message/sendText/{instance}`.
- Media: `POST /message/sendMedia/{instance}`.
- El adaptador convierte el teléfono a dígitos y permite seleccionar instancia en el envelope.
- `key.id` es utilizado como identificador de mensaje si la respuesta lo incluye.
- La respuesta HTTP exitosa se registra como `SENT` en el worker legado; esto no prueba entrega final.

## Detalles de SMSGate y riesgo de incompatibilidad

- Compose: `ghcr.io/android-sms-gateway/server:latest`, MariaDB 11, modo `private`, `GATEWAY__PRIVATE_TOKEN`.
- Adaptador: HTTP Basic con `SMSGATE_USERNAME` y `SMSGATE_PASSWORD`; ruta `POST /message`.
- El ZIP no contiene evidencia que vincule el token privado del Compose con las credenciales Basic del adaptador. La combinación se marca como inconsistencia por verificar.
- No se fija versión de la app Android ni digest de la imagen `latest`.

## Límites de evidencia

- No se infieren contratos de webhook a partir de nombres de eventos o variables de entorno. Se necesitan ejemplos reales y documentación de la release instalada.
- No se considera que el worker BullMQ/Redis sea reutilizable tal cual: la nueva arquitectura aprobada utiliza RabbitMQ y Outbox.
- No se trasladan secretos ni valores de credenciales desde el ZIP. Los valores con apariencia de secreto deben rotarse/revocarse y reemplazarse por placeholders en la fuente.
- Meta Cloud API queda fuera de la primera versión, pero la arquitectura nueva conservará un contrato neutral para añadir un adaptador independiente en una etapa posterior.

## Conclusión

La fuente anterior sirve como referencia de compatibilidad y comportamiento para Evolution, SMSGate y SMTP. No constituye una especificación completa de webhooks ni una base que deba migrarse literalmente. La nueva implementación deberá respetar la arquitectura aprobada (.NET 10, RabbitMQ, MinIO, PostgreSQL, multi-organización/multiaplicación) y validar los contratos externos con las versiones fijadas.
