BEGIN TRANSACTION;

                IF OBJECT_ID(N'dbo.AjustesInventario', N'U') IS NULL
                   OR OBJECT_ID(N'dbo.DetalleAjustesInventario', N'U') IS NULL
                BEGIN
                    THROW 51001,
                        N'No se encontraron las tablas anteriores de ajustes. Verifique el estado de la base de datos.',
                        1;
                END;

                IF OBJECT_ID(N'dbo.AjustesInventarioHistorico', N'U') IS NOT NULL
                   OR OBJECT_ID(N'dbo.DetalleAjustesInventarioHistorico', N'U') IS NOT NULL
                BEGIN
                    THROW 51002,
                        N'Ya existen tablas historicas de ajustes. Revise la base de datos antes de continuar.',
                        1;
                END;


EXEC sp_rename N'[AjustesInventario]', N'AjustesInventarioHistorico', 'OBJECT';

EXEC sp_rename N'[DetalleAjustesInventario]', N'DetalleAjustesInventarioHistorico', 'OBJECT';

EXEC sp_rename N'[AjustesInventarioHistorico].[IX_AjustesInventario_AlmacenId_Estado]', N'IX_AjustesInventarioHistorico_AlmacenId_Estado', 'INDEX';

EXEC sp_rename N'[AjustesInventarioHistorico].[IX_AjustesInventario_NumeroDocumento]', N'IX_AjustesInventarioHistorico_NumeroDocumento', 'INDEX';

EXEC sp_rename N'[DetalleAjustesInventarioHistorico].[IX_DetalleAjustesInventario_AjusteInventarioId_LoteId]', N'IX_DetalleAjustesInventarioHistorico_AjusteInventarioId_LoteId', 'INDEX';

EXEC sp_rename N'[DetalleAjustesInventarioHistorico].[IX_DetalleAjustesInventario_LoteId]', N'IX_DetalleAjustesInventarioHistorico_LoteId', 'INDEX';

EXEC sp_rename N'[DetalleAjustesInventarioHistorico].[IX_DetalleAjustesInventario_MedicamentoId]', N'IX_DetalleAjustesInventarioHistorico_MedicamentoId', 'INDEX';

ALTER TABLE [AjustesInventarioHistorico] DROP CONSTRAINT [FK_AjustesInventario_Almacenes_AlmacenId];

ALTER TABLE [DetalleAjustesInventarioHistorico] DROP CONSTRAINT [FK_DetalleAjustesInventario_AjustesInventario_AjusteInventarioId];

ALTER TABLE [DetalleAjustesInventarioHistorico] DROP CONSTRAINT [FK_DetalleAjustesInventario_Lotes_LoteId];

ALTER TABLE [DetalleAjustesInventarioHistorico] DROP CONSTRAINT [FK_DetalleAjustesInventario_Medicamentos_MedicamentoId];

ALTER TABLE [AjustesInventarioHistorico] DROP CONSTRAINT [PK_AjustesInventario];

ALTER TABLE [DetalleAjustesInventarioHistorico] DROP CONSTRAINT [PK_DetalleAjustesInventario];

ALTER TABLE [AjustesInventarioHistorico] ADD CONSTRAINT [PK_AjustesInventarioHistorico] PRIMARY KEY ([Id]);

ALTER TABLE [DetalleAjustesInventarioHistorico] ADD CONSTRAINT [PK_DetalleAjustesInventarioHistorico] PRIMARY KEY ([Id]);

ALTER TABLE [AjustesInventarioHistorico] ADD CONSTRAINT [FK_AjustesInventarioHistorico_Almacenes_AlmacenId] FOREIGN KEY ([AlmacenId]) REFERENCES [Almacenes] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [DetalleAjustesInventarioHistorico] ADD CONSTRAINT [FK_DetalleAjustesInventarioHistorico_AjustesInventarioHistorico_AjusteInventarioId] FOREIGN KEY ([AjusteInventarioId]) REFERENCES [AjustesInventarioHistorico] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [DetalleAjustesInventarioHistorico] ADD CONSTRAINT [FK_DetalleAjustesInventarioHistorico_Lotes_LoteId] FOREIGN KEY ([LoteId]) REFERENCES [Lotes] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [DetalleAjustesInventarioHistorico] ADD CONSTRAINT [FK_DetalleAjustesInventarioHistorico_Medicamentos_MedicamentoId] FOREIGN KEY ([MedicamentoId]) REFERENCES [Medicamentos] ([Id]) ON DELETE NO ACTION;

CREATE TABLE [AjustesInventario] (
    [Id] int NOT NULL IDENTITY,
    [AlmacenId] int NOT NULL,
    [UsuarioId] int NOT NULL,
    [ConfirmadoPorUsuarioId] int NULL,
    [FechaAjuste] datetime2 NOT NULL,
    [FechaConfirmacion] datetime2 NULL,
    [TipoAjuste] nvarchar(30) NOT NULL,
    [Estado] nvarchar(20) NOT NULL,
    [Motivo] nvarchar(150) NOT NULL,
    [Observaciones] nvarchar(250) NULL,
    CONSTRAINT [PK_AjustesInventario] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AjustesInventario_Almacenes_AlmacenId] FOREIGN KEY ([AlmacenId]) REFERENCES [Almacenes] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_AjustesInventario_Usuarios_ConfirmadoPorUsuarioId] FOREIGN KEY ([ConfirmadoPorUsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_AjustesInventario_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [DetalleAjustes] (
    [Id] int NOT NULL IDENTITY,
    [AjusteInventarioId] int NOT NULL,
    [LoteId] int NOT NULL,
    [ExistenciaSistema] int NOT NULL,
    [CantidadFisica] int NOT NULL,
    [Diferencia] int NOT NULL,
    [Observacion] nvarchar(250) NULL,
    CONSTRAINT [PK_DetalleAjustes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DetalleAjustes_AjustesInventario_AjusteInventarioId] FOREIGN KEY ([AjusteInventarioId]) REFERENCES [AjustesInventario] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DetalleAjustes_Lotes_LoteId] FOREIGN KEY ([LoteId]) REFERENCES [Lotes] ([Id]) ON DELETE NO ACTION
);

ALTER TABLE [DetalleAjustes] ADD CONSTRAINT [CK_DetalleAjustes_CantidadFisica] CHECK ([CantidadFisica] >= 0);

ALTER TABLE [DetalleAjustes] ADD CONSTRAINT [CK_DetalleAjustes_Diferencia] CHECK ([Diferencia] = [CantidadFisica] - [ExistenciaSistema]);

ALTER TABLE [DetalleAjustes] ADD CONSTRAINT [CK_DetalleAjustes_ExistenciaSistema] CHECK ([ExistenciaSistema] >= 0);

CREATE TABLE [MovimientosInventario] (
    [Id] int NOT NULL IDENTITY,
    [TipoMovimiento] nvarchar(30) NOT NULL,
    [AlmacenOrigenId] int NULL,
    [AlmacenDestinoId] int NULL,
    [UsuarioId] int NOT NULL,
    [CompraId] int NULL,
    [VentaId] int NULL,
    [AjusteInventarioId] int NULL,
    [FechaMovimiento] datetime2 NOT NULL,
    [Motivo] nvarchar(150) NULL,
    [Referencia] nvarchar(100) NULL,
    [Estado] nvarchar(20) NOT NULL,
    [Observaciones] nvarchar(250) NULL,
    CONSTRAINT [PK_MovimientosInventario] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MovimientosInventario_AjustesInventario_AjusteInventarioId] FOREIGN KEY ([AjusteInventarioId]) REFERENCES [AjustesInventario] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_MovimientosInventario_Almacenes_AlmacenDestinoId] FOREIGN KEY ([AlmacenDestinoId]) REFERENCES [Almacenes] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_MovimientosInventario_Almacenes_AlmacenOrigenId] FOREIGN KEY ([AlmacenOrigenId]) REFERENCES [Almacenes] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_MovimientosInventario_Compras_CompraId] FOREIGN KEY ([CompraId]) REFERENCES [Compras] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_MovimientosInventario_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_MovimientosInventario_Ventas_VentaId] FOREIGN KEY ([VentaId]) REFERENCES [Ventas] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [DetalleMovimientos] (
    [Id] int NOT NULL IDENTITY,
    [MovimientoInventarioId] int NOT NULL,
    [LoteOrigenId] int NULL,
    [LoteDestinoId] int NULL,
    [Cantidad] int NOT NULL,
    [Observacion] nvarchar(250) NULL,
    CONSTRAINT [PK_DetalleMovimientos] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_DetalleMovimientos_AlMenosUnLote] CHECK ([LoteOrigenId] IS NOT NULL OR [LoteDestinoId] IS NOT NULL),
    CONSTRAINT [CK_DetalleMovimientos_Cantidad] CHECK ([Cantidad] > 0),
    CONSTRAINT [FK_DetalleMovimientos_Lotes_LoteDestinoId] FOREIGN KEY ([LoteDestinoId]) REFERENCES [Lotes] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DetalleMovimientos_Lotes_LoteOrigenId] FOREIGN KEY ([LoteOrigenId]) REFERENCES [Lotes] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DetalleMovimientos_MovimientosInventario_MovimientoInventarioId] FOREIGN KEY ([MovimientoInventarioId]) REFERENCES [MovimientosInventario] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_AjustesInventario_AlmacenId_Estado] ON [AjustesInventario] ([AlmacenId], [Estado]);

CREATE INDEX [IX_AjustesInventario_ConfirmadoPorUsuarioId] ON [AjustesInventario] ([ConfirmadoPorUsuarioId]);

CREATE INDEX [IX_AjustesInventario_UsuarioId] ON [AjustesInventario] ([UsuarioId]);

CREATE UNIQUE INDEX [IX_DetalleAjustes_AjusteInventarioId_LoteId] ON [DetalleAjustes] ([AjusteInventarioId], [LoteId]);

CREATE INDEX [IX_DetalleAjustes_LoteId] ON [DetalleAjustes] ([LoteId]);

CREATE INDEX [IX_DetalleMovimientos_LoteDestinoId] ON [DetalleMovimientos] ([LoteDestinoId]);

CREATE INDEX [IX_DetalleMovimientos_LoteOrigenId] ON [DetalleMovimientos] ([LoteOrigenId]);

CREATE INDEX [IX_DetalleMovimientos_MovimientoInventarioId] ON [DetalleMovimientos] ([MovimientoInventarioId]);

CREATE UNIQUE INDEX [IX_MovimientosInventario_AjusteInventarioId] ON [MovimientosInventario] ([AjusteInventarioId]) WHERE [AjusteInventarioId] IS NOT NULL;

CREATE INDEX [IX_MovimientosInventario_AlmacenDestinoId] ON [MovimientosInventario] ([AlmacenDestinoId]);

CREATE INDEX [IX_MovimientosInventario_AlmacenOrigenId] ON [MovimientosInventario] ([AlmacenOrigenId]);

CREATE UNIQUE INDEX [IX_MovimientosInventario_CompraId] ON [MovimientosInventario] ([CompraId]) WHERE [CompraId] IS NOT NULL;

CREATE INDEX [IX_MovimientosInventario_FechaMovimiento] ON [MovimientosInventario] ([FechaMovimiento]);

CREATE INDEX [IX_MovimientosInventario_UsuarioId] ON [MovimientosInventario] ([UsuarioId]);

CREATE UNIQUE INDEX [IX_MovimientosInventario_VentaId] ON [MovimientosInventario] ([VentaId]) WHERE [VentaId] IS NOT NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261120010257_AjustesInventarioConteoFisicoYMovimientos', N'10.0.15');

COMMIT;
GO
