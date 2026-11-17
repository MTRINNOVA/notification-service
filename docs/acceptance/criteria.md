# Criterios de aceptación — v1.5

## 1. Identidad y multi-tenancy

- Un usuario puede pertenecer a más de una organización y seleccionar el contexto activo.
- Los roles se resuelven por membresía y no se comparten accidentalmente entre organizaciones. Roles de organización v1: `organization_admin`, `operator`, `viewer` (no existen `org_admin`, `application_admin` ni roles personalizados). La API no permite crear ni editar roles ni asignarlos fuera de una membresía.
- `super_admin` puede operar globalmente; toda acción global queda auditada. El primero se crea por bootstrap idempotente (`SYSTEM_BOOTSTRAP`); los siguientes, con la Platform API. No se puede revocar el último activo (`422 LAST_SUPER_ADMIN_REVOCATION_FORBIDDEN`). Se almacena en `platform_role_assignments`, independiente de las membresías; intentar asignarlo en una membresía devuelve `422 ROLE_NOT_ASSIGNABLE`. Su concesión y revocación se auditan.
- Una API Key queda vinculada a una sola aplicación y, por transitividad, a una organización.
- Suspender o desactivar una membresía (`PATCH .../memberships/{membershipId}/status`) retira el acceso a esa organización sin afectar a las otras membresías del usuario ni a su cuenta global.
- El reenvío manual lo solicita la aplicación con su API Key y el scope `notifications.retry`; la auditoría lo atribuye a la aplicación (`actor_type = APPLICATION`) y a la API Key, nunca a un usuario.
- Una aplicación no puede consultar ni modificar recursos de otra organización, incluso manipulando UUIDs.
- Las pruebas de integración verifican aislamiento para notificaciones, remitentes, proveedores, plantillas, campañas, adjuntos y auditoría.

## 2. Autenticación

- Access JWT de corta duración y validación de issuer, audience, firma, expiración y scopes/roles.
- Refresh token opaco almacenado como hash, rotativo y en cookie `HttpOnly`, `Secure` y `SameSite` apropiado.
- Reutilización de refresh token revoca la familia de sesión y genera evento de auditoría.
- Operaciones autenticadas por cookie implementan protección CSRF.
- API Keys se muestran una sola vez; se almacenan como hash, pueden rotarse/revocarse y tienen scopes/expiración. Sus scopes no superan los permisos de la aplicación (TP-48). La rotación crea una clave nueva y revoca la anterior sin solape (TP-52).
- Un usuario creado con contraseña temporal solo puede cambiarla, consultar su perfil, refrescar y cerrar sesión hasta sustituirla (`403 PASSWORD_CHANGE_REQUIRED`, TP-46).
- Ningún secreto de proveedor aparece en responses, frontend, logs o mensajes RabbitMQ.

## 3. Evolution API / WhatsApp

- La plataforma crea una instancia por número y organización usando Evolution centralizado.
- El QR solo es visible a un usuario autorizado, es temporal y no se almacena como secreto permanente.
- El estado de conexión se obtiene de eventos autenticados y/o consulta al proveedor.
- La sesión sobrevive a reinicios controlados de Evolution si la versión/almacenamiento fijados lo permiten; esto se prueba antes de producción.
- Un remitente solo se asigna a aplicaciones de su organización.
- La aplicación puede indicar `senderId`; si lo omite se usa el default activo; si no existe, se rechaza con `422 DEFAULT_SENDER_NOT_CONFIGURED`.
- Un número desconectado no acepta nuevas entregas; se devuelve `422 SENDER_NOT_CONNECTED`.
- La aceptación del proveedor no se interpreta como entrega confirmada: la entrega queda en `PROVIDER_ACCEPTED` y el estado público en `SENT`.
- Resultado ambiguo no genera reenvío ciego: el intento queda `AMBIGUOUS`, la entrega `RECONCILIATION_REQUIRED` y el estado público `PROCESSING` hasta conciliar.
- `DELIVERED`/`READ` solo se registran con evidencia del proveedor; cada transición de entrega queda en `delivery_status_history`.
- Reintento técnico: nuevo intento de la misma entrega, sin consumo de cuota. Reenvío manual: nueva notificación con `resendOfNotificationId`, nueva `Idempotency-Key`, motivo auditado y un consumo de cuota; la original no cambia. Conciliación: no crea notificaciones ni entregas y no consume cuota; al vencer su plazo configurable, la entrega queda en intervención manual sin asumir la no entrega.

## 4. SMS y email

- SMS solo se habilita cuando versión de servidor, app Android, autenticación, endpoint y SIM han sido verificados.
- El gateway SMS está asociado a una sola organización y su salud/última actividad es visible.
- SMTP central y SMTP externo se configuran por organización sin exponer secretos.
- Prueba SMTP distingue conexión/autenticación aceptada de entrega final.
- SPF, DKIM, DMARC y PTR se verifican en DNS antes de habilitar envío productivo desde dominio propio.

## 5. Notificaciones y cuotas

- La creación de notificación es transaccional con reserva de cuota y Outbox.
- `202 Accepted` solo se devuelve después de persistir la solicitud y su evento Outbox.
- Repetición idempotente no crea una segunda notificación ni consume cuota adicional.
- Misma idempotency key con request hash distinto produce `409 IDEMPOTENCY_KEY_CONFLICT`.
- Cuota inicial WhatsApp: 500 mensajes aceptados por día y organización, agregada entre aplicaciones y números.
- El día se calcula en `organizations.timezone` (no existe zona horaria por aplicación en v1); el contador se actualiza atómicamente.
- Límites menores por aplicación no pueden exceder el límite efectivo de organización.
- Validaciones fallidas no consumen cuota; aceptación sí; reintentos técnicos no vuelven a consumirla. Las notificaciones programadas consumen al ejecutarse, en el día local de ejecución (TP-44). En un lote, cada ítem aceptado consume y los rechazados no (TP-43).
- El límite diario de la organización vive en `organization_limits` (WhatsApp 500 por defecto) y solo lo cambia `super_admin` (TP-45).
- Un lote con ítems inválidos responde `202` con el resultado de cada ítem; los válidos se aceptan (TP-43).
- El exceso de cuota devuelve `429` con `WHATSAPP_DAILY_QUOTA_EXCEEDED` (o `APPLICATION_DAILY_LIMIT_EXCEEDED`), `Retry-After` y `retryAt` cuando sean calculables.
- Un límite de aplicación mayor que el de la organización se rechaza con `422 APPLICATION_LIMIT_EXCEEDS_ORGANIZATION_LIMIT`.
- Todos los errores siguen RFC 9457 con `code` del catálogo [error-codes.md](../api/error-codes.md): `400` formato, `409` conflicto de estado/idempotencia, `422` regla de negocio.

## 6. Mensajería resiliente

- Outbox y recurso de negocio se guardan en la misma transacción PostgreSQL.
- Publicador usa publisher confirms y puede publicar más de una vez sin duplicar efectos internos.
- Consumidores usan ack manual e Inbox deduplicada.
- Reintentos usan backoff exponencial con jitter y límite configurado.
- Errores permanentes van a fallo terminal/DLQ; los mensajes DLQ son inspeccionables y reprocesables bajo autorización.
- No se promete exactly-once extremo a extremo.

## 7. Adjuntos y plantillas

- Adjuntos se almacenan en bucket privado; metadata y checksum en PostgreSQL.
- Una notificación puede tener varios adjuntos y un adjunto puede reutilizarse en varias notificaciones de la misma aplicación sin duplicar el archivo en MinIO (`notification_attachments`). Un adjunto de otra organización o aplicación responde `404`; uno eliminado, expirado o bloqueado por el escaneo, `422 ATTACHMENT_NOT_AVAILABLE`.
- Validar tamaño, extensión, MIME real y política de escaneo.
- Las URLs firmadas expiran y no permiten listado público.
- Las versiones publicadas de plantillas son inmutables.
- El cifrado PGP (EMAIL) y ZIP AES-256 (EMAIL/WHATSAPP con adjuntos) se aplica antes del envío. La contraseña ZIP nunca aparece en respuestas, logs ni mensajes de RabbitMQ (TP-47).
- El renderizado valida variables requeridas y escapa contenido según canal.

## 8. Campañas y programación

- Campaña valida audiencia, plantilla, remitente y cuota antes de iniciar. En v1 la plataforma **no** registra consentimientos: el opt-in es responsabilidad de la organización cliente (decisión del 2026-11-15; [risk-register.md](../providers/risk-register.md)).
- Pausa/cancelación detiene nuevos despachos; no se promete cancelar mensajes ya aceptados por proveedor.
- Programación interpreta la hora local en la zona horaria de la **organización**, la convierte a UTC al crear y conserva la zona horaria y el valor local para auditoría.
- El scheduler es idempotente ante reinicios y múltiples réplicas.

## 9. Operación

- Health/readiness distingue aplicación, PostgreSQL, RabbitMQ, Redis, MinIO y proveedores externos.
- Logs estructurados incluyen correlation ID y omiten secretos/PII no necesaria.
- Métricas incluyen cola, latencia, errores, reintentos, cuota, salud de proveedor y desconexiones.
- Backups PostgreSQL y MinIO tienen prueba documentada de restauración.
- Límites de CPU/memoria/disco y alertas se prueban en el VPS antes de producción.

## 10. Condición de salida a producción

- Pruebas automatizadas unitarias, integración, contrato, seguridad multi-tenant y carga completadas.
- Versiones/digests de imágenes fijados; no usar `latest` en producción.
- Secretos del ZIP legado rotados y fuera del repositorio/historial accesible.
- RPO/RTO aprobados y restauración demostrada.
- Contratos reales de Evolution, SMSGate y SMTP validados en entorno de pruebas.
