# Índice de documentación — Notification Platform v1.5

## Backend (`notification-service`)

- Arquitectura y estructura: `notification-service/docs/architecture/`
- ADR: `notification-service/docs/adr/`
- Datos y ERD: `notification-service/docs/data/erd.md`
- API: `notification-service/docs/api/api-design.md`, `endpoint-inventory.md`, `openapi.yaml`, `error-codes.md`
- Seguridad: `notification-service/docs/security/security-model.md`, `rbac-matrix.md`
- Mensajería: `notification-service/docs/messaging/rabbitmq-outbox.md`
- Proveedores: `notification-service/docs/providers/provider-adapters.md`, `provider-contracts-v1.5.md`, `legacy-source-assessment.md`, `webhook-contract-readiness.md`, `risk-register.md`
- Despliegue: `notification-service/docs/deployment/`
- Operación y readiness: `notification-service/docs/operations/`
- Aceptación: `notification-service/docs/acceptance/criteria.md`
- Pruebas: `notification-service/docs/testing/integration-testing-postgresql.md`

## Frontend (`notification-admin`)

- Arquitectura: `notification-admin/docs/architecture/`
- API: `notification-admin/docs/api/management-api-consumption.md`
- Seguridad: `notification-admin/docs/security/frontend-security.md`
- Despliegue: `notification-admin/docs/deployment/docker.md`
- Aceptación: `notification-admin/docs/acceptance/criteria.md`

## Decisiones y cambios

- `DECISIONS-v1.5.md` (decisiones aprobadas)
- `PENDING-DECISIONS.md` (registro histórico de PD-01 a PD-08, **todas aprobadas** el 2026-11-15)
- `CHANGELOG-v1.5.md`

## Operación del repositorio

- Saneamiento de la fundación: `notification-service/docs/operations/repository-sanitization-report.md`
- Pendientes técnicos: `notification-service/docs/operations/technical-pending-register.md`
- Cierre de decisiones PD-01 a PD-08: `notification-service/docs/operations/decision-closure-report.md`
- Cierre de decisiones TP-02, TP-15 a TP-20: `notification-service/docs/operations/decision-closure-report-tp15-tp20.md`
- Bootstrap y recuperación de `super_admin`: `notification-service/docs/operations/super-admin-bootstrap.md`
