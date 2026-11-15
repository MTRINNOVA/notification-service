# Despliegue Docker

## Principios

- Traefik ya existe en el VPS y queda fuera del repositorio.
- Compose de backend y Compose de Admin independientes.
- Imágenes publicadas en GHCR con tags inmutables/semver y digest en producción.
- PostgreSQL, RabbitMQ, Redis y MinIO no publican puertos al host en producción.
- Persistencia mediante volúmenes/directorios dedicados.
- Secretos fuera del repositorio; `.env.example` solo contiene nombres y valores ficticios.
- Healthchecks, límites de recursos, usuario no-root y logs con rotación.
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

## Dominios sugeridos

- `api-notifications.<dominio>` -> API
- `admin-notifications.<dominio>` -> Admin
- `hooks-notifications.<dominio>` -> API/webhooks, opcionalmente router separado

## Datos persistentes

- PostgreSQL: backup lógico y físico según RPO/RTO.
- RabbitMQ: volumen y definición de colas reproducible.
- Redis: decidir si la caché es reconstruible; AOF solo si se requiere persistencia de datos operativos.
- MinIO: versionado/retención y backup independiente; no asumir que el volumen por sí solo es backup.

## Release

1. Build/test.
2. Publicar imágenes versionadas.
3. Backup y verificación.
4. Ejecutar migración como tarea única.
5. Actualizar API/Worker/Scheduler.
6. Smoke tests.
7. Verificar colas, salud de proveedores y métricas.
8. Rollback de imagen; las migraciones deben ser compatibles con despliegue expand/contract.

## Pendiente antes de producción

Fijar versiones/digests de imágenes, credenciales, nombres de redes reales, rutas, recursos, backups, TLS, DNS y políticas de firewall. No copiar valores secretos a Portainer como texto en archivos versionados.
