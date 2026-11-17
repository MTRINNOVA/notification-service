# Índice de documentación — Notification Platform v1

Índice del repositorio `notification-service` (actualizado el 2026-11-15). Los documentos **normativos** definen lo vigente; los **históricos** son evidencia de auditorías y decisiones pasadas y no deben usarse como fuente operativa.

## Normativos

### Decisiones

- [DECISIONS-v1.5.md](DECISIONS-v1.5.md): decisiones aprobadas de producto y técnicas (incluye PD-01..PD-08, TP-02, TP-05..TP-07, TP-15..TP-22, TP-28, TP-33..TP-56 y PostgreSQL 17).
- ADR vigentes:
  - [ADR-0001](../adr/0001-architecture.md): Clean + Hexagonal
  - [ADR-0002](../adr/0002-repositories.md): repositorios separados
  - [ADR-0003](../adr/0003-tenancy.md): tenancy
  - [ADR-0004](../adr/0004-whatsapp-adapters.md): adaptadores WhatsApp
  - [ADR-0005](../adr/0005-messaging.md): RabbitMQ
  - [ADR-0006](../adr/0006-storage.md): MinIO
  - [ADR-0007](../adr/0007-rest-json.md): REST/JSON
  - [ADR-0008](../adr/0008-dotnet.md): .NET 10 + EF Core + PostgreSQL
  - [ADR-0009](../adr/0009-provider-config.md): configuración de proveedores
  - [ADR-0010](../adr/0010-traefik.md): Traefik externo
  - [ADR-0011](../adr/0011-admin-identity.md): identidad, roles y `super_admin`
  - [ADR-0012](../adr/0012-delivery-semantics.md): semántica de entrega, reenvío y conciliación

### Contrato API v1

- [openapi.yaml](../api/openapi.yaml): **fuente normativa única** del contrato HTTP.
- [endpoint-inventory.md](../api/endpoint-inventory.md): resumen derivado de OpenAPI.
- [api-design.md](../api/api-design.md): criterios de diseño; no define rutas.
- [error-codes.md](../api/error-codes.md): catálogo cerrado de códigos de error.
- [admin-panel-auth-contract.md](../api/admin-panel-auth-contract.md): contrato de autenticación, cookie y CSRF que consume `notification-admin` (TP-37, TP-38).

### Arquitectura y datos

- [hexagonal.md](../architecture/hexagonal.md): capas y puertos, incluidos los de proveedores.
- [project-structure.md](../architecture/project-structure.md): estructura de la solución.
- C4: [contexto](../architecture/c4-context.md), [contenedores](../architecture/c4-containers.md), [componentes](../architecture/c4-components.md).
- [erd.md](../data/erd.md): modelo lógico aprobado v1 (PostgreSQL 17).

### Seguridad

- [security-model.md](../security/security-model.md)
- [rbac-matrix.md](../security/rbac-matrix.md)

### Mensajería y proveedores

- [rabbitmq-outbox.md](../messaging/rabbitmq-outbox.md)
- [provider-adapters.md](../providers/provider-adapters.md)
- [provider-contracts-v1.5.md](../providers/provider-contracts-v1.5.md)
- [webhook-contract-readiness.md](../providers/webhook-contract-readiness.md)
- [risk-register.md](../providers/risk-register.md): registro de **riesgos**, distinto del de pendientes.
- [legacy-source-assessment.md](../providers/legacy-source-assessment.md): evaluación de la fuente legada (evidencia).

### Operación y despliegue

- [technical-pending-register.md](../operations/technical-pending-register.md): **registro operativo único de pendientes**.
- [super-admin-bootstrap.md](../operations/super-admin-bootstrap.md): bootstrap y recuperación de `super_admin`.
- [quotas-retention.md](../operations/quotas-retention.md)
- [technical-readiness-v1.5.md](../operations/technical-readiness-v1.5.md)
- Despliegue: [docker.md](../deployment/docker.md), [compose-topology.md](../deployment/compose-topology.md), [environment-decisions.md](../deployment/environment-decisions.md), [health-checks.md](../deployment/health-checks.md) (Worker/Scheduler, TP-05), [ci-cd.md](../deployment/ci-cd.md) (GitHub Actions y GHCR, TP-07).

### Pruebas y aceptación

- [integration-testing-postgresql.md](../testing/integration-testing-postgresql.md): Testcontainers con PostgreSQL 17.
- [scheduler-tests.md](../testing/scheduler-tests.md): proyecto `Scheduler.Tests` y pruebas previstas (TP-06).
- [criteria.md](../acceptance/criteria.md): criterios de aceptación.

## Históricos (solo trazabilidad)

Todos están en [docs/history/](../history/README.md), separados de los normativos desde el 2026-11-17.


- [PENDING-DECISIONS.md](../history/PENDING-DECISIONS.md): historial de las propuestas PD-01..PD-08, todas aprobadas.
- [CHANGELOG-v1.5.md](CHANGELOG-v1.5.md)
- Informes de auditoría del 2026-11-15:
  - [repository-sanitization-report.md](../history/repository-sanitization-report.md)
  - [decision-closure-report.md](../history/decision-closure-report.md)
  - [decision-closure-report-tp15-tp20.md](../history/decision-closure-report-tp15-tp20.md)
  - [documentation-consolidation-report.md](../history/documentation-consolidation-report.md)
  - [decision-closure-report-tp28-tp37.md](../history/decision-closure-report-tp28-tp37.md) (TP-05..TP-07, TP-28, TP-33..TP-37)
  - [decision-closure-report-tp38-tp39.md](../history/decision-closure-report-tp38-tp39.md) (TP-38, TP-39)
  - [decision-closure-report-phase2-readiness.md](../history/decision-closure-report-phase2-readiness.md) (preparación de la Fase 2: TP-40, TP-43..TP-53)
  - [decision-closure-report-tp41-tp56.md](../history/decision-closure-report-tp41-tp56.md) (TP-41, TP-42, TP-54..TP-56)

## Frontend (`notification-admin`, repositorio independiente)

Rutas dentro de ese repositorio; no se pueden enlazar desde aquí:

- `docs/architecture/`
- `docs/api/management-api-consumption.md`
- `docs/security/frontend-security.md`
- `docs/security/session-management.md` (sesión de 60 minutos, aviso de cierre, recarga y contraseñas)
- `docs/deployment/docker.md`
- `docs/acceptance/criteria.md`
