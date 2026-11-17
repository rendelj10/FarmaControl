# FarmaControl - Pruebas del módulo Medicamentos



## 1. Objetivo



Documentar las pruebas realizadas al módulo Medicamentos para verificar las operaciones de registro, consulta, actualización y desactivación, así como las validaciones implementadas.



## 2. Entorno de pruebas



- **Proyecto:** Sistema de gestión de farmacia FarmaControl.

- **Framework:** ASP.NET Core MVC (.NET 10).

- **Lenguaje:** C#.

- **Base de datos:** SQL Server.

- **ORM:** Entity Framework Core.

- **Base de datos utilizada:** FarmaControlDB.

- **Herramienta de pruebas:** PowerShell mediante Invoke-RestMethod.

- **Dirección local:** https://localhost:7171.

- **Rama de trabajo:** feature/medicamentos-backend.



## 3. Pruebas realizadas



### 3.1. Consulta de medicamentos (GET)



Se realizó una consulta al endpoint `/api/medicamentos` para comprobar la recuperación de los registros almacenados.



**Resultado:** La consulta se ejecutó correctamente. Inicialmente no existían medicamentos registrados.



### 3.2. Registro de medicamentos (POST)



Se registró un medicamento utilizando una categoría activa existente.



**Datos utilizados:**



- Código: MED001.

- Nombre: Paracetamol.

- Principio activo: Paracetamol.

- Concentración: 500 mg.

- Forma farmacéutica: Tableta.

- Presentación: Caja de 100 tabletas.

- Categoría: Analgesicos.



**Resultado:** Registro exitoso. La API devolvió el identificador 1 y los datos del medicamento, incluyendo el nombre de su categoría.



### 3.3. Consulta por identificador (GET)



Se consultó el medicamento mediante el endpoint `/api/medicamentos/1`.



**Resultado:** Se recuperaron correctamente los datos del medicamento y su categoría relacionada.



### 3.4. Actualización de medicamentos (PUT)



Se modificó el campo presentación del medicamento registrado.



**Cambio realizado:** Caja de 100 tabletas - actualizada.



**Resultado:** La operación se completó correctamente. Se verificó mediante una nueva consulta GET que el cambio quedó almacenado.



### 3.5. Validación de códigos duplicados (POST)



Se intentó registrar otro medicamento utilizando el código MED001.



**Resultado:** La API rechazó el registro y devolvió el código HTTP 409 Conflict, evitando la duplicación del código.



### 3.6. Desactivación de medicamentos (DELETE)



Se ejecutó la operación DELETE sobre el identificador 1.



**Resultado:** La operación se completó correctamente. Posteriormente se consultó el registro y se comprobó que permanecía almacenado con el campo Activo establecido en False.



### 3.7. Consulta general después de la desactivación



Se realizó nuevamente una consulta al endpoint `/api/medicamentos`.



**Resultado:** El medicamento continuó apareciendo en el listado, conservando sus datos y mostrando su estado inactivo.



## 4. Resumen de resultados



| Operación | Resultado |

|---|---|

| GET - Consultar todos | Correcto |

| POST - Registrar | Correcto |

| GET - Consultar por ID | Correcto |

| PUT - Actualizar | Correcto |

| Validar código duplicado | HTTP 409 |

| DELETE - Desactivar | Correcto |

| Verificar desactivación | Correcto |



## 5. Resultado final



Las pruebas realizadas permitieron comprobar el funcionamiento de las operaciones CRUD del módulo Medicamentos, la validación de códigos duplicados y la relación con el módulo Categorías.



También se verificó que la desactivación de un medicamento modifica su estado sin eliminar físicamente el registro de la base de datos.



Las operaciones se realizaron mediante los endpoints de la API y se comprobaron las respuestas obtenidas desde PowerShell.
