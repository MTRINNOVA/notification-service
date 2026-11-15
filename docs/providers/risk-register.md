# Registro de riesgos — WhatsApp con Evolution/Baileys

| Riesgo | Impacto | Mitigación / control | Condición de salida |
|---|---|---|---|
| Sesión desconectada o invalidada | Mensajes no enviados y atención interrumpida | Monitor de conexión, alertas, estado visible, re-vinculación QR con administrador | Probar desconexión, reconexión y recuperación |
| Restricción o suspensión del número | Pérdida temporal o permanente del canal del cliente | Opt-in verificable, bajas, control de frecuencia, política anti-spam, transparencia contractual | Aprobar política de uso y proceso de incidentes |
| Cambios de protocolo/compatibilidad Baileys | Interrupción general o parcial | Pin de versión/digest, staging, pruebas de regresión, plan de actualización/rollback | Validar release y procedimiento de rollback |
| QR o credenciales expuestos | Toma de sesión o acceso no autorizado | QR efímero, acceso RBAC, no logs, TLS, secretos protegidos y auditoría | Prueba de seguridad del onboarding |
| Cruce de datos entre organizaciones | Envío desde número equivocado o exposición de datos | Aislamiento por organization_id, claves foráneas, autorización sender-app, pruebas negativas | Pruebas automatizadas de aislamiento |
| Webhook falso/repetido | Estados incorrectos o manipulación | Autenticación validada por versión, deduplicación, persistencia antes de ACK | Contrato real de webhook documentado y probado |
| Envío ambiguo y reintento duplicado | Mensajes duplicados | Idempotencia, estados indeterminados, conciliación; no retry ciego | Pruebas de timeout después de aceptación |
| Saturación de instancia o VPS | Retrasos y acumulación de cola | Cuotas por tenant/sender, backpressure, métricas y pruebas de carga | Prueba con carga representativa y picos |
| Dependencia de un solo proveedor WhatsApp | Mayor impacto ante caída o cambio de Evolution | Puerto neutral y diseño preparado para adaptador Meta Cloud API | Contrato interno independiente del proveedor |
| Incumplimiento de políticas/consentimiento | Restricción del canal y riesgo contractual/legal | Responsabilidad del cliente, opt-in, listas de supresión, auditoría y límites | Política de uso y términos aceptados |

## Riesgo residual y comunicación

Evolution/Baileys no ofrece las mismas garantías contractuales ni el mismo modelo operativo que la API oficial de WhatsApp Business Platform. La plataforma no debe prometer entrega, disponibilidad del número ni inmunidad a restricciones. Comunicar al cliente que debe ser titular o contar con autorización sobre el número, mantenerlo operativo y cumplir las políticas aplicables.
