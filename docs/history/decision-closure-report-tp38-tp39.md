# Informe de cierre documental TP-38 y TP-39 — 2026-11-15

> **Documento histórico** (evidencia de la tarea del 2026-11-15). No es fuente operativa: el estado vigente está en [technical-pending-register.md](../operations/technical-pending-register.md) y las decisiones en [DECISIONS-v1.5.md](../project/DECISIONS-v1.5.md).

**La Fase 2 no ha comenzado. No se ha implementado código funcional.**

## 1. Objetivo

Incorporar a la documentación normativa dos decisiones aprobadas por el usuario:

- **TP-38:** el mecanismo de emisión y verificación del token CSRF.
- **TP-39:** el resultado de cancelar una notificación ya cancelada.

El trabajo es exclusivamente documental: no incluye ninguna implementación.

## 2. Estado inicial

Ambos puntos se abrieron en la tarea de cierre de TP-28 a TP-37 ([decision-closure-report-tp28-tp37.md](decision-closure-report-tp28-tp37.md)), en estado `Requiere aprobación`:

- **TP-38:** TP-37 fijó el nombre de la cabecera `X-CSRF-Token`, pero no cómo obtiene el panel su valor ni cómo lo verifica el backend. Además, la documentación decía que el CSRF se validaba "antes de leer el refresh token", lo que es incompatible con un token ligado a la sesión.
- **TP-39:** `cancelManagementNotification` documentaba `409 RESOURCE_STATE_CONFLICT` para una notificación ya cancelada, pero `cancelNotification` (Public API) solo declaraba `NOTIFICATION_ALREADY_DISPATCHED`.

Punto de partida: rama `main`, commit `f7b1f3a`, con los cambios de la tarea TP-28..TP-37 aún sin confirmar. Antes de editar se guardó una copia de esos documentos para comparar los cambios.

## 3. Decisiones aprobadas

| ID | Decisión | Estado documental | Estado de implementación |
|---|---|---|---|
| TP-38 | Token CSRF sincronizado con la sesión | Cerrado | Pendiente |
| TP-39 | Cancelación repetida HTTP 409 | Cerrado | Pendiente |

- **TP-38.** El backend genera el token con un generador criptográficamente seguro y guarda su hash asociado a la sesión. Login y refresh lo devuelven en el JSON (`csrfToken`); cada refresh emite uno nuevo y el anterior deja de valer. El panel lo conserva en memoria y lo envía en `X-CSRF-Token`. El backend lo compara con el de la sesión y valida `Origin`. No se usa el doble envío con cookie legible.
- **TP-39.** Cancelar una notificación ya cancelada responde `409 RESOURCE_STATE_CONFLICT`, con la misma regla en la Public API y en la Management API. No es una operación idempotente con `200`.

## 4. Cambios documentales

| Archivo | Cambio |
|---|---|
| `docs/api/openapi.yaml` | `LoginResponse.csrfToken` (requerido), con descripción de la respuesta. `login`: nueva descripción (emisión de la sesión, la cookie y `csrfToken`; sin CSRF). `refreshSession` y `logout`: flujo de validación (Origin → sesión → comparación con el hash), rotación del token CSRF y `detail` genérico. `CsrfHeader`: descripción del token sincronizado. `cancelNotification`: descripción con los dos 409. `cancelManagementNotification`: referencia a TP-39 |
| `docs/adr/0011-admin-identity.md` | Nueva sección «Token CSRF sincronizado con la sesión (TP-38)»: flujos, reglas de seguridad y puntos sin definir. Corregida la fila CSRF de TP-37: la sesión se obtiene antes de comparar el token |
| `docs/api/admin-panel-auth-contract.md` | **Nuevo.** Contrato que consume `notification-admin`. No existía documentación de integración del frontend en este repositorio, solo referencias al otro repositorio en el índice |
| `docs/api/api-design.md` | Tabla de resultados de cancelación, idéntica en ambas APIs; enlace al contrato del panel |
| `docs/api/error-codes.md` | `PERMISSION_DENIED`: fallo CSRF u `Origin`, con `detail` genérico. `RESOURCE_STATE_CONFLICT`: cancelación repetida en ambas APIs |
| `docs/api/endpoint-inventory.md` | Login y refresh devuelven `csrfToken`; nota de cancelación con los dos 409 |
| `docs/security/security-model.md` | Resumen del mecanismo TP-38 |
| `docs/project/DECISIONS-v1.5.md` | Tabla de TP-38 y TP-39 |
| `docs/operations/technical-pending-register.md` | TP-38 y TP-39 en «Decisiones aprobadas y especificadas documentalmente», con su texto anterior como trazabilidad; nuevos TP-40, TP-41 y TP-42 |
| `docs/project/notification-architecture-index.md` | Contrato del panel y este informe |
| `AGENTS.md` | §13 (mecanismo CSRF) y §19 (TP-38 y TP-39 aprobados; puntos abiertos TP-40 a TP-42) |

## 5. Contrato CSRF entre backend y frontend

Detalle completo en [admin-panel-auth-contract.md](../api/admin-panel-auth-contract.md).

- **Login** devuelve `csrfToken` y `accessToken` en el JSON. El refresh token llega solo en la cookie `refresh` (`HttpOnly`, `Secure` en producción, `SameSite=Strict`, `Path=/api/v1/management/auth`).
- El panel guarda `csrfToken` **solo en memoria**: nunca en `localStorage`, `sessionStorage`, cookies ni URLs.
- **Refresh** y **logout** envían la cookie (petición con credenciales) y `X-CSRF-Token` con el token vigente. Tras un refresh correcto, el panel sustituye `csrfToken` y `accessToken`.
- Las llamadas de negocio usan `Authorization: Bearer` y **no** requieren `X-CSRF-Token`. La protección CSRF no es global.
- **Errores:** `401 AUTHENTICATION_REQUIRED` (sesión no válida) o `403 PERMISSION_DENIED` (CSRF u `Origin`) llevan al login, sin reintentar con el mismo token. El `detail` es genérico.
- **Reglas derivadas** (concreción de "no reintentar indefinidamente" y de la detección de reutilización ya aprobada):
  - ante un `401` en una llamada de negocio, el panel hace como máximo un refresh y un reintento;
  - el panel no lanza dos refresh en paralelo, porque el segundo presentaría un refresh token ya usado y provocaría la revocación de la familia.

## 6. Contrato de cancelación (Public y Management)

| Situación | HTTP | `code` |
|---|---:|---|
| Cancelación correcta | 200 | `ActionResult` (sin cambios) |
| Ya cancelada | 409 | `RESOURCE_STATE_CONFLICT` |
| Ya despachada | 409 | `NOTIFICATION_ALREADY_DISPATCHED` |
| Inexistente o fuera del ámbito autorizado | 404 | `RESOURCE_NOT_FOUND` (sin cambios) |

- Autenticación, scope (`notifications.cancel`), permisos de Management y conjunto de respuestas declaradas: sin cambios en ninguna de las dos operaciones.
- La semántica de los demás estados tampoco cambia (ADR-0012).

## 7. Validaciones ejecutadas

Herramientas: PyYAML 6.0.3 y `openapi-spec-validator` 0.9.0 en un entorno virtual del scratchpad, fuera del repositorio, más scripts propios.

| Validación | Resultado |
|---|---|
| Batería de contrato de TP-28..TP-37, repetida | **175 OK, 0 fallos**: YAML válido; OpenAPI 3.1.0 válido; 101 operaciones y 79 rutas con `operationId` únicos; 645 `$ref` sin ninguno roto; 0 esquemas sin uso; inventario = OpenAPI; scopes = RBAC; paginación, cookie y CSRF |
| `operationId` estables | Las mismas 101 operaciones, con las mismas rutas y métodos que antes de la tarea |
| `csrfToken` | Definido y requerido en `LoginResponse`, que es la respuesta `200` de `login` y de `refreshSession` |
| Refresh/logout | Conservan `RefreshCookie` AND `CsrfHeader`; `login` sin CSRF; ninguna otra operación usa `CsrfHeader`; sin seguridad global; `RefreshCookie` sin cambios |
| Cancelación | Ambas operaciones declaran el 409 con `RESOURCE_STATE_CONFLICT` y `NOTIFICATION_ALREADY_DISPATCHED`; autenticación, scope, respuestas y `200` sin cambios |
| Alcance de los cambios en OpenAPI | Solo cambiaron `login`, `refreshSession`, `logout`, `cancelNotification` y `cancelManagementNotification`, el esquema `LoginResponse` y la descripción de `CsrfHeader`. `components.responses` sin cambios |
| Catálogo de errores | Contiene `RESOURCE_STATE_CONFLICT` (409) |
| Registro | TP-38 y TP-39 en "Aprobado — especificado documentalmente" (no implementados); las filas de TP-05..TP-07, TP-28 y TP-33..TP-37 son idénticas a las anteriores |
| Contradicciones | Búsqueda en los documentos normativos de: CSRF validado antes de la sesión, doble envío no negado, almacenamiento del token en el navegador, cancelación idempotente y TP-38 "pendiente de definir". **0 vigentes.** La única mención a "idempotente (`200`)" es el texto antiguo de TP-39, conservado como trazabilidad |
| Enlaces relativos y anclas | 55 archivos Markdown, 457 enlaces y anclas, **0 rotos** (ejecución final, con este informe ya creado) |
| Mermaid | No se modificó ningún diagrama: los 7 bloques son idénticos a `HEAD`. El parser oficial no se ejecutó (su descarga con `npx` fue denegada en la tarea anterior) |
| Código sin cambios | `git status`: ningún archivo fuera de `docs/`, `AGENTS.md` y `README.md` |

No se ejecutaron `dotnet build` ni `dotnet test`: no cambió código, y la tarea limitaba las validaciones al alcance documental.

## 8. Documentos afectados

- **Modificados (10):** `docs/api/openapi.yaml`, `docs/adr/0011-admin-identity.md`, `docs/api/api-design.md`, `docs/api/error-codes.md`, `docs/api/endpoint-inventory.md`, `docs/security/security-model.md`, `docs/project/DECISIONS-v1.5.md`, `docs/operations/technical-pending-register.md`, `docs/project/notification-architecture-index.md` y `AGENTS.md`.
- **Creados (2):** `docs/api/admin-panel-auth-contract.md` y este informe.
- **Sin cambios:** `rbac-matrix.md`, porque permisos y scopes no cambian; `decision-closure-report-tp28-tp37.md`, que es histórico y conserva el estado de entonces; ERD; ADR-0012.

## 9. Pendientes de implementación

- **TP-38:**
  - generación, hash y verificación del token en el backend;
  - rotación junto con el refresh token;
  - invalidación en logout, revocación y reutilización;
  - validación de `Origin`;
  - consumo en `notification-admin`.

  Depende de TP-40.
- **TP-39:** respuesta `409 RESOURCE_STATE_CONFLICT` en el caso de uso de cancelación compartido (TP-36), con pruebas en ambas APIs.

## 10. Riesgos y decisiones adicionales identificadas

Registrados como `Requiere aprobación`. No se inventó ninguna respuesta:

- **TP-40: parámetros sin definir.**
  - algoritmo de hash;
  - longitud y entropía mínima del token;
  - ubicación del hash en el modelo lógico: el ERD no tiene dónde guardarlo y no se modificó porque la tarea prohibía tocar modelos de datos;
  - tratamiento de una petición sin `Origin`;
  - si el login también valida `Origin`.
- **TP-41: recarga de página.** Con el token solo en memoria y el refresh exigiéndolo, recargar la página o abrir otra pestaña obliga a iniciar sesión de nuevo. TP-38 prohíbe las alternativas habituales (cookie legible o almacenamiento del navegador), así que hace falta aceptar ese comportamiento o aprobar otro mecanismo.
- **TP-42: `401` frente a `403`.** El contrato mantiene `401` para la sesión y `403` para CSRF u `Origin`, con `detail` genérico. El código HTTP sigue distinguiendo si falló la sesión o el token. El riesgo es bajo, porque una página atacante no lee respuestas de otro origen, pero la regla "no revelar" de TP-38 no lo resuelve de forma explícita.

**Ejecución final** (con este informe ya creado): enlaces relativos y anclas, 55 archivos Markdown y 457 enlaces, **0 rotos**; 7 bloques Mermaid idénticos a `HEAD`; `git status` sin cambios fuera de `docs/`, `AGENTS.md` y `README.md`.
