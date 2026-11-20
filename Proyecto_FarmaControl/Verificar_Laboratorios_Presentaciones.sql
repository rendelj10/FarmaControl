USE FarmaControlDB;
GO

-- 1. Confirmar que las dos tablas existen.
SELECT name AS Tabla
FROM sys.tables
WHERE name IN ('Laboratorios', 'Presentaciones')
ORDER BY name;
GO

-- 2. Revisar columnas, tipos y nulabilidad.
SELECT
    t.name AS Tabla,
    c.name AS Columna,
    ty.name AS TipoDato,
    c.max_length AS LongitudBytes,
    c.precision AS Precision,
    c.scale AS Escala,
    c.is_nullable AS PermiteNull
FROM sys.tables t
INNER JOIN sys.columns c ON c.object_id = t.object_id
INNER JOIN sys.types ty ON ty.user_type_id = c.user_type_id
WHERE t.name IN ('Laboratorios', 'Presentaciones')
ORDER BY t.name, c.column_id;
GO

-- 3. Confirmar índice único de Presentaciones.Nombre.
SELECT
    i.name AS Indice,
    i.is_unique AS EsUnico,
    c.name AS Columna
FROM sys.indexes i
INNER JOIN sys.index_columns ic
    ON ic.object_id = i.object_id
   AND ic.index_id = i.index_id
INNER JOIN sys.columns c
    ON c.object_id = ic.object_id
   AND c.column_id = ic.column_id
INNER JOIN sys.tables t ON t.object_id = i.object_id
WHERE t.name = 'Presentaciones'
  AND i.is_unique = 1;
GO

-- 4. Confirmar los permisos nuevos y su asignación al Administrador.
SELECT
    p.Id,
    p.Name,
    p.Description,
    rp.RoleId
FROM dbo.Permissions p
LEFT JOIN dbo.RolePermissions rp
    ON rp.PermissionId = p.Id
   AND rp.RoleId = 2
WHERE p.Id BETWEEN 52 AND 59
ORDER BY p.Id;
GO

-- 5. Confirmar la restricción de cantidad positiva.
SELECT
    t.name AS Tabla,
    cc.name AS Restriccion,
    cc.definition AS Definicion
FROM sys.check_constraints cc
INNER JOIN sys.tables t ON t.object_id = cc.parent_object_id
WHERE t.name = 'Presentaciones'
  AND cc.name = 'CK_Presentaciones_CantidadContenido';
GO
