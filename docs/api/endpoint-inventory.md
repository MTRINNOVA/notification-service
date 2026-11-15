# Inventario de endpoints — contrato funcional

Este inventario complementa `openapi.yaml`. El YAML de esta versión contiene esquemas y operaciones representativas para fijar convenciones; antes de implementar cada módulo, todas las rutas de este inventario deben incorporarse al OpenAPI versionado y revisarse mediante contract review.

## Authentication API — `/api/v1/management/auth`

- `POST /login`
- `POST /refresh`
- `POST /logout`
- `GET /me`
- `PUT /active-organization`

## Public API — `/api/v1/public`

| Método | Ruta                                                            | Permiso                |
| ------ | --------------------------------------------------------------- | ---------------------- |
| POST   | `/notifications`                                                | `notifications.send`   |
| POST   | `/notifications/bulk`                                           | `notifications.send`   |
| GET    | `/notifications`                                                | `notifications.read`   |
| GET    | `/notifications/{notificationId}`                               | `notifications.read`   |
| GET    | `/notifications/{notificationId}/events`                        | `notifications.read`   |
| POST   | `/notifications/{notificationId}/cancel`                        | `notifications.cancel` |
| POST   | `/notifications/{notificationId}/retry` (reenvío manual, TP-19) | `notifications.retry`  |
| POST   | `/attachments`                                                  | `attachments.upload`   |
| GET    | `/templates`                                                    | `templates.read`       |
| GET    | `/templates/{code}`                                             | `templates.read`       |

## Management API — `/api/v1/management`

### Organizaciones

- `GET /organizations`
- `POST /organizations`
- `GET /organizations/{organizationId}`
- `PUT /organizations/{organizationId}`
- `PATCH /organizations/{organizationId}/status`

### Aplicaciones

- `GET /applications`
- `POST /applications`
- `GET /applications/{applicationId}`
- `PUT /applications/{applicationId}`
- `PATCH /applications/{applicationId}/status`
- `PUT /applications/{applicationId}/permissions`
- `PUT /applications/{applicationId}/limits`

### API Keys

- `GET /applications/{applicationId}/api-keys`
- `POST /applications/{applicationId}/api-keys`
- `POST /applications/{applicationId}/api-keys/{keyId}/rotate`
- `DELETE /applications/{applicationId}/api-keys/{keyId}`

### Usuarios y roles

- `GET /organizations/{organizationId}/memberships`, `POST /organizations/{organizationId}/memberships`
- `PUT /organizations/{organizationId}/memberships/{membershipId}/roles`
- `GET /users`, `POST /users`, `GET /users/{userId}`, `PUT /users/{userId}`, `PATCH /users/{userId}/status`
- `GET /roles`, `GET /roles/{roleId}`: catálogo de roles del sistema, solo lectura (TP-20)
- `PUT /roles/{roleId}/permissions`: solo `super_admin`, auditado
- Los roles se asignan únicamente por membresía. `POST /roles`, `PUT /roles/{roleId}` y `PUT /users/{userId}/roles` quedan **retirados** (TP-16/TP-20)

### Catálogo y cuentas de proveedores

- `GET /providers/definitions`
- `GET /providers/accounts`
- `POST /providers/accounts`
- `GET /providers/accounts/{providerAccountId}`
- `PUT /providers/accounts/{providerAccountId}`
- `DELETE /providers/accounts/{providerAccountId}` (deshabilitación lógica por defecto)
- `POST /providers/accounts/{providerAccountId}/test`
- `GET /providers/accounts/{providerAccountId}/health`
- `POST /providers/accounts/{providerAccountId}/enable`
- `POST /providers/accounts/{providerAccountId}/disable`
- `POST /providers/accounts/{providerAccountId}/credentials/rotate`

### Instancias WhatsApp

- `GET /providers/accounts/{providerAccountId}/instances`
- `POST /providers/accounts/{providerAccountId}/instances`
- `POST /providers/instances/{instanceId}/connect`
- `GET /providers/instances/{instanceId}/qr`
- `GET /providers/instances/{instanceId}/status`
- `POST /providers/instances/{instanceId}/disconnect`
- `POST /providers/instances/{instanceId}/reconnect`
- `DELETE /providers/instances/{instanceId}`

### Remitentes

- `GET /senders`, `GET /senders/{senderId}`
- `PUT /applications/{applicationId}/senders`

### Plantillas

- `GET /templates`
- `POST /templates`
- `GET /templates/{templateId}`
- `PUT /templates/{templateId}`
- `DELETE /templates/{templateId}` (deshabilitación lógica)
- `POST /templates/{templateId}/versions`
- `GET /templates/{templateId}/versions`
- `POST /templates/{templateId}/preview`
- `POST /templates/{templateId}/test`

### Campañas y programación

- `GET /campaigns`, `POST /campaigns`, `GET /campaigns/{campaignId}`, `PUT /campaigns/{campaignId}`
- `POST /campaigns/{campaignId}/start`
- `POST /campaigns/{campaignId}/pause`
- `POST /campaigns/{campaignId}/cancel`
- `GET /campaigns/{campaignId}/statistics`
- `GET /campaigns/{campaignId}/recipients`
- `POST /scheduled-notifications`
- `GET /scheduled-notifications`
- `GET /scheduled-notifications/{scheduleId}`
- `DELETE /scheduled-notifications/{scheduleId}`

### Dashboard y auditoría

- `GET /dashboard/summary`
- `GET /dashboard/deliveries`
- `GET /dashboard/providers-health`
- `GET /dashboard/consumption`
- `GET /audit/events`
- `GET /audit/notifications/{notificationId}`

## Platform API — `/api/v1/platform`

Solo `super_admin` activo; operaciones auditadas (TP-15).

- `GET /super-admins`
- `POST /super-admins`
- `POST /super-admins/{assignmentId}/revoke`

## Webhook API — `/api/v1/webhooks`

- `POST /whatsapp/evolution/{providerAccountId}`
- `POST /email/{providerAccountId}`
- `POST /sms/{providerAccountId}`

Los métodos, headers y esquemas de webhook deben alinearse con las versiones exactas de cada proveedor instaladas. No normalizar el payload externo antes de verificar autenticidad; guardar un evento bruto controlado y luego transformarlo a un evento interno.
