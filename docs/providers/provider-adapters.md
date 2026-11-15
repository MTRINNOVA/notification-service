# Adaptadores de proveedores

## Catálogo aprobado

| Código | Canal | Tecnología | Etapa | Evidencia/estado |
|---|---|---|---|---|
| `EVOLUTION_API` | WhatsApp | Evolution API / Baileys | Inicial | `evoapicloud/evolution-api:v2.3.7` confirmado por el ZIP; digest pendiente |
| `META_CLOUD_API` | WhatsApp | Meta WhatsApp Cloud API | Futuro, no implementado | Extensión arquitectónica prevista; onboarding, permisos y contrato se definirán antes de esa etapa |
| `POSTFIX_SMTP` | Email | Docker Mailserver / Postfix + OpenDKIM/OpenDMARC | Inicial | `ghcr.io/docker-mailserver/docker-mailserver:17.0.0` en ZIP |
| `ANDROID_SMS_GATEWAY` | SMS | Android SMS Gateway (SMSGate) | Inicial | `ghcr.io/android-sms-gateway/server:latest`; versión exacta pendiente |

## Contratos y aislamiento WhatsApp

- Las aplicaciones consumidoras invocan el endpoint neutral de notificaciones e indican `senderId` opcional o una política de remitente predeterminado. No envían `instanceName`, API Key de Evolution, `phone_number_id` ni tokens del proveedor.
- `sender_id` es un identificador interno, opaco y validado contra `organization_id`, `application_id`, canal y permisos. El backend resuelve el proveedor y la instancia externa.
- Una organización puede conectar varios números. Cada número/sesión se representa mediante un remitente interno y se puede asignar a una o varias aplicaciones de la misma organización según permisos explícitos.
- No se permite compartir instancias, sesiones ni credenciales entre organizaciones. La unicidad del nombre de instancia debe contemplar el ámbito global de Evolution.
- Puertos especializados por canal (PD-03, aprobada; detalle en [hexagonal.md](../architecture/hexagonal.md#puertos-de-proveedores-decisión-aprobada-pd-03-2026-11-15)): envío WhatsApp (`IWhatsAppProvider`) separado de la administración de conexión/QR (`IWhatsAppConnectionManager`); `IEmailProvider` e `ISmsProvider` sin operaciones de QR. Todos devuelven el `ProviderSendResult` neutral (`PROVIDER_ACCEPTED`, `TRANSIENT_FAILURE`, `PERMANENT_FAILURE`, `AMBIGUOUS`). No existe contrato universal `INotificationProvider`. El dominio no debe conocer rutas ni DTOs de Evolution, SMTP o SMSGate.
- Capacidades explícitas por proveedor (`ProviderCapabilities`): texto, media, estado de conexión, webhooks/eventos, evidencia de entrega, evidencia de lectura y límites. No asumir que ambos proveedores futuros soportarán exactamente las mismas funciones.

### Capacidades de estado por proveedor (v1)

| Proveedor | Aceptación (`SENT`) | Entrega final (`DELIVERED`) | Lectura (`READ`) |
|---|---|---|---|
| Evolution v2.3.7 | Sí (`data.key.id`) | Posible por webhook, **por validar** | Posible por webhook, **por validar** |
| SMTP (Postfix/externo) | Sí (respuesta SMTP positiva) | No (solo rebotes/DSN si se configuran → `FAILED`) | No |
| SMSGate | Sí (`id`) | Depende de dispositivo/operador, **por validar** | No |

Un estado sin capacidad confirmada no se reporta. Los estados y su semántica están en [ADR-0012](../adr/0012-delivery-semantics.md).

## WhatsApp — Evolution API v2.3.7 (primera etapa)

### Evidencia de la implementación anterior

`src/adapters/WhatsAppEvolutionAdapter.ts`:
- URL base `WHATSAPP_API_URL` (default `http://127.0.0.1:8081`).
- Header `apikey` con `WHATSAPP_API_KEY`.
- Instancia mediante `envelope.whatsappInstance` o `WHATSAPP_INSTANCE_NAME` (default `notificaciones_corp`).
- Texto: `POST /message/sendText/{instance}` con `number`, `text`, `delay`.
- Media: `POST /message/sendMedia/{instance}` con `number`, `mediatype`, `mimetype`, `caption`, `media` Base64 y `fileName`.
- Identificador externo: `data.key.id` cuando existe.
- Compose fuente fija `evoapicloud/evolution-api:v2.3.7`, configura PostgreSQL, Redis y volumen `evolution_instances`.

### Onboarding por QR

1. Un administrador autorizado inicia la conexión dentro de su organización.
2. El backend crea una instancia con un identificador interno/externo no predecible y configuración aislada.
3. El QR se obtiene bajo demanda, se entrega solo al administrador autorizado y expira según el contrato real de la versión fijada.
4. El backend consulta/recibe el estado de conexión y vincula el número identificado a la instancia tras validar el resultado.
5. El QR se invalida y no se almacena como dato permanente. La sesión persistente queda bajo almacenamiento protegido de Evolution.
6. Desconectar, eliminar o volver a vincular exige autorización y auditoría.

### Límites y seguridad operativa

- Cuota inicial de referencia: hasta 500 mensajes por día y organización; configurar límites adicionales por número, aplicación y canal. No convertir el valor de referencia en una promesa de throughput.
- Aplicar rate limiting conservador, concurrencia configurable y backpressure por instancia; los valores finales deben salir de pruebas con la versión fijada y de las condiciones de uso aplicables.
- No hacer reintento ciego cuando el resultado del envío sea ambiguo: consultar/conciliar estado cuando el proveedor lo permita o registrar resultado indeterminado para evitar duplicados.
- Supervisar conexión, última actividad, errores, latencia y cola pendiente por instancia. Alertar por desconexión, reconexiones repetidas y acumulación de mensajes.
- No automatizar mensajes no solicitados ni prácticas de spam. Obtener consentimiento/opt-in, respetar solicitudes de baja y las políticas aplicables de WhatsApp.
- Documentar que Baileys opera sobre el mecanismo de WhatsApp Web y no constituye la API oficial de Meta. Puede haber desconexiones, cambios incompatibles, restricciones o suspensión del número. El cliente conserva la titularidad y responsabilidad sobre el número.

### Webhooks

- El ZIP no contiene un controlador funcional de webhook Evolution (`src/api/v1/webhook.controller.ts` está vacío).
- Implementar recepción solo después de validar eventos, autenticación, payloads, reintentos y semántica para v2.3.7.
- Persistir el evento antes de responder, deduplicar por identificador/evento y resolver instancia a organización sin confiar en datos enviados por el cliente.
- La aceptación del envío y `data.key.id` no acreditan entrega final.

## Meta WhatsApp Cloud API (futuro, no implementado)

- La extensión prevista será otro adaptador del mismo puerto de aplicación; no se reutilizarán DTOs ni credenciales de Evolution.
- El onboarding esperado para clientes será Embedded Signup o el flujo oficial que se valide al iniciar esa etapa. Cada cliente conservará la propiedad de su Business Portfolio, WABA y números; la plataforma recibirá autorización para operar.
- Se deberán modelar WABA, `phone_number_id`, permisos, tokens, suscripciones de webhook, plantillas y ciclo de revocación. No almacenar tokens en frontend ni exponerlos a aplicaciones consumidoras.
- Los estados y capacidades de Meta deben mapearse al modelo neutral sin fingir equivalencia con estados de Evolution.
- No habilitar `META_CLOUD_API` como proveedor operativo hasta completar revisión de Meta, permisos requeridos, contrato de Embedded Signup, webhooks, políticas y pruebas de aceptación.

## Email — Postfix / Docker Mailserver

El ZIP utiliza `ghcr.io/docker-mailserver/docker-mailserver:17.0.0`, con OpenDKIM, OpenDMARC y SPF policy service habilitados. El adaptador usa Nodemailer, pool SMTP y perfiles etiquetados GMAIL/OUTLOOK; estos nombres no prueban OAuth completo ni entrega/tracking garantizados.

- Configurar host, puerto, TLS, autenticación y remitente por cuenta.
- SPF, DKIM, DMARC y PTR se configuran en DNS/infraestructura.
- La aceptación SMTP no equivale a entrega en bandeja ni lectura.

## SMS — Android SMS Gateway (SMSGate)

El Compose fuente usa `ghcr.io/android-sms-gateway/server:latest`, MariaDB 11, modo privado y `GATEWAY__PRIVATE_TOKEN`. El adaptador legado usa HTTP Basic y `POST /message` con `{ message, phoneNumbers, simNumber }`. La compatibilidad entre autenticación, endpoint y versión no está demostrada.

- Fijar release/digest; no desplegar `latest` en producción.
- Confirmar versión de app Android, dispositivo y SIM.
- Resolver Basic Auth frente a token privado y validar endpoint, esquema y delivery reports.

## Configuración y secretos

Configuración no sensible en PostgreSQL JSONB validado por proveedor. Secretos en un almacén seguro o cifrados con clave maestra externa. Las APIs nunca devuelven secretos; solo `credentialsConfigured`, versión/fecha de rotación y metadatos. No registrar QR, tokens, API Keys, contenido sensible ni números completos sin necesidad.

## Seguridad del ZIP de referencia

El archivo de referencia contiene valores con apariencia de credenciales. Tratarlos como comprometidos: rotar/revocar, sustituir por placeholders y revisar historial Git. No reutilizar secretos.
