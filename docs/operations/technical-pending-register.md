# Registro de pendientes técnicos

Registro vivo de pendientes técnicos y deuda conocida. **No sustituye a los ADR**, a [DECISIONS-v1.5.md](../project/DECISIONS-v1.5.md) ni a las decisiones pendientes de producto/arquitectura ([PENDING-DECISIONS.md](../project/PENDING-DECISIONS.md)).

Estados: `Abierto`, `Requiere aprobación`, `Planificado (fase N)`, `Cerrado`.

| ID | Pendiente | Impacto | Propuesta | Estado |
|---|---|---|---|---|
| TP-01 | Imágenes Docker no construidas en este entorno (daemon Docker no disponible el 2026-11-15) | Los Dockerfiles no tienen evidencia de build | Ejecutar `docker build -f src/NotificationService.<Host>/Dockerfile .` para los tres hosts antes de publicar en GHCR | Abierto |
| TP-02 | Base de datos para pruebas de integración de persistencia | Sin ella no se puede validar la Fase 2 contra PostgreSQL real (EF InMemory no está permitido como sustituto) | Elegir mecanismo: Testcontainers, servicio PostgreSQL en CI o Compose de pruebas | Requiere aprobación (antes de Fase 2) |
| TP-03 | `dotnet-ef` y `Microsoft.EntityFrameworkCore.Design` no instalados | Necesarios para migraciones | Al autorizar la Fase 2: manifiesto local `.config/dotnet-tools.json` con `dotnet-ef` 10.0.x igual a EF Core, y `Design` (con `PrivateAssets=all`) en el proyecto que corresponda | Planificado (Fase 2) |
| TP-04 | Readiness sin comprobaciones de dependencias | `/health/ready` responde *Healthy* porque aún no hay dependencias implementadas | Registrar cada dependencia con la etiqueta `ready` cuando su fase la implemente (PostgreSQL en Fase 2) | Planificado (por fase) |
| TP-05 | Worker y Scheduler sin endpoint/sonda de salud | Docker/Portainer no puede distinguir liveness/readiness de esos procesos | Definir el mecanismo (endpoint HTTP mínimo, archivo de heartbeat o `HEALTHCHECK` del contenedor) al crear el Compose | Requiere aprobación |
| TP-06 | No existe `NotificationService.Scheduler.Tests` | Las pruebas de composición del Scheduler viven temporalmente en `Architecture.Tests` | Crear el proyecto cuando el Scheduler tenga lógica propia (cambio de estructura, requiere autorización) | Requiere aprobación |
| TP-07 | Sin CI/CD | Restore/build/test/vulnerabilidades no se verifican automáticamente | Workflow de GitHub Actions: restore, build Release, test, `--vulnerable`, build de imágenes y publicación en GHCR por tag | Requiere aprobación |
| TP-08 | Sin Docker Compose | Topología solo documentada | Compose de desarrollo y, después, el de producción (Traefik externo, redes, límites, secretos) en su fase | Planificado |
| TP-09 | Catálogo de códigos de error (`code`) no definido | El middleware usa `INTERNAL_SERVER_ERROR`; los demás códigos no existen | Crear el catálogo junto con PD-04/PD-08 | Abierto |
| TP-10 | Paquetes de pruebas con versiones mayores nuevas disponibles (xunit.runner.visualstudio 4, Microsoft.NET.Test.Sdk 18, coverlet 10) | Ninguno funcional; las versiones actuales funcionan en net10.0 | Evaluar en un cambio dedicado, no "de paso" | Abierto |
| TP-11 | Las imágenes runtime usan el tag `10.0` (flotante dentro de 10.0.x) | Builds no reproducibles byte a byte | Fijar digest en el manifiesto de versiones de release ([docker.md](../deployment/docker.md)) | Abierto |
| TP-12 | `AllowedHosts: "*"` en la API | Sin filtrado de cabecera Host en la aplicación (Traefik enruta por host) | Configurar `AllowedHosts` por entorno cuando se confirmen los DNS (PD-06) | Abierto |
| TP-13 | Referencias a documentos inexistentes en el índice (`DECISIONS-2026-11.md`, `CHANGELOG-v1.4.md`) | Navegación documental incompleta | El usuario retiró las referencias del índice | Cerrado |
| TP-14 | El documento OpenAPI generado en tiempo de ejecución (`/openapi/v1.json`) y el contrato normativo (`docs/api/openapi.yaml`) pueden divergir | Riesgo de que el cliente use un contrato no normativo | Al implementar endpoints, añadir pruebas de contrato que comparen lo implementado con `openapi.yaml` | Planificado |
