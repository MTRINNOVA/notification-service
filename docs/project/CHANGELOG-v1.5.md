# Cambios documentales — v1.5 Technical Readiness

- Se confirma la relación muchos-a-muchos entre usuarios y organizaciones; un usuario puede administrar varias organizaciones con roles por ámbito.
- Se confirma una instalación centralizada de Evolution API con múltiples instancias/sesiones aisladas por organización.
- Se confirma asignación de remitentes a aplicaciones: selección explícita o predeterminado; rechazo si no existe remitente predeterminado.
- Se confirma que cada cliente proporciona y administra su propio dispositivo Android y SIM para SMS.
- Se confirma soporte SMTP dual: Postfix administrado por la plataforma y SMTP externo por organización.
- Se confirma el alcance funcional inicial: WhatsApp, email, SMS, envíos individuales y por lotes, plantillas, adjuntos, tracking disponible, scheduling, campañas y dashboard.
- Se confirma JWT de corta duración y refresh token opaco rotativo en cookie HttpOnly/Secure, con protección CSRF.
- Se confirma cuota WhatsApp agregada de 500 mensajes diarios por organización, compartida entre aplicaciones y números, con reinicio según zona horaria.
- Se confirma zona horaria IANA obligatoria por organización, con `America/Lima` como valor inicial predeterminado y almacenamiento de instantes en UTC.
- Se separan decisiones de negocio ya cerradas de pruebas técnicas pendientes de los proveedores.
- No se ha generado código de aplicación ni configuración de despliegue ejecutable.

## Consolidación documental

- Se completa el ERD lógico con membresías multi-organización, remitentes, gateways SMS, cuotas, sesiones, refresh tokens, outbox/inbox y auditoría.
- Se amplía OpenAPI 3.1 a las rutas de autenticación, API pública, administración, proveedores, remitentes, plantillas, campañas, programación, dashboard y webhooks.
- Se incorpora matriz RBAC por rol y ámbito.
- Se documentan contratos y pruebas de aceptación por proveedor, separando evidencia del ZIP y validaciones de ejecución pendientes.
- Se consolidan criterios de aceptación y Technical Readiness para iniciar el desarrollo por etapas.
- Se alinea el frontend con refresh token HttpOnly/Secure y protección CSRF.
