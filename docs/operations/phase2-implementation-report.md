# Fase 2 - Reporte de Implementacion

## Resumen

Se implemento la base de Fase 2 para dominio y persistencia con .NET 10, EF Core 10, Npgsql y PostgreSQL 17 para pruebas de integracion con Testcontainers.

Se incluyo:

- Modelo de dominio con 42 entidades del ERD aprobado.
- Configuracion EF Core con DbContext, configuraciones por entidad, claves, indices y restricciones principales.
- Contratos de persistencia en Application e implementaciones en Infrastructure.
- Migracion inicial EF Core.
- Pruebas de integracion con PostgreSQL real en contenedor.

## Tareas ejecutadas (T0-T7)

### T0 - Auditoria inicial

- Revision de AGENTS.md, DECISIONS, technical-pending-register, ERD, hexagonal, project-structure, criterios de aceptacion, y estrategia de Testcontainers.
- Auditoria de estructura real de la solucion y estado Git.
- Resultado: repo en estado de fundacion con capas y pruebas base, pero sin dominio/persistencia funcional de Fase 2.

### T1 - Preparacion de persistencia

- Se agrego Microsoft.EntityFrameworkCore.Design en version centralizada.
- Se habilito referencia de Design en Infrastructure y startup API para herramientas EF.
- Se configuro dotnet-ef local en .config/dotnet-tools.json (version 10.0.15).

### T2 - Modelo de dominio

- Se implementaron enums de dominio cerrados.
- Se implementaron 42 entidades del ERD en Domain.

### T3 - Persistencia EF Core

- Se amplio NotificationDbContext con DbSet para las 42 entidades.
- Se crearon configuraciones IEntityTypeConfiguration por entidad (archivo agrupado):
  - Tablas
  - Claves primarias
  - Claves compuestas
  - FKs
  - Indices unicos/compuestos
  - Filtros parciales principales
  - Restriccion de consistencia de lotes

### T4 - Repositorios

- Se crearon contratos de persistencia en Application:
  - IUnitOfWork
  - IOrganizationRepository
  - IApplicationRepository
  - INotificationRepository
  - IOutboxRepository
  - IIdempotencyRecordRepository
- Se implementaron repositorios EF y registro DI en Infrastructure.

### T5 - Migracion inicial

- Se agrego NotificationDbContextFactory para diseno de migraciones.
- Se genero migracion inicial:
  - 20261118032145_InitialCreate.cs
  - 20261118032145_InitialCreate.Designer.cs
  - NotificationDbContextModelSnapshot.cs

### T6 - Pruebas de integracion con PostgreSQL real

- Se agrego Testcontainers.PostgreSql 4.17.0.
- Se implemento fixture de PostgreSQL con digest fijado:
  - postgres@sha256:0d938e975177bef9f1f61f3f9dbcf2e0aa0a139568f66d3415ed8892eb97f6ac
- Se implementaron pruebas de esquema/migracion y restricciones:
  - existencia de tablas esperadas (42)
  - unicidad de sms_gateways por provider_account
  - unicidad de provider_account default por organizacion/canal
  - unicidad de inbox_messages por consumidor+mensaje
  - unicidad de idempotency por aplicacion+clave

### T7 - Validacion final

- tool restore, restore, build, test de solucion completa.
- analisis de vulnerabilidades de paquetes.

## Skills utilizados por tarea

### Descubrimiento ejecutado

- Skills de plugin detectados desde el catalogo de skills del entorno (plugins dotnet, dotnet-msbuild, etc.).
- Busqueda de skills locales del repositorio en:
  - .github/skills/
  - .agents/skills/
  - .claude/skills/
  - skills/
- Resultado: no se encontraron skills locales en el repositorio.

### Skills seleccionados y aplicacion

1. csharp-expert (plugin: dotnet)
   - Mecanismo de descubrimiento: catalogo de skills del entorno + lectura de SKILL.md.
   - Uso: guia para enfoque de trabajo .NET/C# cuando no hay skill de modelado de dominio dedicado.

2. msbuild (plugin: dotnet)
   - Mecanismo de descubrimiento: catalogo de skills del entorno + lectura de SKILL.md.
   - Uso: decisiones en cambios de csproj, CPM y herramientas de build/migraciones.

3. directory-build-organization (plugin: dotnet-msbuild)
   - Mecanismo de descubrimiento: catalogo de skills del entorno + lectura de SKILL.md.
   - Uso: validacion de consistencia de estructura compartida (Directory.Packages.props y csproj).

### Skills relevantes no disponibles

- No se detectaron skills locales del repositorio para modelado de dominio/ERD.
- No se detecto un skill especifico de EF Core migrations en formato local del repo; se aplico flujo con herramientas y practicas .NET/EF.

## Archivos creados

- .config/dotnet-tools.json
- src/NotificationService.Application/Abstractions/Persistence/PersistenceContracts.cs
- src/NotificationService.Domain/Enums/DomainEnums.cs
- src/NotificationService.Domain/Entities/DomainEntities.cs
- src/NotificationService.Infrastructure/Persistence/Configurations/EntityConfigurations.cs
- src/NotificationService.Infrastructure/Persistence/NotificationDbContextFactory.cs
- src/NotificationService.Infrastructure/Persistence/Repositories/EfRepositories.cs
- src/NotificationService.Infrastructure/Persistence/Migrations/20261118032145_InitialCreate.cs
- src/NotificationService.Infrastructure/Persistence/Migrations/20261118032145_InitialCreate.Designer.cs
- src/NotificationService.Infrastructure/Persistence/Migrations/NotificationDbContextModelSnapshot.cs
- tests/NotificationService.Infrastructure.Tests/PostgreSqlIntegrationFixture.cs
- tests/NotificationService.Infrastructure.Tests/PersistenceSchemaIntegrationTests.cs

## Archivos modificados

- Directory.Packages.props
- src/NotificationService.Api/NotificationService.Api.csproj
- src/NotificationService.Infrastructure/NotificationService.Infrastructure.csproj
- src/NotificationService.Infrastructure/DependencyInjection.cs
- src/NotificationService.Infrastructure/Persistence/NotificationDbContext.cs
- tests/NotificationService.Infrastructure.Tests/NotificationService.Infrastructure.Tests.csproj

## Entidades implementadas y cobertura ERD

Cobertura implementada: 42/42 entidades del ERD.

1. organizations
2. applications
3. users
4. organization_memberships
5. roles
6. permissions
7. membership_roles
8. role_permissions
9. platform_role_assignments
10. refresh_tokens
11. api_keys
12. application_permissions
13. application_limits
14. organization_limits
15. provider_definitions
16. provider_accounts
17. provider_credentials
18. provider_instances
19. whatsapp_senders
20. application_senders
21. sms_gateways
22. provider_health_checks
23. notification_batches
24. notifications
25. notification_deliveries
26. notification_attempts
27. delivery_status_history
28. notification_status_history
29. notification_events
30. idempotency_records
31. templates
32. template_versions
33. attachments
34. notification_attachments
35. campaigns
36. campaign_recipients
37. scheduled_notifications
38. quota_usage
39. webhook_events
40. outbox_messages
41. inbox_messages
42. audit_logs

## Relaciones y restricciones destacadas

- Unicidad por tenant para codigos de organizacion/aplicacion.
- Unicidad parcial de provider_accounts default por organizacion y canal.
- Unicidad por provider_account en sms_gateways.
- Unicidad compuesta de idempotency por aplicacion + idempotency_key.
- Unicidad compuesta en inbox_messages (consumer_name, message_id).
- Restriccion de consistencia en notification_batches (accepted + rejected = total).

## Migraciones

- Migracion inicial creada y aplicada en pruebas de integracion.
- Snapshot generado correctamente.

## Pruebas ejecutadas y resultados

- dotnet test tests/NotificationService.Infrastructure.Tests/NotificationService.Infrastructure.Tests.csproj -c Release
  - Resultado: 20/20 aprobadas.
- dotnet test NotificationService.sln -c Release
  - Resultado: correcto (sin errores).

## Compilacion y validaciones

- dotnet tool restore: correcto.
- dotnet restore NotificationService.sln: correcto.
- dotnet build NotificationService.sln -c Release: correcto.
- dotnet test NotificationService.sln -c Release: correcto.
- dotnet list NotificationService.sln package --vulnerable --include-transitive: sin vulnerabilidades detectadas.

## Verificacion de imagen PostgreSQL 17 y digest

Fuente de verificacion usada en entorno local:

1. docker version
2. docker pull postgres:17
3. docker image inspect postgres:17 --format "{{index .RepoDigests 0}}"

Referencia obtenida:

- postgres@sha256:0d938e975177bef9f1f61f3f9dbcf2e0aa0a139568f66d3415ed8892eb97f6ac

## Pendientes tecnicos

1. Endurecer constraints/checks adicionales para cobertura total de catálogos cerrados TP-50 (parte de enums ya modelados; faltan algunos checks SQL explicitos finos).
2. Extender pruebas de integridad multi-tenant cruzada por FKs compuestas en todas las relaciones sensibles.
3. Implementar seed de catalogos cerrados autorizado (roles/permisos/definiciones), aun no agregado en esta entrega para mantener alcance de estructura y persistencia base.
4. Añadir validaciones transaccionales de reglas de negocio TP-35 y TP-15 a nivel de casos de uso (fuera del alcance de persistencia base pura).

## Limitaciones encontradas

- El repositorio partia de una base de fundacion muy minima (sin entidades ni persistencia funcional), por lo que el volumen de implementacion de Fase 2 fue amplio.
- Se detecto y corrigio una falla inicial en SQL de filtros/check de migracion por nombres fisicos de columnas.

## Decisiones que requieren autorizacion

1. Si deseas, en una tarea separada puedo agregar seeding idempotente de catálogos cerrados (roles, permisos, provider_definitions) con datos normativos exactos de docs/security/rbac-matrix.md y docs/providers.
2. Si deseas endurecer al maximo el aislamiento multi-tenant a nivel de DB, se puede planificar una iteracion adicional enfocada en FKs compuestas y pruebas negativas cruzadas por cada agregado principal.
