# Decisiones confirmadas — Technical Readiness v1.5

**Estado:** decisiones funcionales confirmadas; implementación no iniciada.

Este documento registra la aceptación del usuario de las nueve recomendaciones propuestas para cerrar el diseño de `notification-service`. Las decisiones se incorporan a la especificación; no autorizan por sí mismas el inicio de programación.

## 1. Organizaciones, usuarios y aplicaciones

- Se permite que un mismo usuario administre varias organizaciones mediante una relación muchos-a-muchos entre usuarios y organizaciones.
- Los roles y permisos se evalúan dentro del ámbito de cada organización.
- Se mantiene un `super_admin` de plataforma.
- Cada organización administra sus administradores y operadores.
- Cada aplicación pertenece a una única organización.
- El backend debe validar el ámbito de organización y aplicación en cada operación; no confiar en identificadores enviados por el cliente.

## 2. Evolution API y números WhatsApp

- Modalidad inicial: una instalación centralizada de Evolution API, administrada por la plataforma.
- Cada organización podrá conectar múltiples números, cada uno representado por una instancia/sesión independiente.
- Las sesiones, credenciales, remitentes y eventos deben quedar aislados por organización.
- El onboarding inicial será mediante QR.
- El contrato interno de WhatsApp será neutral respecto al proveedor; Meta Cloud API podrá incorporarse posteriormente mediante otro adaptador.
- Meta Cloud API no forma parte del alcance funcional inicial.

## 3. Asignación de remitentes a aplicaciones

- Una aplicación podrá tener varios remitentes autorizados.
- Un remitente podrá asignarse a varias aplicaciones de la misma organización.
- La solicitud podrá indicar `senderId` explícitamente.
- Si no se indica, se utilizará el remitente predeterminado de la aplicación.
- Si no existe un remitente predeterminado válido, la solicitud se rechazará con un error de negocio documentado.
- No se permitirá asignar remitentes entre organizaciones distintas.

## 4. SMS Gateway

- Modalidad inicial: cada empresa cliente proporciona y administra su propio dispositivo Android y SIM.
- La plataforma centraliza la integración y administración lógica, pero debe mantener la asociación del gateway/dispositivo con su organización.
- La autenticación, versión del servidor SMSGate, versión de la app Android y contrato de callbacks siguen sujetos a validación técnica antes de habilitar el adaptador en producción.

## 5. Email

- Se soportarán ambas modalidades:
  1. Postfix/SMTP administrado por la plataforma como proveedor inicial.
  2. Servidor SMTP externo configurado por una organización.
- Las credenciales SMTP externas se almacenarán cifradas y nunca se expondrán al frontend ni a las aplicaciones consumidoras.
- La selección del perfil SMTP se resolverá en el backend según la organización y la configuración autorizada.

## 6. Alcance funcional de primera versión

Se confirma como alcance objetivo inicial:

- WhatsApp mediante Evolution API/Baileys.
- Email mediante Postfix/SMTP y perfiles SMTP externos.
- SMS mediante Android SMS Gateway.
- Envío individual y por lotes.
- Plantillas.
- Adjuntos.
- Seguimiento de estados disponibles por proveedor.
- Programación de envíos.
- Campañas.
- Dashboard administrativo.

Meta WhatsApp Cloud API queda expresamente fuera de la primera versión.

## 7. Autenticación del panel

- Access token JWT de corta duración.
- Refresh token opaco, rotativo, almacenado en cookie `HttpOnly` y `Secure`.
- Protección CSRF para operaciones autenticadas mediante cookie.
- API Keys independientes por aplicación consumidora, con rotación y revocación.
- Los secretos de proveedores no se entregan al navegador.

## 8. Cuotas

- Cuota de referencia: hasta 500 mensajes WhatsApp diarios por organización.
- La cuota es agregada y compartida entre todas las aplicaciones y números de la organización.
- El reinicio ocurre a las 00:00 en la zona horaria de la organización.
- Las solicitudes rechazadas por validación no consumen cuota.
- Una solicitud aceptada para procesamiento consume cuota.
- Los reintentos técnicos del mismo mensaje no consumen cuota adicional.
- Al superar el límite se responde HTTP 429 con código de error estable y datos de reintento cuando se conozcan.
- Se podrán configurar límites menores por aplicación.
- La cuota no constituye SLA ni garantía de entrega o throughput.

## 9. Zona horaria

- Cada organización tendrá una zona horaria IANA obligatoria.
- Valor inicial predeterminado para nuevas organizaciones: `America/Lima`.
- Las aplicaciones heredan la zona horaria de su organización, salvo que exista una configuración explícita permitida. _(Precisado el 2026-11-15, PD-05: en v1 no existe ninguna configuración por aplicación; ver «Decisiones de cierre».)_
- Los instantes se almacenan en UTC; los cálculos de calendario, cuotas y programación respetan la zona horaria configurada.

## Reglas transversales

- Toda consulta y comando de negocio debe validar `organization_id` y, cuando corresponda, `application_id`.
- La semántica de estados será común, pero solo se reportarán estados que el proveedor realmente permita comprobar.
- Un timeout ambiguo no se reintentará ciegamente; se aplicará reconciliación/deduplicación para reducir envíos duplicados.
- RabbitMQ será el broker; Redis no se utilizará como cola principal.
- Se mantiene PostgreSQL + EF Core, MinIO, .NET 10, Clean Architecture + Hexagonal y frontend independiente.

## Decisiones de cierre PD-01 a PD-08 (aprobadas el 2026-11-15)

Aprobadas expresamente por el usuario. Precisan, sin sustituirlas, las decisiones de negocio anteriores. Historial y alternativas: [PENDING-DECISIONS.md](PENDING-DECISIONS.md).

| ID    | Decisión                                                                                                                                                                                   | Documento normativo                                                                                               |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ----------------------------------------------------------------------------------------------------------------- |
| PD-01 | Roles v1: `super_admin` (privilegio de plataforma en `platform_role_assignments`), `organization_admin`, `operator`, `viewer` (roles de membresía). Sin `application_admin` ni `org_admin` | [ADR-0011](../adr/0011-admin-identity.md), [rbac-matrix.md](../security/rbac-matrix.md), [erd.md](../data/erd.md) |
| PD-02 | Estados en tres niveles: público, entrega e intento; `RECONCILIATION_REQUIRED` solo en entrega; historial `delivery_status_history`                                                        | [ADR-0012](../adr/0012-delivery-semantics.md)                                                                     |
| PD-03 | Puertos de proveedores especializados por canal; sin `INotificationProvider`                                                                                                               | [hexagonal.md](../architecture/hexagonal.md), [ADR-0004](../adr/0004-whatsapp-adapters.md)                        |
| PD-04 | `400` formato, `409` conflicto, `422` regla de negocio; RFC 9457 con catálogo cerrado de `code`                                                                                            | [api-design.md](../api/api-design.md), [error-codes.md](../api/error-codes.md)                                    |
| PD-05 | Sin `applications.timezone_override` en v1; zona horaria única de la organización para cuotas y programación                                                                               | [erd.md](../data/erd.md)                                                                                          |
| PD-06 | Hosts por variables `NOTIFICATIONS_API_HOST`, `NOTIFICATIONS_ADMIN_HOST`, `NOTIFICATIONS_WEBHOOKS_HOST`; DNS reales sin fijar; Traefik externo                                             | [docker.md](../deployment/docker.md)                                                                              |
| PD-07 | Una imagen por host: `notification-api`, `notification-worker`, `notification-scheduler`                                                                                                   | [docker.md](../deployment/docker.md)                                                                              |
| PD-08 | `openapi.yaml` declara `409`, `422`, `429` (y `413`/`415`/`503` donde corresponde) por operación, con componentes RFC 9457 reutilizables                                                   | [openapi.yaml](../api/openapi.yaml)                                                                               |

## Decisiones técnicas TP-02 y TP-15 a TP-22 (aprobadas el 2026-11-15)

| ID    | Decisión                                                                                                                                                  | Documento normativo                                                                                           |
| ----- | --------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------- |
| TP-02 | Testcontainers con PostgreSQL 17 real para las pruebas de integración de persistencia                                                                        | [integration-testing-postgresql.md](../testing/integration-testing-postgresql.md)                             |
| TP-15 | Bootstrap manual e idempotente del primer `super_admin` (`SYSTEM_BOOTSTRAP`) y Platform API para concesiones y revocaciones; protección del último activo | [ADR-0011](../adr/0011-admin-identity.md), [super-admin-bootstrap.md](../operations/super-admin-bootstrap.md) |
| TP-16 | Contrato OpenAPI normalizado (rutas, paginación, operaciones del inventario); retirada de `PUT /users/{userId}/roles`                                     | [openapi.yaml](../api/openapi.yaml)                                                                           |
| TP-17 | Puerto opcional `IProviderAccountDiagnostics` para prueba de conexión y salud                                                                             | [hexagonal.md](../architecture/hexagonal.md)                                                                  |
| TP-18 | Relación muchos a muchos `notification_attachments`                                                                                                       | [erd.md](../data/erd.md)                                                                                      |
| TP-19 | Reintento técnico, reenvío manual y conciliación separados; conciliación configurable por proveedor                                                       | [ADR-0012](../adr/0012-delivery-semantics.md)                                                                 |
| TP-20 | Sin roles personalizados en v1: catálogo cerrado `organization_admin`, `operator`, `viewer`                                                               | [ADR-0011](../adr/0011-admin-identity.md), [rbac-matrix.md](../security/rbac-matrix.md)                       |
| TP-21 | Reenvío manual solicitado por la aplicación consumidora (API Key con scope `notifications.retry`); actor auditado `APPLICATION` | [ADR-0012](../adr/0012-delivery-semantics.md), [rbac-matrix.md](../security/rbac-matrix.md) |
| TP-22 | `openapi.yaml` como fuente normativa única del contrato HTTP; `PATCH .../memberships/{membershipId}/status` incorporada; `PATCH /senders/{id}/default` descartada; esquemas sin uso eliminados | [api-design.md](../api/api-design.md), [openapi.yaml](../api/openapi.yaml) |


## Base de datos (confirmada el 2026-11-15)

- **PostgreSQL 17** como versión principal en desarrollo, pruebas y producción.
- Testcontainers usa PostgreSQL 17 para las pruebas de integración (TP-02).
- Nunca `latest`: la imagen concreta se fija con versión patch explícita (`postgres:17.<patch>`) y, cuando corresponda, con digest (TP-11). No se modificaron configuraciones ejecutables ni Compose en esta decisión.

## Pendientes técnicos que no son decisiones abiertas

La lista original de ocho puntos se migró el 2026-11-15 al **registro operativo único** [technical-pending-register.md](../operations/technical-pending-register.md): Evolution → TP-24; SMSGate → TP-25; SMTP/DNS → TP-26; dominios, red Traefik, rutas y almacenamiento → TP-27; RPO/RTO → TP-29; pruebas de aislamiento, carga y recuperación → TP-32. El punto de completar OpenAPI, ERD y contratos de error quedó resuelto con PD-01..PD-08, TP-16 y TP-22.

**Estado final:** las nueve decisiones consultadas quedan aceptadas. La programación sigue sin iniciarse; el siguiente entregable es consolidar estas decisiones en el modelo de datos, OpenAPI y criterios de aceptación antes de autorizar el desarrollo.
