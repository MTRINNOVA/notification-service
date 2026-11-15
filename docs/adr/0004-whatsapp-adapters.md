# ADR-0004: Evolution API inicial y arquitectura extensible para Meta Cloud API

- Estado: Aprobado.
- Decisión inicial: utilizar Evolution API mediante Baileys como único proveedor WhatsApp implementado en la primera etapa.
- Decisión de diseño: mantener un contrato interno independiente del proveedor y puntos de extensión para incorporar Meta WhatsApp Cloud API en una etapa posterior, sin exponer contratos de Evolution a las aplicaciones consumidoras.
- Modelo comercial/tenancy: cada organización cliente conecta sus propios números y es responsable de mantener el acceso legítimo a esos números. La plataforma administra las sesiones y credenciales técnicas necesarias, aisladas por organización.
- Onboarding inicial: creación de instancia y vinculación del número mediante QR. El QR es efímero, de un solo uso cuando corresponda y nunca se persiste en logs ni se expone a otras organizaciones.
- Escala de referencia: hasta 500 mensajes WhatsApp por día por organización. Es un objetivo de cuota/configuración inicial, no un SLA ni una garantía de capacidad del proveedor o de la infraestructura.
- Meta Cloud API no se implementa en esta etapa. El futuro adaptador podrá utilizar otro flujo de onboarding y otro modelo de credenciales, sin alterar la API pública de notificaciones.

## Consecuencias

1. Definir un puerto de aplicación neutral, por ejemplo `IWhatsAppProvider`, para envío y consulta de capacidades/estado, y un puerto administrativo separado para crear/eliminar instancias, obtener QR y consultar conexión. **Concretado el 2026-11-15 (PD-03, aprobada):** los puertos son `IWhatsAppProvider` (envío) e `IWhatsAppConnectionManager` (instancias/QR/estado), dentro del esquema de puertos especializados por canal de [hexagonal.md](../architecture/hexagonal.md#puertos-de-proveedores-decisión-aprobada-pd-03-2026-11-15). Email (`IEmailProvider`) y SMS (`ISmsProvider`) tienen sus propios puertos sin operaciones de QR. El contrato universal `INotificationProvider` queda retirado.
2. Implementar `EvolutionWhatsAppProvider` detrás de esos puertos. La lógica de negocio no debe depender de nombres de rutas, payloads ni estados exclusivos de Evolution.
3. Identificar cada remitente mediante un ID interno estable (`sender_id`). La instancia externa y el número asociado son atributos del registro, no identificadores que el cliente consumidor pueda manipular libremente.
4. Mantener `provider_account`/conexión, instancia y sender como conceptos separados. Una organización puede tener varias conexiones y números; una aplicación solo puede utilizar los remitentes que tenga asignados.
5. Meta Cloud API queda como extensión futura: reservar el valor de proveedor y el diseño de credenciales/capacidades, pero no crear endpoints, pantallas ni flujos funcionales de Meta en la primera etapa.
6. La integración Baileys depende de sesiones basadas en WhatsApp Web, no es la API oficial de WhatsApp Business Platform y está expuesta a desconexiones, cambios de protocolo, restricciones y suspensión de números. No prometer continuidad ni ausencia de bloqueos.
7. El tag `evoapicloud/evolution-api:v2.3.7` está confirmado por el ZIP de referencia; el digest real, configuración de webhook y payloads de eventos deben validarse antes de producción.
8. Respuesta de envío aceptada por Evolution no equivale a entrega ni lectura. Los estados finales dependen de eventos y contratos efectivamente validados.

## Alternativas consideradas

- Meta Cloud API desde el inicio: descartada para la primera etapa por decisión de alcance, pero se mantiene como proveedor futuro.
- Exponer Evolution directamente a cada cliente: descartada por acoplamiento, filtración de credenciales y falta de aislamiento centralizado.
- Una instancia compartida entre empresas: descartada; las sesiones y remitentes deben quedar vinculados a una organización y aislados.
