USE FarmaControlDB;
GO

-- 1. Encabezados de movimientos recientes.
SELECT TOP (100)
    m.Id AS MovimientoId,
    m.TipoMovimiento,
    m.Estado,
    m.FechaMovimiento,
    m.UsuarioId,
    u.UserName AS Usuario,
    m.CompraId,
    c.NumeroDocumento AS DocumentoCompra,
    m.VentaId,
    v.NumeroDocumento AS DocumentoVenta,
    m.AlmacenOrigenId,
    ao.Nombre AS AlmacenOrigen,
    m.AlmacenDestinoId,
    ad.Nombre AS AlmacenDestino,
    m.Motivo,
    m.Referencia
FROM dbo.MovimientosInventario AS m
LEFT JOIN dbo.Usuarios AS u
    ON u.Id = m.UsuarioId
LEFT JOIN dbo.Compras AS c
    ON c.Id = m.CompraId
LEFT JOIN dbo.Ventas AS v
    ON v.Id = m.VentaId
LEFT JOIN dbo.Almacenes AS ao
    ON ao.Id = m.AlmacenOrigenId
LEFT JOIN dbo.Almacenes AS ad
    ON ad.Id = m.AlmacenDestinoId
ORDER BY m.Id DESC;
GO

-- 2. Detalle de lotes y cantidades de cada movimiento.
SELECT TOP (200)
    dm.Id AS DetalleMovimientoId,
    dm.MovimientoInventarioId,
    m.TipoMovimiento,
    m.CompraId,
    m.VentaId,
    dm.LoteOrigenId,
    lo.NumeroLote AS NumeroLoteOrigen,
    dm.LoteDestinoId,
    ld.NumeroLote AS NumeroLoteDestino,
    dm.Cantidad,
    dm.Observacion
FROM dbo.DetalleMovimientos AS dm
INNER JOIN dbo.MovimientosInventario AS m
    ON m.Id = dm.MovimientoInventarioId
LEFT JOIN dbo.Lotes AS lo
    ON lo.Id = dm.LoteOrigenId
LEFT JOIN dbo.Lotes AS ld
    ON ld.Id = dm.LoteDestinoId
ORDER BY dm.Id DESC;
GO

-- 3. Comprobar que cada compra recibida tenga un movimiento Entrada.
SELECT
    c.Id AS CompraId,
    c.NumeroDocumento,
    c.Estado AS EstadoCompra,
    COUNT(m.Id) AS CantidadMovimientosEntrada
FROM dbo.Compras AS c
LEFT JOIN dbo.MovimientosInventario AS m
    ON m.CompraId = c.Id
   AND m.TipoMovimiento = 'Entrada'
WHERE c.Estado = 'Recibida'
GROUP BY c.Id, c.NumeroDocumento, c.Estado
ORDER BY c.Id DESC;
GO

-- 4. Comprobar que cada venta confirmada tenga un movimiento Salida.
SELECT
    v.Id AS VentaId,
    v.NumeroDocumento,
    v.Estado AS EstadoVenta,
    COUNT(m.Id) AS CantidadMovimientosSalida
FROM dbo.Ventas AS v
LEFT JOIN dbo.MovimientosInventario AS m
    ON m.VentaId = v.Id
   AND m.TipoMovimiento = 'Salida'
WHERE v.Estado = 'Confirmada'
GROUP BY v.Id, v.NumeroDocumento, v.Estado
ORDER BY v.Id DESC;
GO
