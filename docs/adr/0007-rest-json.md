# ADR-0007: REST + JSON como contrato de integración

- Estado: Aprobado
- Decisión: API pública, management y respuestas de negocio usan HTTPS/JSON UTF-8. OpenAPI es contrato.
- Excepciones: upload multipart; formatos externos de webhook; protocolos de webhook del proveedor cuando aplique.
- Consecuencia: DTO estables, versionado y pruebas de contrato.
