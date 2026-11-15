# ADR-0012: Semántica de entrega

- Estado: Aprobado
- Decisión: persistencia transaccional + Outbox + consumo al menos una vez; idempotencia en el servicio y deduplicación de webhooks.
- No prometer exactly-once extremo a extremo.
- En timeout ambiguo consultar estado externo cuando sea posible y evitar reenvío ciego.
