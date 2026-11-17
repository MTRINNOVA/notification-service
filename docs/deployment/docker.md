# Despliegue Docker

## Principios

- Traefik ya existe en el VPS y queda fuera del repositorio.
- Compose de backend y Compose de Admin independientes.
- Una imagen por host ([PD-07](../history/PENDING-DECISIONS.md#pd-07-estrategia-de-imágenes-docker), aprobada): `notification-api`, `notification-worker`, `notification-scheduler`, publicadas en GHCR (`ghcr.io/<owner>/notification-<host>`) con tags inmutables/semver y digest en producción.
- PostgreSQL, RabbitMQ, Redis y MinIO no publican puertos al host en producción.
- Persistencia mediante volúmenes/directorios dedicados.
- Secretos fuera del repositorio; `.env.example` solo contiene nombres y valores ficticios.
- Healthchecks, límites de recursos, usuario no-root y logs con rotación. Los tres hosts exponen `/health/live` y `/health/ready`; en Worker y Scheduler son endpoints internos (TP-05, [health-checks.md](health-checks.md)).
- Migraciones EF Core ejecutadas como tarea controlada de release, no simultáneamente por cada réplica API.

## Servicios del Compose backend

- `notification-api`
- `notification-worker`
- `notification-scheduler`
- `postgres`
- `rabbitmq`
- `redis`
- `minio`

Evolution y Postfix pueden tener Compose independiente. SMS Gateway normalmente es un dispositivo/servicio externo.

## Redes

- `traefik` externa: API y Admin.
- `notification_internal`: API, Worker, Scheduler y datos.
- `notification_providers`: Worker y proveedores autohospedados.
- No declarar `notification_internal` como `internal: true` si contenedores de esa red necesitan salida a Internet. Para mayor aislamiento, crear red de datos interna y una red de egress separada.

## Dominios (variables; DNS reales sin fijar)

Decisión aprobada ([PD-06](../history/PENDING-DECISIONS.md#pd-06-subdominios-y-dns)): Compose y labels de Traefik usan siempre estas variables, nunca nombres fijos. Los nombres DNS reales todavía no están fijados (dato de infraestructura). Traefik es el existente del VPS y no forma parte de este repositorio:

| Variable                      | Destino                                                               |
| ----------------------------- | --------------------------------------------------------------------- |
| `NOTIFICATIONS_API_HOST`      | API pública y de administración                                       |
| `NOTIFICATIONS_ADMIN_HOST`    | Panel administrativo (repositorio `notification-admin`)               |
| `NOTIFICATIONS_WEBHOOKS_HOST` | Recepción de webhooks (puede apuntar a la API con un router separado) |

## Imágenes

Decisión aprobada ([PD-07](../history/PENDING-DECISIONS.md#pd-07-estrategia-de-imágenes-docker)): **una imagen independiente por host**, cada una con su Dockerfile, su runtime y su despliegue (contexto de build: raíz del repositorio):

| Imagen                   | Dockerfile                                     | Runtime                |
| ------------------------ | ---------------------------------------------- | ---------------------- |
| `notification-api`       | `src/NotificationService.Api/Dockerfile`       | `aspnet` (puerto 8080) |
| `notification-worker`    | `src/NotificationService.Worker/Dockerfile`    | `runtime`              |
| `notification-scheduler` | `src/NotificationService.Scheduler/Dockerfile` | `runtime`              |

Todos usan multi-stage con build en Release, SDK fijado a `global.json`, restore cacheable y usuario no-root (`$APP_UID`). No incluyen secretos: `.dockerignore` excluye `.env*`, `secrets.json`, `bin/`, `obj/`, `tests/` y `docs/`.

## Datos persistentes

- PostgreSQL: backup lógico y físico según RPO/RTO.
- RabbitMQ: volumen y definición de colas reproducible.
- Redis: decidir si la caché es reconstruible; AOF solo si se requiere persistencia de datos operativos.
- MinIO: versionado/retención y backup independiente; no asumir que el volumen por sí solo es backup.

## Release

CI y publicación en GHCR con GitHub Actions: [ci-cd.md](ci-cd.md) (TP-07, aprobado; workflows pendientes). El despliegue en el VPS sigue siendo manual.

1. Build/test.
2. Publicar imágenes versionadas (tags inmutables; registrar el digest).
3. Backup y verificación.
4. Ejecutar migración como tarea única.
5. Actualizar API/Worker/Scheduler.
6. Smoke tests.
7. Verificar colas, salud de proveedores y métricas.
8. Rollback de imagen; las migraciones deben ser compatibles con despliegue expand/contract.

## Pendiente antes de producción

Registrado en [technical-pending-register.md](../operations/technical-pending-register.md): versiones/digests (TP-11), infraestructura, redes, DNS y rutas (TP-27), backups (TP-29) y recursos (TP-30). No copiar valores secretos a Portainer como texto en archivos versionados. PostgreSQL de la plataforma: versión principal **17** ([DECISIONS-v1.5.md](../project/DECISIONS-v1.5.md)).
