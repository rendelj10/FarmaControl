# FarmaControl — Pruebas del módulo Categorías

## 1. Información general

**Proyecto:** Sistema de gestión de farmacia FarmaControl  
**Responsable:** Integrante 1 — Backend  
**Fecha:** 14 de noviembre de 2026  
**Rama:** `feature/categorias-backend`

## 2. Objetivo

Verificar el funcionamiento de las operaciones CRUD y la validación de nombres duplicados del módulo Categorías mediante ASP.NET Core MVC, Entity Framework Core y SQL Server.

## 3. Entorno de pruebas

- **Lenguaje:** C#.
- **Framework:** ASP.NET Core MVC (.NET 10).
- **Base de datos:** `FarmaControlDB`.
- **Motor:** SQL Server Express.
- **Herramienta:** Developer PowerShell.
- **API local:** `https://localhost:7171/api/categorias`.

## 4. Resultados

| Prueba | Resultado observado | Estado |
|---|---|---|
| GET — Consultar categorías | Respuesta inicial `[]` | Aprobada |
| POST — Crear categoría | Se registró la categoría con ID 1 | Aprobada |
| PUT — Actualizar categoría | Se actualizaron el nombre y la descripción | Aprobada |
| DELETE — Desactivar categoría | El campo `activo` cambió a `false` | Aprobada |
| POST — Nombre duplicado | Respuesta HTTP `409 Conflict` | Aprobada |

## 5. Detalle de las pruebas

### Prueba 01 — Consulta GET

Se realizó una solicitud GET a `/api/categorias`.

**Resultado:** respuesta JSON `[]`, correspondiente a una lista de categorías inicialmente vacía.

### Prueba 02 — Registro POST

Se registró una categoría con los siguientes datos:

- **ID:** 1.
- **Nombre:** Analgesicos.
- **Descripción:** Medicamentos para aliviar el dolor.
- **Activo:** `true`.

**Resultado:** registro exitoso.

### Prueba 03 — Actualización PUT

Se modificó la categoría con ID 1:

- **Nuevo nombre:** Analgesicos y antipireticos.
- **Nueva descripción:** Medicamentos para aliviar el dolor y reducir la fiebre.

Una consulta GET posterior confirmó los cambios y el estado activo.

**Resultado:** actualización exitosa.

### Prueba 04 — Desactivación DELETE

Se ejecutó DELETE sobre `/api/categorias/1`.

La consulta GET posterior confirmó que la categoría permanecía registrada con `activo = false`.

**Resultado:** desactivación lógica exitosa.

### Prueba 05 — Validación de duplicados

Se registró la categoría **Vitaminas Prueba** con ID 2.

Al intentar registrarla nuevamente con el mismo nombre, el servidor respondió HTTP `409 Conflict`. Se repitió el intento y se obtuvo la misma respuesta.

**Resultado:** validación de nombres duplicados exitosa.

## 6. Conclusiones

Las cinco pruebas realizadas produjeron los resultados esperados. Se comprobó el funcionamiento básico del CRUD de categorías y la validación de nombres duplicados.

Quedan pendientes pruebas adicionales de autorización, campos obligatorios, límites de longitud y concurrencia.

## 7. Evidencias

- Salidas de Developer PowerShell de las solicitudes GET, POST, PUT y DELETE.
- Respuestas HTTP `409 Conflict` ante solicitudes duplicadas.
- Capturas de las pruebas realizadas.
- Commit de documentación original: `755fe4e`.
- Revisión e integración mediante Pull Request hacia `develop`: pendientes de confirmar.

## 8. Próximas actividades

- Implementar y verificar validaciones adicionales.
- Integrar el módulo con el Frontend.
- Desarrollar la gestión de medicamentos.
- Implementar autenticación y permisos por roles.
