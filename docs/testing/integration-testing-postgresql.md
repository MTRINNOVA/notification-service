# Pruebas de integración con PostgreSQL (Testcontainers)

**Estado:** decisión aprobada (TP-02, 2026-11-15). **Planificada para la Fase 2; no implementada.** No se ha instalado ningún paquete ni se han modificado proyectos de prueba.

## Decisión

Las pruebas de integración de persistencia usan **Testcontainers para .NET** con un **PostgreSQL real** en un contenedor efímero. **EF Core InMemory y SQLite no se admiten como sustitutos de PostgreSQL**, porque no reproducen tipos, restricciones, índices parciales, JSONB, bloqueo (`FOR UPDATE SKIP LOCKED`) ni la semántica transaccional que exige el modelo ([erd.md](../data/erd.md)).

## Reglas

| Tema | Regla |
|---|---|
| Imagen | **PostgreSQL 17**, la misma versión principal que desarrollo y producción (confirmada el 2026-11-15). Tag con versión patch explícita (`postgres:17.<patch>`) y, cuando corresponda, digest. **Nunca `latest`.** El patch concreto se fija al implementar la Fase 2 y se registra en este documento |
| Paquetes | Testcontainers (módulo PostgreSQL) y Npgsql se añaden en la Fase 2 a `Directory.Packages.props`, con versiones estables y compatibles, y solo en `NotificationService.Infrastructure.Tests` (o en los proyectos de prueba que lo necesiten) |
| Ciclo de vida | Un contenedor por ejecución de la colección de pruebas (fixture compartida de xUnit), arrancado antes de las pruebas y eliminado al terminar. No hay estado persistente entre ejecuciones |
| Esquema | Las migraciones EF Core se aplican al contenedor antes de las pruebas que lo requieran, con el mismo mecanismo que en release. Nunca `EnsureCreated` para validar el esquema real |
| Aislamiento de datos | Cada prueba trabaja dentro de una transacción revertida o sobre datos con identificadores propios (organización/aplicación de prueba únicas). Limpieza explícita (p.ej. `TRUNCATE` de tablas de negocio o reinicio por plantilla de base de datos) cuando una prueba necesita confirmar transacciones (Outbox, bloqueos) |
| Paralelismo | Las pruebas que comparten contenedor no dependen del orden. Las que confirman transacciones se agrupan en colecciones no paralelas o usan bases de datos separadas en el mismo contenedor |
| Secretos | Credenciales efímeras generadas por la fixture; no se versionan ni se leen de User Secrets |
| Multi-tenancy | Las pruebas de aislamiento crean al menos dos organizaciones y verifican que no hay accesos cruzados ([criteria.md](../acceptance/criteria.md) §1) |

## Requisitos de ejecución

- **Local:** Docker Engine o Docker Desktop en ejecución, accesible para el usuario que lanza `dotnet test`. Sin Docker, las pruebas de integración fallan con un mensaje explícito; no se omiten en silencio.
- **CI (futuro, TP-07):** runner con Docker disponible (p.ej. `ubuntu-latest` de GitHub Actions); misma imagen fijada que en local; caché de la imagen cuando sea viable; el job falla si Docker no está disponible.
- Las pruebas unitarias y de arquitectura actuales **no** requieren Docker y deben poder ejecutarse sin él.

## Qué cubrirán (Fase 2 en adelante)

- Restricciones del ERD: unicidades, índices parciales (asignación activa de `super_admin`), CHECK de estados y de roles, FK compuestas de tenant.
- Transacción de aceptación: notificación + reserva de cuota + Outbox; idempotencia (`IDEMPOTENCY_KEY_CONFLICT`).
- Publicador Outbox con `FOR UPDATE SKIP LOCKED` y consumidores con Inbox.
- Historiales `delivery_status_history` / `notification_status_history` en la misma transacción.
