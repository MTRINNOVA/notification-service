# Technical Readiness — v1.5

## Resultado

**Diseño funcional y decisiones de arquitectura aprobados.** La documentación está preparada para iniciar la construcción del backend y frontend por etapas. Esto no equivale a que los proveedores o el despliegue de producción hayan sido probados.

## Qué está definido para comenzar

- Stack: .NET 10, PostgreSQL + EF Core, RabbitMQ, Redis, MinIO.
- Arquitectura: Clean Architecture + Hexagonal; API REST JSON UTF-8.
- Tenancy: usuarios multi-organización, aplicaciones de una organización, RBAC por membresía y `super_admin` de plataforma.
- WhatsApp inicial: Evolution centralizado, instancia/sesión aislada por número y organización, onboarding QR.
- Meta Cloud API: adaptador futuro; no forma parte del primer release.
- SMS: dispositivo Android/SIM aportado por cada organización; contrato SMSGate por validar.
- Email: Postfix central y SMTP externo por organización.
- Cuota inicial WhatsApp: 500 mensajes diarios por organización, agregada entre apps/números y reiniciada según timezone IANA de la organización.
- Seguridad panel: access JWT corto, refresh token opaco rotativo en cookie HttpOnly/Secure, protección CSRF; API Keys separadas por aplicación.
- Frontend: repositorio independiente React + TypeScript + Tailwind + shadcn/ui.
- Traefik existente: externo a los Compose del proyecto.

## Puerta de inicio de programación

Se puede iniciar la fundación del software cuando se aprueben en revisión técnica los documentos de esta versión:

1. `docs/data/erd.md`
2. `docs/api/openapi.yaml` y `docs/api/api-design.md`
3. `docs/security/security-model.md` y `docs/security/rbac-matrix.md`
4. `docs/messaging/rabbitmq-outbox.md`
5. `docs/providers/provider-contracts-v1.5.md`
6. `docs/acceptance/criteria.md`

La implementación de un adaptador queda condicionada a la validación del contrato real del proveedor. No se necesita bloquear la fundación del backend por una prueba física de SMSGate o DNS SMTP.

## Validaciones durante el desarrollo, antes de habilitar cada canal

- Evolution: versión/digest, QR, sesión, reconexión, envío y webhooks reales.
- SMSGate: release/digest, app Android, dispositivo/SIM, autenticación, endpoint y callbacks.
- SMTP: autenticación TLS, DNS SPF/DKIM/DMARC/PTR, rebotes y reputación.
- MinIO: versión fijada, bucket privado, URLs firmadas, escaneo y recuperación.
- RabbitMQ: versión fijada, durabilidad, publisher confirms, DLQ y recuperación.

## Pendientes de producción

1. Dominio raíz y DNS definitivos.
2. Nombre exacto de la red Traefik y labels.
3. Confirmar qué servicios se alojan en VPS compartido o separado.
4. Paths, filesystem, UID/GID, espacio libre y backups externos.
5. RPO/RTO y prueba de restauración.
6. Límites CPU/RAM/disco y observabilidad.
7. Dimensionamiento según cantidad de organizaciones, números WhatsApp, dispositivos SMS, adjuntos y mezcla de canales.
8. Rotación de credenciales del ZIP legado y limpieza del historial Git si fueron versionadas.

## Alcance de v1

WhatsApp Evolution, Email SMTP, SMS Android Gateway, envío individual y por lotes, plantillas, adjuntos, seguimiento de estados disponibles, programación, campañas y dashboard administrativo. La habilitación de cada canal depende de sus pruebas de aceptación. Meta Cloud API queda para una etapa posterior.

## Definition of Done por módulo

- Código con pruebas unitarias y de integración.
- Validación de autorización y aislamiento multi-tenant.
- Migración EF Core revisada y reversible cuando aplique.
- Contrato OpenAPI actualizado y pruebas de contrato.
- Logs, métricas y auditoría sin secretos/PII innecesaria.
- Manejo de fallos, reintentos, idempotencia y DLQ definido.
- Documentación operativa y configuración de entorno actualizadas.
