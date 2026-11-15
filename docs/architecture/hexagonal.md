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
- `INotificationProvider`
- `IProviderConnectionManager`
- `IProviderWebhookVerifier`
- `IIdempotencyStore`
- `IClock`

## Contrato de proveedor

Todo adaptador implementa un contrato común para enviar y, cuando proceda, administrar conexión:

- `ProviderCode`
- `Channel`
- `Capabilities`
- `SendAsync`
- `TestConnectionAsync`
- `GetHealthAsync`

La administración de sesiones/QR de WhatsApp debe residir en un puerto específico (`IWhatsAppConnectionManager`) o capacidad equivalente; no se debe forzar a proveedores Email/SMS a implementar operaciones de QR.

## Selección de adaptador

Un `ProviderResolver` resuelve el adaptador según `ProviderAccount.ProviderDefinition` y canal. La selección de cuenta y la política de enrutamiento pertenecen a Application. La llamada HTTP/SMTP concreta pertenece a Infrastructure.

## CQRS

Se permite CQRS ligero para separar Commands y Queries. MediatR es opcional; no se exige Event Sourcing. PostgreSQL sigue siendo la fuente de verdad.
