# ADR-0005: RabbitMQ como broker

- Estado: Aprobado
- Decisión: RabbitMQ para comandos de entrega, eventos, retry y DLQ. Redis se reserva para caché/rate limit/temporal.
- Consecuencia: Requiere Outbox, consumidores idempotentes, publisher confirms y operación del broker.
