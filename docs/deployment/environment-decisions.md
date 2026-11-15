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

## Subdominios propuestos (no confirmados)

Usar variables de configuración hasta que se defina el DNS real:

- `notifications-api.<dominio>`: API pública y de administración según autorización.
- `notifications-admin.<dominio>`: panel administrativo.
- `notifications-webhooks.<dominio>`: recepción de callbacks.
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

1. Dominio raíz y nombres DNS definitivos.
2. Nombre exacto de la red Traefik y convenciones de labels.
3. Paths definitivos, filesystem, espacio libre, UID/GID y permisos.
4. Si Evolution API, Postfix y SMSGate estarán en el mismo VPS o en infraestructura separada.
5. Política de backups y objetivos RPO/RTO.
6. Límites de recursos por contenedor y monitoreo.
7. Tamaño medio/máximo de adjuntos y distribución del tráfico por canal.
