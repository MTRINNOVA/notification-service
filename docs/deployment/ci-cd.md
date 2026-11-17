# CI/CD con GitHub Actions (TP-07)

**Estado:** decisión aprobada el 2026-11-15 y especificada en este documento. **Implementación pendiente:** todavía no existe ningún workflow en `.github/workflows/`. Crearlos es una tarea posterior con autorización propia.

## Decisión

GitHub Actions tendrá **dos responsabilidades separadas**, en workflows distintos y con permisos distintos:

| Responsabilidad | Disparador | Permisos del `GITHUB_TOKEN` | Publica imágenes |
|---|---|---|---|
| **CI** | Pull Requests (y pushes a ramas de trabajo) | Solo lectura (`contents: read`) | **No** |
| **Publicación** | Acción controlada: tag de versión o ejecución manual aprobada | `packages: write` solo en este workflow | Sí, en GHCR |

### CI para Pull Requests

Pasos, en orden; cualquier fallo marca el PR como no integrable:

1. **Restore:** `dotnet restore NotificationService.sln` con el SDK fijado en `global.json`.
2. **Build Release:** `dotnet build NotificationService.sln --configuration Release --no-restore`, sin advertencias nuevas.
3. **Tests:** `dotnet test NotificationService.sln --configuration Release --no-build`, incluido `Architecture.Tests`.
4. **Vulnerabilidades:** `dotnet list NotificationService.sln package --vulnerable --include-transitive`; el job falla si aparece alguna vulnerabilidad.
5. **Validación de OpenAPI:** `docs/api/openapi.yaml` debe ser OpenAPI 3.1 válido, con `operationId` únicos y todos los `$ref` resueltos. La herramienta concreta se elige al implementar.
6. **Construcción de imágenes Docker:** `docker build` de los tres Dockerfiles (`notification-api`, `notification-worker`, `notification-scheduler`), **sin publicar**. Cubre la evidencia de build que falta (TP-01).
7. **Pruebas de integración con Testcontainers** (cuando existan, Fase 2): runner con Docker disponible y **PostgreSQL 17** con la imagen fijada (patch y digest) definida en [integration-testing-postgresql.md](../testing/integration-testing-postgresql.md). El job falla si Docker no está disponible; no se omiten las pruebas en silencio.

Un Pull Request **nunca** publica imágenes productivas. Esto vale para cualquier PR, también los de forks, que además no reciben secretos.

### Publicación en GHCR

- **Imágenes independientes** (PD-07): `ghcr.io/<owner>/notification-api`, `ghcr.io/<owner>/notification-worker` y `ghcr.io/<owner>/notification-scheduler`. `<owner>` se fija al implementar.
- **Tags inmutables asociados a la versión** (p. ej. `1.4.2`, más el SHA del commit). Nunca `latest` como referencia de despliegue, y un tag publicado no se sobrescribe.
- **Digest:** se registra el digest `sha256:` de cada imagen publicada (en el resumen del workflow y en el manifiesto de versiones de release, TP-11). Producción referencia las imágenes por digest ([docker.md](docker.md)).
- La publicación solo ocurre desde un commit que ya pasó la CI completa.
- **Control:** solo se publica con un tag de versión o una ejecución manual en un environment protegido de GitHub que requiera aprobación.

### Fuera de esta aprobación

- **Despliegue automático al VPS:** no forma parte de TP-07. Queda como etapa futura independiente, que requiere aprobación explícita y depende de TP-27 (infraestructura de producción). El despliegue sigue siendo manual mediante Portainer.
- Ejecución de migraciones desde CI: no. Las migraciones siguen siendo una tarea única de release ([docker.md](docker.md#release)).

## Reglas de seguridad

- Sin secretos en los workflows ni en los logs. La publicación en GHCR usa el `GITHUB_TOKEN` con permisos mínimos.
- Las acciones de terceros se fijan por SHA de commit, no por tag móvil.
- Las imágenes de servicio (PostgreSQL 17 de Testcontainers, SDK y runtime de .NET) se fijan por versión y, cuando corresponda, por digest (TP-11).

## Trabajo pendiente

1. Crear `.github/workflows/ci.yml` (CI) y un workflow de publicación separado. **No creados en esta tarea.**
2. Elegir la herramienta de validación de OpenAPI para CI.
3. Configurar el environment protegido y el owner de GHCR.
4. Activar el job de Testcontainers cuando existan las pruebas de integración (TP-02, Fase 2).

Registro: TP-07 en [technical-pending-register.md](../operations/technical-pending-register.md). Relacionado: TP-01 (build de imágenes), TP-11 (tags y digests), [health-checks.md](health-checks.md).
