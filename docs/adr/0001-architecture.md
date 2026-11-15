# ADR-0001: Clean Architecture + Hexagonal

- Estado: Aprobado
- Contexto: Se requiere sustituir proveedores y motores sin cambiar reglas de negocio.
- Decisión: Domain/Application independientes; Infrastructure implementa puertos; API/Worker/Scheduler son adaptadores de entrada.
- Consecuencia: Más proyectos y contratos, a cambio de pruebas y reemplazo de proveedores.
