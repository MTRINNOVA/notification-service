# C4 — Nivel 3: Componentes de backend

```mermaid
flowchart TB
  subgraph Inbound["Adaptadores de entrada"]
    Public["Public API"]
    Management["Management API"]
    Webhooks["Webhook API"]
    Consumers["RabbitMQ Consumers"]
    Scheduler["Scheduler Jobs"]
  end

  subgraph Application["Application — casos de uso"]
    Notifications["Notifications"]
    Organizations["Organizations / Applications"]
    Providers["Provider Management"]
    Templates["Templates"]
    Campaigns["Campaigns"]
    Scheduling["Scheduling"]
    Queries["Queries / Analytics"]
  end

  Domain["Domain — entidades, reglas, eventos y value objects"]

  subgraph Outbound["Adaptadores de salida"]
    EF["EF Core / PostgreSQL"]
    Bus["RabbitMQ Publisher"]
    Cache["Redis"]
    Storage["MinIO"]
    Evolution["Evolution Adapter"]
    SMTP["SMTP Adapter"]
    Sms["SMS Gateway Adapter"]
    Secrets["Secret Protection"]
  end

  Public --> Notifications
  Management --> Organizations
  Management --> Providers
  Management --> Templates
  Management --> Campaigns
  Management --> Scheduling
  Webhooks --> Providers
  Consumers --> Notifications
  Scheduler --> Scheduling
  Notifications --> Domain
  Providers --> Domain
  Templates --> Domain
  Campaigns --> Domain
  Scheduling --> Domain
  Queries --> Domain
  Notifications -. ports .-> EF
  Notifications -. ports .-> Bus
  Providers -. ports .-> Evolution
  Notifications -. ports .-> SMTP
  Notifications -. ports .-> Sms
  Notifications -. ports .-> Storage
  Providers -. ports .-> Secrets
  EF --> Domain
  Bus --> Domain
  Cache --> Domain
```

## Regla de dependencias

- `Domain` no referencia proyectos externos ni frameworks de infraestructura.
- `Application` referencia `Domain` y define los puertos requeridos.
- `Infrastructure` implementa los puertos definidos por Application (y usa Domain).
- `Api`, `Worker` y `Scheduler` son composition roots y adaptadores de entrada.
- Los controladores/endpoints no contienen reglas de negocio.
- No devolver entidades EF ni entidades de dominio directamente desde la API.
