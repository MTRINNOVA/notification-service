# Arquitectura Clean + Hexagonal

## Capas

### Domain

Entidades y reglas puras: `Organization`, `ApplicationClient`, `ProviderAccount`, `ProviderInstance`, `Notification`, `NotificationDelivery`, `NotificationAttempt`, `Template`, `Campaign`, `ScheduledNotification`, `Attachment`, `WebhookEvent`, value objects, enums y domain events.

### Application

Casos de uso y puertos: comandos, consultas, validadores, autorización de casos de uso, DTO de aplicación y contratos de salida.

### Infrastructure

Implementaciones de persistencia, mensajería, caché, almacenamiento, cifrado y proveedores externos.

### Presentation / Hosts

- `NotificationService.Api`: adaptadores HTTP REST.
- `NotificationService.Worker`: adaptador de entrada RabbitMQ.
- `NotificationService.Scheduler`: adaptador de entrada de reloj/planificación.

## Puertos de entrada

- `ISendNotificationUseCase`
- `IQueryNotificationsUseCase`
- `IManageOrganizationsUseCase`
- `IManageApplicationsUseCase`
- `IManageProviderAccountsUseCase`
- `IManageTemplatesUseCase`
- `IManageCampaignsUseCase`
- `IScheduleNotificationsUseCase`
- `IProcessProviderWebhookUseCase`

## Puertos de salida

- `INotificationRepository`
- `IOrganizationRepository`
- `IApplicationRepository`
- `IProviderAccountRepository`
- `ITemplateRepository`
- `ICampaignRepository`
- `IUnitOfWork`
- `IOutboxRepository`
- `IMessagePublisher`
- `IObjectStorage`
- `ISecretProtector`
- `IContentEncryptionService` (TP-47): cifrado PGP de cuerpo y adjuntos y empaquetado ZIP AES-256 de adjuntos. Lo define Application y lo implementa Infrastructure. Lo usa el Worker al construir el envío, después de descifrar la contraseña ZIP con `ISecretProtector`. Implementarlo requiere autorizar sus paquetes NuGet
- Puertos de proveedores por canal (ver la sección siguiente)
- `IIdempotencyStore`
- `IClock`

## Puertos de proveedores (decisión aprobada PD-03, 2026-11-15)

Se usan **puertos especializados por canal**, sin contrato universal. Esta sección sustituye al contrato común `INotificationProvider` (`ProviderCode`, `Channel`, `Capabilities`, `SendAsync`, `TestConnectionAsync`, `GetHealthAsync`) y al puerto genérico `IProviderConnectionManager` de la versión anterior de este documento. Ambos quedan **retirados** y no deben implementarse. Historial: [PENDING-DECISIONS.md, PD-03](../history/PENDING-DECISIONS.md#pd-03-puertos-de-proveedores).

Ubicación prevista (aún no implementada): `NotificationService.Application/Abstractions/Providers/`.

| Grupo    | Puerto / tipo                            | Responsabilidad                                                                                                                                                                                                                         |
| -------- | ---------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| WhatsApp | `IWhatsAppProvider`                      | Envío de texto y media → `ProviderSendResult`                                                                                                                                                                                           |
| WhatsApp | `IWhatsAppConnectionManager`             | Crear/eliminar instancia, obtener QR efímero, consultar estado, desconectar/reconectar                                                                                                                                                  |
| Email    | `IEmailProvider`                         | Envío de correo → `ProviderSendResult`. Postfix y SMTP externo comparten el puerto con perfiles distintos                                                                                                                               |
| SMS      | `ISmsProvider`                           | Envío de SMS → `ProviderSendResult`                                                                                                                                                                                                     |
| Común    | `ProviderSendResult`                     | Resultado neutral del intento: `PROVIDER_ACCEPTED`, `TRANSIENT_FAILURE`, `PERMANENT_FAILURE` o `AMBIGUOUS`, más `providerMessageId`, `errorCode` y `retryAfter` opcionales (semántica en [ADR-0012](../adr/0012-delivery-semantics.md)) |
| Común    | `ProviderCapabilities`                   | Capacidades explícitas por proveedor: texto, media, conexión por QR, evidencia de entrega, evidencia de lectura, webhooks                                                                                                               |
| Común    | `IProviderWebhookVerifier`               | Verificar la autenticidad del webhook para una cuenta de proveedor                                                                                                                                                                      |
| Común    | `IProviderWebhookTranslator`             | Traducir el payload ya validado a eventos internos canónicos                                                                                                                                                                            |
| Común    | `IProviderAccountDiagnostics` (opcional) | Diagnóstico de una cuenta de proveedor (TP-17): `TestConnectionAsync` (prueba activa de conexión y credenciales) y `GetHealthAsync` (salud conocida). Resultados neutrales; nunca credenciales, tokens ni DTO del proveedor             |

Reglas:

1. Solo WhatsApp tiene operaciones de conexión/QR. Email y SMS **no** implementan operaciones de QR.
2. Los DTO, rutas y excepciones de Evolution, SMTP y SMSGate permanecen en Infrastructure y se traducen dentro del adaptador.
3. Application define los puertos y las reglas de resolución (qué cuenta y qué remitente usar). Infrastructure implementa los adaptadores.
4. Las capacidades de cada proveedor se declaran explícitamente. Nunca se asume `DELIVERED` o `READ` si la capacidad no existe.

**Diagnóstico de cuentas (TP-17, aprobada 2026-11-15):**

- `IProviderAccountDiagnostics` atiende `POST /management/providers/accounts/{id}/test` y `GET /management/providers/accounts/{id}/health`.
- Es opcional: cada adaptador declara en `ProviderCapabilities` si admite prueba activa, salud o ambas. Sin capacidad → `422 PROVIDER_DIAGNOSTICS_NOT_SUPPORTED`.
- Está separado de los puertos de envío: nunca envía una notificación ni consume cuota.
- Resultado de la prueba: `SUCCEEDED`, `AUTHENTICATION_FAILED`, `CONFIGURATION_INVALID`, `UNREACHABLE` o `TIMEOUT`. Salud: `HEALTHY`, `DEGRADED`, `UNHEALTHY` o `NOT_CHECKED`. Los timeouts e indisponibilidades del proveedor se informan en el resultado, con timeout explícito en el adaptador.

## Selección de adaptador

Un resolvedor por canal elige el adaptador según `provider_definitions.adapter_key` de la cuenta de proveedor. La selección de cuenta, de remitente y la política de enrutamiento pertenecen a Application. La llamada HTTP/SMTP concreta pertenece a Infrastructure.

## CQRS

Se permite CQRS ligero para separar Commands y Queries. MediatR es opcional; no se exige Event Sourcing. PostgreSQL sigue siendo la fuente de verdad.
