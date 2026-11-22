/* FARMA CONTROL — vendedor con funciones de cobro y caja.
   Ejecutar EN BASE LOCAL DE PRUEBAS, despues de crear un respaldo .bak.
   No elimina ni crea usuarios. No elimina permisos ni roles.
   Conserva Administrador, Vendedor y Cajero.
   Puede ejecutarse mas de una vez (operacion idempotente).
*/
USE [FarmaControlDB];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.Roles', N'U') IS NULL
        OR OBJECT_ID(N'dbo.Permissions', N'U') IS NULL
        OR OBJECT_ID(N'dbo.RolePermissions', N'U') IS NULL
        THROW 51010, N'No estan presentes las tablas de roles y permisos.', 1;

    DECLARE @VendedorId INT = (SELECT TOP(1) Id FROM dbo.Roles WHERE Name = N'Vendedor');
    IF @VendedorId IS NULL
        THROW 51011, N'No existe el rol Vendedor; no se cambio nada.', 1;

    DECLARE @NuevosPermisos TABLE (Codigo NVARCHAR(100) PRIMARY KEY);
    INSERT INTO @NuevosPermisos (Codigo) VALUES
        (N'metodopago.ver'),
        (N'pago.ver'),
        (N'pago.crear'),
        (N'turnocaja.ver'),
        (N'turnocaja.abrir'),
        (N'turnocaja.cerrar');

    IF EXISTS (
        SELECT 1 FROM @NuevosPermisos n
        WHERE NOT EXISTS (SELECT 1 FROM dbo.Permissions p WHERE p.Name = n.Codigo)
    )
        THROW 51012, N'Falta un permiso necesario; no se cambio nada.', 1;

    INSERT INTO dbo.RolePermissions (RoleId, PermissionId)
    SELECT @VendedorId, p.Id
    FROM dbo.Permissions p
    INNER JOIN @NuevosPermisos n ON n.Codigo = p.Name
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.RolePermissions rp
        WHERE rp.RoleId = @VendedorId AND rp.PermissionId = p.Id
    );

    COMMIT TRANSACTION;
    PRINT N'Vendedor ya puede vender, registrar pagos y abrir/cerrar turnos segun los permisos del rol.';
    SELECT r.Name AS Rol, COUNT(rp.PermissionId) AS PermisosAsignados
    FROM dbo.Roles r
    LEFT JOIN dbo.RolePermissions rp ON rp.RoleId = r.Id
    WHERE r.Name IN (N'Administrador', N'Vendedor', N'Cajero')
    GROUP BY r.Name
    ORDER BY r.Name;

    SELECT p.Name AS PermisoCobroVendedor
    FROM dbo.RolePermissions rp
    INNER JOIN dbo.Permissions p ON p.Id = rp.PermissionId
    WHERE rp.RoleId = @VendedorId
        AND p.Name IN (SELECT Codigo FROM @NuevosPermisos)
    ORDER BY p.Name;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
