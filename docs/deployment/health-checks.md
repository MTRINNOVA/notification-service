# Health checks de Worker y Scheduler (TP-05)

**Estado:** decisión aprobada el 2026-11-15 y especificada en este documento. **Implementación pendiente:** los hosts Worker y Scheduler todavía no exponen HTTP, y los Dockerfiles y el Compose no se han modificado.

## Decisión

Cada host `notification-worker` y `notification-scheduler` expondrá dos endpoints HTTP **internos**, con la misma semántica que la API:

| Endpoint | Tipo | Responde `Healthy` cuando… |
|---|---|---|
| `GET /health/live` | Liveness | El proceso está vivo y atiende la petición. No comprueba dependencias; si falla, el orquestador reinicia el contenedor |
| `GET /health/ready` | Readiness | El host terminó de inicializarse y puede ejecutar su función. Incluye solo los checks etiquetados `HealthCheckTags.Ready` |

Reglas comunes:

- Mismo criterio que la API ([README](../../README.md#endpoints-técnicos-de-la-api), `AGENTS.md` §16): nunca se registra un check que devuelva *Healthy* sin comprobar realmente lo que dice comprobar.
- Las dependencias (PostgreSQL, RabbitMQ, Redis, MinIO) se añaden a readiness **a medida que se implementan** en su fase, igual que en la API (TP-04). Hasta entonces, readiness solo refleja la inicialización del propio host.
- Las respuestas no incluyen cadenas de conexión, hosts internos, credenciales ni mensajes de excepción.
- Un fallo de readiness no debe reiniciar el contenedor. Solo liveness lo hace.

### Worker

- **Liveness:** el proceso responde.
- **Readiness:** el consumidor de RabbitMQ está inicializado (conexión, canal y suscripción activos) y puede procesar mensajes. Durante el apagado ordenado, readiness pasa a *Unhealthy* antes de cancelar el consumo, para no recibir trabajo nuevo.

### Scheduler

- **Liveness:** el proceso responde.
- **Readiness:** el planificador está inicializado (jobs registrados y bucle de planificación en marcha) y puede ejecutar tareas.

## Exposición

- Los endpoints son **internos**: escuchan en un puerto del contenedor que no se publica al host ni se enruta por Traefik. Worker y Scheduler no se conectan a la red `traefik` ([docker.md](docker.md#redes)).
- Docker `HEALTHCHECK` (o el healthcheck del servicio en Compose) y Portainer consultan `/health/live` y `/health/ready` dentro del contenedor.
- El puerto concreto se configura por variable de entorno al implementar. No se fija aquí.

## Implicaciones para la implementación (pendientes; requieren autorización propia)

Esta decisión **no** autoriza por sí misma los cambios siguientes. Cada uno se hará en su tarea, con las restricciones de `AGENTS.md`:

1. **Hosts:** Worker y Scheduler usan hoy el host genérico (`Microsoft.NET.Sdk.Worker`) sin servidor HTTP. Exponer HTTP requiere un servidor HTTP mínimo (p. ej. Kestrel mediante la referencia de framework `Microsoft.AspNetCore.App`). Esto cambia los `.csproj` y la composición en `Common/*HostBuilderExtensions.cs`.
2. **Imágenes:** la imagen `runtime` de .NET no incluye ASP.NET Core ni `curl`/`wget`. Habrá que pasar a la imagen `aspnet` o usar un binario de sonda propio, y ajustar el `HEALTHCHECK` sin añadir herramientas innecesarias. Esto afecta a los Dockerfiles (TP-01).
3. **Compose:** healthchecks de los servicios `notification-worker` y `notification-scheduler` cuando exista el Compose (TP-08).
4. **Pruebas:** pruebas de composición que verifiquen los dos endpoints y que readiness no incluya checks ficticios (`Worker.Tests` y, cuando exista, `Scheduler.Tests`, TP-06).

## Relacionado

- Registro: TP-04 (readiness de dependencias) y TP-05 en [technical-pending-register.md](../operations/technical-pending-register.md).
- Topología: [compose-topology.md](compose-topology.md). CI/CD: [ci-cd.md](ci-cd.md).
