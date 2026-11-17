# Inventario de endpoints — v1

**Documento derivado.** Resume las operaciones de [openapi.yaml](openapi.yaml), que es la **fuente normativa única** del contrato HTTP v1 ([api-design.md](api-design.md#fuente-de-verdad)). No define contrato propio: ante cualquier discrepancia prevalece `openapi.yaml`, y este inventario se regenera a partir de él.

Operaciones: **104**.

## Authentication API — `/api/v1/management/auth`

| Método | Ruta | `operationId` | Autenticación / permiso |
|---|---|---|---|
| PUT | `/active-organization` | `selectActiveOrganization` | JWT + membresía/rol |
| POST | `/login` | `login` | Credenciales de usuario (sin sesión previa); devuelve `csrfToken` (TP-38) |
| POST | `/logout` | `logout` | `RefreshCookie` (cookie `refresh`) **y** `CsrfHeader` (`X-CSRF-Token`) (TP-37) |
| GET | `/me` | `getCurrentUser` | JWT + membresía/rol |
| POST | `/password` | `changePassword` | JWT del propio usuario; admitido con `mustChangePassword` (TP-46) |
| POST | `/refresh` | `refreshSession` | `RefreshCookie` (cookie `refresh`) **y** `CsrfHeader` (`X-CSRF-Token`) (TP-37); devuelve un `csrfToken` nuevo (TP-38) |

## Public API — `/api/v1/public`

La columna de permiso indica el scope de API Key que la operación declara en `x-required-scopes` (catálogo cerrado `ApiKeyScope`).

| Método | Ruta | `operationId` | Autenticación / permiso |
|---|---|---|---|
| POST | `/attachments` | `uploadAttachment` | `attachments.upload` |
| GET | `/attachments/{attachmentId}` | `getAttachment` | `attachments.read` (TP-28; solo adjuntos de su organización y aplicación) |
| GET | `/notifications` | `listNotifications` | `notifications.read` |
| POST | `/notifications` | `createNotification` | `notifications.send` |
| POST | `/notifications/bulk` | `createBulkNotifications` | `notifications.send` |
| GET | `/notifications/{notificationId}` | `getNotificationById` | `notifications.read` |
| POST | `/notifications/{notificationId}/cancel` | `cancelNotification` | `notifications.cancel` |
| GET | `/notifications/{notificationId}/events` | `listNotificationEvents` | `notifications.read` |
| POST | `/notifications/{notificationId}/retry` | `retryNotification` | `notifications.retry` (actor: la aplicación, TP-21) |
| GET | `/templates` | `listPublicTemplates` | `templates.read` |
| GET | `/templates/{code}` | `getPublicTemplate` | `templates.read` |

## Management API — `/api/v1/management`

| Método | Ruta | `operationId` | Autenticación / permiso |
|---|---|---|---|
| GET | `/applications` | `listApplications` | JWT + membresía/rol |
| POST | `/applications` | `createApplication` | JWT + membresía/rol |
| GET | `/applications/{applicationId}/api-keys` | `listApiKeys` | JWT + membresía/rol |
| POST | `/applications/{applicationId}/api-keys` | `createApiKey` | JWT + membresía/rol |
| DELETE | `/applications/{applicationId}/api-keys/{keyId}` | `revokeApiKey` | JWT + membresía/rol |
| POST | `/applications/{applicationId}/api-keys/{keyId}/rotate` | `rotateApiKey` | JWT + membresía/rol |
| PUT | `/applications/{applicationId}/limits` | `setApplicationLimits` | JWT + membresía/rol |
| PUT | `/applications/{applicationId}/permissions` | `setApplicationPermissions` | JWT + membresía/rol |
| PUT | `/applications/{applicationId}/senders` | `assignApplicationSenders` | JWT + membresía/rol |
| PATCH | `/applications/{applicationId}/status` | `updateApplicationStatus` | JWT + membresía/rol |
| GET | `/applications/{resourceId}` | `getApplication` | JWT + membresía/rol |
| PUT | `/applications/{resourceId}` | `updateApplication` | JWT + membresía/rol |
| GET | `/audit/events` | `listAuditEvents` | JWT + membresía/rol |
| GET | `/audit/notifications/{notificationId}` | `getNotificationAudit` | JWT + membresía/rol |
| GET | `/campaigns` | `listCampaigns` | JWT + membresía/rol |
| POST | `/campaigns` | `createCampaign` | JWT + membresía/rol |
| POST | `/campaigns/{campaignId}/cancel` | `cancelCampaign` | JWT + membresía/rol |
| POST | `/campaigns/{campaignId}/pause` | `pauseCampaign` | JWT + membresía/rol |
| GET | `/campaigns/{campaignId}/recipients` | `listCampaignRecipients` | JWT + membresía/rol |
| POST | `/campaigns/{campaignId}/start` | `startCampaign` | JWT + membresía/rol |
| GET | `/campaigns/{campaignId}/statistics` | `getCampaignStatistics` | JWT + membresía/rol |
| GET | `/campaigns/{resourceId}` | `getCampaign` | JWT + membresía/rol |
| PUT | `/campaigns/{resourceId}` | `updateCampaign` | JWT + membresía/rol |
| GET | `/dashboard/consumption` | `getDashboardConsumption` | JWT + membresía/rol |
| GET | `/dashboard/deliveries` | `getDashboardDeliveries` | JWT + membresía/rol |
| GET | `/dashboard/providers-health` | `getDashboardProvidersHealth` | JWT + membresía/rol |
| GET | `/dashboard/summary` | `getDashboardSummary` | JWT + membresía/rol |
| POST | `/notifications/{notificationId}/cancel` | `cancelManagementNotification` | JWT + membresía activa + permiso de rol `notifications.cancel`; `super_admin` global, auditado (TP-36) |
| GET | `/organizations` | `listOrganizations` | JWT + membresía/rol |
| POST | `/organizations` | `createOrganization` | JWT + membresía/rol |
| GET | `/organizations/{organizationId}/memberships` | `listMemberships` | JWT + membresía/rol |
| POST | `/organizations/{organizationId}/memberships` | `createMembership` | JWT + membresía/rol |
| PUT | `/organizations/{organizationId}/memberships/{membershipId}/roles` | `setMembershipRoles` | JWT + membresía/rol |
| PATCH | `/organizations/{organizationId}/memberships/{membershipId}/status` | `updateMembershipStatus` | JWT + membresía/rol |
| PATCH | `/organizations/{organizationId}/status` | `updateOrganizationStatus` | JWT + membresía/rol |
| GET | `/organizations/{resourceId}` | `getOrganization` | JWT + membresía/rol |
| PUT | `/organizations/{resourceId}` | `updateOrganization` | JWT + membresía/rol |
| GET | `/providers/accounts` | `listProviderAccounts` | JWT + membresía/rol |
| POST | `/providers/accounts` | `createProviderAccount` | JWT + membresía/rol |
| POST | `/providers/accounts/{providerAccountId}/credentials/rotate` | `rotateProviderCredentials` | JWT + membresía/rol |
| POST | `/providers/accounts/{providerAccountId}/disable` | `disableProviderAccount` | JWT + membresía/rol |
| POST | `/providers/accounts/{providerAccountId}/enable` | `enableProviderAccount` | JWT + membresía/rol |
| GET | `/providers/accounts/{providerAccountId}/health` | `getProviderHealth` | JWT + membresía/rol |
| GET | `/providers/accounts/{providerAccountId}/instances` | `listProviderInstances` | JWT + membresía/rol |
| POST | `/providers/accounts/{providerAccountId}/instances` | `createProviderInstance` | JWT + membresía/rol |
| POST | `/providers/accounts/{providerAccountId}/test` | `testProviderAccount` | JWT + membresía/rol |
| GET | `/providers/accounts/{resourceId}` | `getProviderAccount` | JWT + membresía/rol |
| PUT | `/providers/accounts/{resourceId}` | `updateProviderAccount` | JWT + membresía/rol |
| DELETE | `/providers/accounts/{resourceId}` | `deleteProviderAccount` | JWT + membresía/rol |
| GET | `/providers/definitions` | `listProviderDefinitions` | JWT + membresía/rol |
| DELETE | `/providers/instances/{instanceId}` | `deleteProviderInstance` | JWT + membresía/rol |
| POST | `/providers/instances/{instanceId}/connect` | `connectProviderInstance` | JWT + membresía/rol |
| POST | `/providers/instances/{instanceId}/disconnect` | `disconnectProviderInstance` | JWT + membresía/rol |
| GET | `/providers/instances/{instanceId}/qr` | `getProviderInstanceQr` | JWT + membresía/rol |
| POST | `/providers/instances/{instanceId}/reconnect` | `reconnectProviderInstance` | JWT + membresía/rol |
| GET | `/providers/instances/{instanceId}/status` | `getProviderInstanceStatus` | JWT + membresía/rol |
| GET | `/roles` | `listRoles` | JWT + membresía/rol |
| GET | `/roles/{resourceId}` | `getRole` | JWT + membresía/rol |
| PUT | `/roles/{roleId}/permissions` | `setSystemRolePermissions` | JWT + `super_admin` activo |
| GET | `/scheduled-notifications` | `listScheduledNotifications` | JWT + membresía/rol |
| POST | `/scheduled-notifications` | `createScheduledNotification` | JWT + membresía/rol |
| GET | `/scheduled-notifications/{scheduleId}` | `getScheduledNotification` | JWT + membresía/rol |
| DELETE | `/scheduled-notifications/{scheduleId}` | `cancelScheduledNotification` | JWT + membresía/rol |
| GET | `/senders` | `listSenders` | JWT + membresía/rol |
| GET | `/senders/{senderId}` | `getSender` | JWT + membresía/rol |
| GET | `/templates` | `listTemplates` | JWT + membresía/rol |
| POST | `/templates` | `createTemplate` | JWT + membresía/rol |
| GET | `/templates/{resourceId}` | `getTemplate` | JWT + membresía/rol |
| PUT | `/templates/{resourceId}` | `updateTemplate` | JWT + membresía/rol |
| DELETE | `/templates/{resourceId}` | `deleteTemplate` | JWT + membresía/rol |
| POST | `/templates/{templateId}/preview` | `previewTemplate` | JWT + membresía/rol |
| POST | `/templates/{templateId}/test` | `testTemplate` | JWT + membresía/rol |
| GET | `/templates/{templateId}/versions` | `listTemplateVersions` | JWT + membresía/rol |
| POST | `/templates/{templateId}/versions` | `createTemplateVersion` | JWT + membresía/rol |
| GET | `/users` | `listUsers` | JWT + membresía/rol |
| POST | `/users` | `createUser` | JWT + membresía/rol |
| GET | `/users/{resourceId}` | `getUser` | JWT + membresía/rol |
| PUT | `/users/{resourceId}` | `updateUser` | JWT + membresía/rol |
| PATCH | `/users/{userId}/status` | `updateUserStatus` | JWT + membresía/rol |

## Platform API — `/api/v1/platform`

| Método | Ruta | `operationId` | Autenticación / permiso |
|---|---|---|---|
| GET | `/organizations/{organizationId}/limits` | `getOrganizationLimits` | JWT + `super_admin` activo (TP-45) |
| PUT | `/organizations/{organizationId}/limits` | `setOrganizationLimits` | JWT + `super_admin` activo; motivo auditado (TP-45) |
| GET | `/super-admins` | `listSuperAdminAssignments` | JWT + `super_admin` activo |
| POST | `/super-admins` | `grantSuperAdmin` | JWT + `super_admin` activo |
| POST | `/super-admins/{assignmentId}/revoke` | `revokeSuperAdmin` | JWT + `super_admin` activo |

## Webhook API — `/api/v1/webhooks`

| Método | Ruta | `operationId` | Autenticación / permiso |
|---|---|---|---|
| POST | `/email/{providerAccountId}` | `receiveEmailWebhook` | Firma/secreto del proveedor |
| POST | `/sms/{providerAccountId}` | `receiveSmsWebhook` | Firma/secreto del proveedor |
| POST | `/whatsapp/evolution/{providerAccountId}` | `receiveEvolutionWebhook` | Firma/secreto del proveedor |

## Notas

- `createBulkNotifications` acepta parcialmente (TP-43). `createTemplate`, `createCampaign` y `createScheduledNotification` exigen `applicationId` (TP-53). Mientras un usuario tenga `mustChangePassword`, solo `getCurrentUser`, `changePassword`, `refreshSession` y `logout` le responden (TP-46).
- Los permisos de rol de la Management API (`RolePermissionCode`) están en [rbac-matrix.md](../security/rbac-matrix.md#catálogo-de-permisos-de-rol-tp-51).

- La cancelación tiene dos puntos de entrada que reutilizan el **mismo caso de uso** (TP-36): `cancelNotification` (Public API, API Key con scope `notifications.cancel`, actor `APPLICATION`) y `cancelManagementNotification` (Management API, JWT, actor `USER`). Ambas responden igual: ya cancelada → `409 RESOURCE_STATE_CONFLICT`; ya despachada → `409 NOTIFICATION_ALREADY_DISPATCHED` (TP-39).
- Las colecciones paginadas devuelven `items`, `page`, `pageSize` y `total` (TP-34); `total` cuenta los resultados que cumplen los filtros antes de paginar.

- Los roles de organización se asignan solo por membresía; no existe asignación directa de roles a un usuario. Las operaciones `POST /management/roles`, `PUT /management/roles/{roleId}` y `PUT /management/users/{userId}/roles` están **retiradas** (TP-16/TP-20).
- Los métodos, cabeceras y esquemas de webhook deben alinearse con las versiones exactas de cada proveedor instaladas. No normalizar el payload externo antes de verificar su autenticidad; guardar un evento bruto controlado y luego transformarlo en un evento interno.
