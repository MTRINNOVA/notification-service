# Pruebas del Scheduler (TP-06)

**Estado:** decisión aprobada el 2026-11-15 y especificada en este documento. **El proyecto no existe todavía**, y no se crea vacío.

## Decisión

`tests/NotificationService.Scheduler.Tests` se crea **cuando se incorpore la primera lógica funcional propia del Scheduler**: el primer job o servicio de `src/NotificationService.Scheduler/Jobs` o `Services` con comportamiento propio. Hasta entonces:

- Las pruebas de composición del host Scheduler siguen en `NotificationService.Architecture.Tests` (`SchedulerHostCompositionTests`).
- Al crear el proyecto, esas pruebas de composición se trasladan a él, y `Architecture.Tests` conserva solo las reglas de dependencias y de estructura.
- Crear el proyecto es un cambio de solución (`.sln`, `.csproj`) y requiere la autorización de la tarea que introduzca esa lógica.

## Pruebas previstas

Cubren lo que es **propio del Scheduler** como adaptador de entrada, con `IClock` para controlar el tiempo y dobles de los puertos:

| Área | Qué se prueba |
|---|---|
| Selección por vencimiento | Solo se toman las notificaciones programadas cuyo `execute_at_utc` ya llegó según `IClock`; las futuras y las canceladas se ignoran |
| Cuota al ejecutar (TP-44) | La cuota se reserva en el día local de ejecución; con la cuota agotada, la programación queda `FAILED` con `failureCode` y no se reprograma |
| Zona horaria de organización | Las horas locales se interpretan con `organizations.timezone` (PD-05), incluidos los cambios de horario de verano y la frontera de las 00:00 locales |
| Prevención de duplicados | Una misma programación no se ejecuta dos veces, ni al repetir el ciclo ni con dos réplicas del Scheduler |
| Concurrencia | Toma de trabajo con bloqueo (`FOR UPDATE SKIP LOCKED` o equivalente) sin pérdidas ni duplicados entre instancias |
| Reintentos y errores | Un fallo transitorio de una dependencia no pierde la programación y se reintenta con límite; un error permanente queda registrado e inspeccionable |
| Recuperación tras interrupción | Al reiniciar después de una caída, se retoman las programaciones vencidas que quedaron pendientes o tomadas sin terminar |
| Apagado | Se respeta el `CancellationToken`; no queda una ejecución a medias marcada como completada |
| Health checks | `/health/live` y `/health/ready` del Scheduler (TP-05, [health-checks.md](../deployment/health-checks.md)) |

Las pruebas de concurrencia y recuperación que necesiten PostgreSQL real usan Testcontainers con PostgreSQL 17 ([integration-testing-postgresql.md](integration-testing-postgresql.md)) y van en `Infrastructure.Tests` o en `Scheduler.Tests` según dónde viva el código probado. Nunca usan EF InMemory.

## Qué no se duplica

- Las **invariantes de dominio** (transiciones de estado de la programación y de la entrega) se prueban en `Domain.Tests`.
- Los **casos de uso** que invoca el Scheduler (aceptar la notificación programada, reservar cuota, escribir en el outbox) se prueban en `Application.Tests`.
- `Scheduler.Tests` comprueba la orquestación del host: qué selecciona, cuándo invoca el caso de uso, cómo maneja la concurrencia, los errores y el apagado. No vuelve a probar las reglas de negocio.

Registro: TP-06 en [technical-pending-register.md](../operations/technical-pending-register.md). Estructura: [project-structure.md](../architecture/project-structure.md).
