# Configuración de dominios, VPS, redes y almacenamiento

## Estado confirmado

- Existe un dominio principal; los subdominios aún no están definidos.
- VPS inicial: **6 vCore, 12 GB RAM y 100 GB de almacenamiento**.
- Traefik ya existe y se administra fuera de este proyecto. No desplegar otro Traefik en los Compose de `notification-service` ni `notification-admin`.
- Almacenamiento de objetos: MinIO.
- PostgreSQL, RabbitMQ, Redis y MinIO permanecerán en redes privadas, sin puertos publicados a Internet.

## Implicaciones del VPS

La capacidad confirmada es una línea base inicial compartida, no una asignación completa para cada contenedor. Los recursos deben repartirse y medirse en conjunto con los demás servicios alojados en el VPS.

- 100 GB no equivalen a 100 GB disponibles para datos: reservar espacio para sistema operativo, Docker, imágenes, logs, backups y servicios existentes.
- No almacenar backups únicamente en el mismo disco del VPS.
- Aplicar límites y reservas de CPU/memoria por contenedor.
- Definir alertas de espacio libre, memoria, CPU, crecimiento de PostgreSQL y MinIO.
- Ejecutar pruebas de carga con el mix real de canales antes de fijar concurrencia.
- El volumen de referencia es hasta 500 mensajes WhatsApp diarios por organización. El agregado de la plataforma depende del número de clientes; dimensionar disco según adjuntos, payloads, logs, retención y pruebas de carga.

## Subdominios (variables; DNS reales sin fijar)

Decisión aprobada ([PD-06](../project/PENDING-DECISIONS.md#pd-06-subdominios-y-dns)): usar siempre variables de configuración; los nombres DNS reales todavía no están fijados:

- `NOTIFICATIONS_API_HOST`: API pública y de administración según autorización.
- `NOTIFICATIONS_ADMIN_HOST`: panel administrativo.
- `NOTIFICATIONS_WEBHOOKS_HOST`: recepción de callbacks.
- No se fijan nombres concretos en la documentación ni en los Compose; se definirán con el dominio raíz (dato de infraestructura).
- No publicar MinIO directamente. Para descargas, usar URLs firmadas de corta duración o un endpoint controlado.

## Redes Docker

- Red externa `traefik` (nombre lógico): solo los servicios que deban recibir tráfico HTTPS. El nombre real debe confirmarse con `docker network ls`.
- `notification_internal`: API, Worker, Scheduler y dependencias internas.
- `notification_providers`: conectividad controlada con proveedores autohospedados cuando sea necesaria.
- La API/webhook puede necesitar una red de entrada y otra interna.
- No marcar como `internal: true` una red que requiera salida a Internet para SMTP y otros proveedores.
- El nombre real de la red Traefik y sus labels se parametrizarán en el Compose final.

## Almacenamiento propuesto

```text
/opt/docker/notification-platform/
├── postgres/data/
├── postgres/backups/       # staging local; copia externa obligatoria
├── rabbitmq/data/
├── redis/data/
├── minio/data/
├── evolution/instances/    # solo si Evolution vive en este VPS
└── logs/
```

Los paths son una propuesta, no una configuración ejecutada. Confirmar filesystem, espacio libre, propietario/UID, permisos, snapshots y ubicación de copias externas antes de cerrar el Compose de producción.

## Datos pendientes para el despliegue final

Migrados el 2026-11-15 al registro operativo único [technical-pending-register.md](../operations/technical-pending-register.md): DNS, red Traefik, paths, UID/GID y ubicación de Evolution/Postfix/SMSGate → TP-27; backups y RPO/RTO → TP-29; límites de recursos, monitoreo y tráfico/adjuntos → TP-30.
