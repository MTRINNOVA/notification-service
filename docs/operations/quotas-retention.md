# Cuotas, volumen de referencia y retención

## Volumen aprobado

- WhatsApp: hasta **500 mensajes por día por organización cliente** como volumen estimado inicial.
- El límite efectivo se configura por organización, aplicación, remitente/número y canal. El total de números de una empresa no debe interpretarse automáticamente como 500 por número: la cuota base es agregada por organización, salvo que un contrato comercial defina otra cosa.
- El volumen agregado de toda la plataforma depende de la cantidad de clientes activos. No se deriva un total de mensajes mensual sin conocer esa cantidad y la distribución horaria.
- La cifra es una referencia de dimensionamiento/cuota, no SLA, garantía de throughput ni autorización para enviar mensajes sin consentimiento.
- Los límites de Evolution/Baileys, el VPS y las políticas de WhatsApp pueden ser más restrictivos. Validar con pruebas controladas y seguimiento operativo.
- El volumen previo de 100,000 notificaciones/mes queda como hipótesis histórica de planificación global, no como requisito confirmado para la nueva estimación por cliente. Recalcular dimensionamiento agregado cuando se conozca el número de organizaciones y la mezcla por canal.

## Cuotas y protección contra abuso

Configurar por organización, aplicación, remitente y canal:

- mensajes por día y mes;
- solicitudes API por minuto;
- concurrencia máxima por instancia;
- velocidad de envío configurable por instancia;
- tamaño máximo de adjuntos;
- destinatarios máximos por campaña;
- trabajos programados pendientes.

Registrar período, unidad, límite, consumo, zona horaria y política de exceso (rechazar, pausar o alertar). Aplicar límites conservadores, backpressure y pausas automáticas ante desconexión, errores repetidos o señales de riesgo. No usar una tasa universal fija hasta medir la versión y las condiciones reales.

## Retención inicial

| Dato                       |                             Retención |
| -------------------------- | ------------------------------------: |
| Notificaciones y estados   |                              12 meses |
| Intentos de entrega        |                              12 meses |
| Eventos/webhooks recibidos |                               90 días |
| Logs técnicos              |                               30 días |
| Auditoría administrativa   |                              12 meses |
| Registros de idempotencia  |                                7 días |
| Adjuntos                   | 90 días después de finalizar el envío |
| Métricas agregadas         |                              24 meses |

Sujeto a obligaciones legales, contratos y necesidades de auditoría.

## Dimensionamiento y métricas

Medir mensajes por minuto/hora y por organización, cola pendiente, tiempo en cola, latencia del proveedor, fallos, duplicados, estado de sesión por instancia, reconexiones y consumo de CPU/RAM/disco. Ejecutar pruebas de carga con escenarios distribuidos y picos; no extrapolar únicamente del promedio diario.

## Implementación

- PostgreSQL: cuotas, consumo agregado, metadatos y auditoría.
- Redis: rate limiting y contadores temporales, con reconciliación persistente.
- RabbitMQ: colas, reintentos controlados y DLQ; aplicar backpressure por remitente.
- MinIO: adjuntos con lifecycle.
- Redactar tokens, QR, teléfonos, destinatarios y contenido sensible en logs.
- Registrar consumo idempotentemente para evitar doble contabilización.
