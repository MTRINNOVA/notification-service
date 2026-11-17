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

## Decisiones de cierre y preparación de la Fase 2 (2026-11-15 a 2026-11-17)

Resumen. El detalle de cada decisión está en [DECISIONS-v1.5.md](DECISIONS-v1.5.md) y la evidencia de cada tarea, en [docs/history/](../history/README.md). Ningún cambio de esta sección implica código implementado.

### Identidad, roles y sesión

- Roles de v1 cerrados: `super_admin` como privilegio de plataforma, y `organization_admin`, `operator` y `viewer` como roles de membresía, sin roles personalizados (PD-01, TP-20). Bootstrap y Platform API para `super_admin` (TP-15).
- Nunca se deja una organización sin `organization_admin` activo (TP-35).
- Catálogo cerrado de permisos de rol (TP-51).
- Sesión del panel:
  - cookie `refresh` y cabecera `X-CSRF-Token` con token sincronizado con la sesión (TP-37, TP-38, TP-40);
  - recuperación de la sesión al recargar la página (TP-41);
  - sesión de 60 minutos desde el login (TP-57), con aviso 5 minutos antes y reautenticación (TP-58).
- Alta de usuarios con contraseña temporal (TP-46).
- Política de contraseñas NIST y reasignación de contraseña temporal por un administrador (TP-55).

### Contrato y datos

- `openapi.yaml` es la fuente única del contrato (TP-16, TP-22). Errores RFC 9457 con catálogo cerrado (PD-04, PD-08).
- Estados en tres niveles: público, entrega e intento (PD-02).
- Reenvío manual solo por la aplicación (TP-19, TP-21).
- Cancelación también desde el panel; cancelar dos veces → `409` (TP-36, TP-39).
- Scope `attachments.read` (TP-28). Remitente predeterminado por aplicación (TP-33). Paginación completa (TP-34).
- ERD aprobado (42 entidades), con:
  - `organization_limits` y cuota de programadas al ejecutarse (TP-44, TP-45);
  - lotes parciales (TP-43);
  - cifrado PGP/ZIP (TP-47, TP-54);
  - techo de scopes y rotación de API Keys (TP-48, TP-52);
  - cuenta y gateway SMS predeterminados (TP-49, TP-56);
  - dominios cerrados de estados (TP-50) y alineación con OpenAPI (TP-53).

### Plataforma y operación

- PostgreSQL 17. Testcontainers para las pruebas de persistencia (TP-02).
- Health checks de Worker y Scheduler (TP-05). Proyecto `Scheduler.Tests` previsto (TP-06). CI/CD con GitHub Actions y GHCR (TP-07).
- Los informes de auditoría y el historial de PD se trasladan a `docs/history/` (2026-11-17).
- `openapi.yaml` queda sin identificadores internos `TP-xx`. En los demás documentos normativos, cada `TP-xx` se cita una vez por sección (2026-11-17).
