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
- Las aplicaciones heredan la zona horaria de su organización, salvo que exista una configuración explícita permitida.
- Los instantes se almacenan en UTC; los cálculos de calendario, cuotas y programación respetan la zona horaria configurada.

## Reglas transversales

- Toda consulta y comando de negocio debe validar `organization_id` y, cuando corresponda, `application_id`.
- La semántica de estados será común, pero solo se reportarán estados que el proveedor realmente permita comprobar.
- Un timeout ambiguo no se reintentará ciegamente; se aplicará reconciliación/deduplicación para reducir envíos duplicados.
- RabbitMQ será el broker; Redis no se utilizará como cola principal.
- Se mantiene PostgreSQL + EF Core, MinIO, .NET 10, Clean Architecture + Hexagonal y frontend independiente.

## Pendientes técnicos que no son decisiones abiertas

Estos puntos requieren inspección o pruebas y no deben confundirse con dudas de negocio ya resueltas:

1. Validar en ejecución Evolution v2.3.7: creación de instancia, QR, persistencia, reconexión, webhooks y envío de texto/media.
2. Fijar digest de Evolution y verificar que coincide con la imagen realmente desplegada.
3. Fijar release/digest de SMSGate; confirmar versión de app Android, dispositivo y SIM; resolver la discrepancia entre Basic Auth y token privado del ZIP.
4. Validar Postfix/SMTP, autenticación, DNS (SPF, DKIM, DMARC, PTR) y rebotes.
5. Completar OpenAPI 3.1, ERD/diccionario y contratos de error con las decisiones de este documento.
6. Cerrar dominios/subdominios, nombre de red Traefik, rutas/UID/GID, recursos y almacenamiento del VPS.
7. Definir y probar RPO/RTO y restauración de backups.
8. Ejecutar pruebas de aislamiento multi-tenant, carga y recuperación.

**Estado final:** las nueve decisiones consultadas quedan aceptadas. La programación sigue sin iniciarse; el siguiente entregable es consolidar estas decisiones en el modelo de datos, OpenAPI y criterios de aceptación antes de autorizar el desarrollo.
