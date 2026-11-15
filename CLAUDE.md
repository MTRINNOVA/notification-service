@AGENTS.md

# Instrucciones adicionales para Claude Code

Las reglas del proyecto están en `AGENTS.md` (importado arriba). Este archivo solo añade cómo debe trabajar Claude Code.

## Antes de modificar código

- Leer los documentos de `docs/` relacionados con la tarea (ver §4 de `AGENTS.md`) y el código existente afectado. No suponer contenido sin abrir el archivo.
- Confirmar la fase y el alcance solicitados. Si la solicitud es ambigua o choca con una decisión aprobada o con una decisión pendiente de §19 (`docs/project/PENDING-DECISIONS.md`), preguntar antes de implementar.
- No cambiar decisiones aprobadas, añadir paquetes ni ampliar el alcance sin autorización explícita en la conversación. La autorización de una tarea no se extiende a la siguiente.

## Durante el trabajo

- Cambios mínimos y focalizados; no reformatear ni "mejorar" archivos fuera del alcance.
- No hacer commits, push ni crear ramas salvo que se pida.
- No ejecutar migraciones, `dotnet ef database update` ni comandos contra entornos o servicios reales.

## Informe al terminar

Cada entrega incluye:

1. **Archivos modificados/creados**, con una línea sobre el motivo de cada uno.
2. **Comandos ejecutados y su resultado real** (p.ej. `dotnet build`: 0 errores, 0 advertencias; `dotnet test`: N superadas, M fallidas).
3. **Pendientes**: lo que quedó incompleto, problemas detectados fuera de alcance y decisiones que requieren aprobación.

Nunca afirmar que se ejecutó una prueba, build o validación que no se ejecutó. Si un comando no pudo ejecutarse o falló, decirlo y mostrar la salida relevante.
