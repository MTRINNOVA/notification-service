# ADR-0009: Configuración dinámica de proveedores

- Estado: Aprobado
- Decisión: configuración validada por tipo en PostgreSQL; secretos en secret manager o cifrados con clave externa a DB.
- Consecuencia: UI puede probar/activar/rotar cuentas sin rebuild; nunca devolver secretos.
