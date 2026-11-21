using Microsoft.EntityFrameworkCore;
using Proyecto_FarmaControl.Models;

namespace Proyecto_FarmaControl.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(
            DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // ========================================
        // 1. TABLAS DE NEGOCIO
        // ========================================

        public DbSet<Categoria> Categorias { get; set; } = null!;

        public DbSet<Laboratorio> Laboratorios { get; set; } = null!;

        public DbSet<Presentacion> Presentaciones { get; set; } = null!;

        public DbSet<MetodoPago> MetodosPago { get; set; } = null!;

        public DbSet<Medicamento> Medicamentos { get; set; } = null!;

        public DbSet<Lote> Lotes { get; set; } = null!;

        public DbSet<Sucursal> Sucursales { get; set; } = null!;

        public DbSet<Almacen> Almacenes { get; set; } = null!;

        public DbSet<Proveedor> Proveedores { get; set; } = null!;

        public DbSet<Compra> Compras { get; set; } = null!;

        public DbSet<DetalleCompra> DetalleCompras { get; set; } = null!;

        // NUEVO MODULO: CLIENTES

        public DbSet<Cliente> Clientes { get; set; } = null!;


        // ========================================
        // VENTAS Y DETALLEVENTAS
        // ========================================

        public DbSet<Venta> Ventas { get; set; } = null!;

        public DbSet<DetalleVenta> DetalleVentas { get; set; } = null!;

        // AJUSTES DE INVENTARIO Y MOVIMIENTOS

        public DbSet<AjusteInventario> AjustesInventario { get; set; } = null!;

        public DbSet<DetalleAjusteInventario> DetalleAjustes { get; set; } = null!;

        public DbSet<MovimientoInventario> MovimientosInventario { get; set; } = null!;

        public DbSet<DetalleMovimiento> DetalleMovimientos { get; set; } = null!;

        // Ventas y DetalleVentas se incorporaran
        // cuando completemos sus servicios
        // y reglas de negocio.

        // ========================================
        // 2. TABLAS DE SEGURIDAD
        // ========================================

        public DbSet<Usuario> Usuarios { get; set; } = null!;

        public DbSet<Rol> Roles { get; set; } = null!;

        public DbSet<Permiso> Permissions { get; set; } = null!;

        public DbSet<RolePermission> RolePermissions { get; set; } = null!;

        public DbSet<UsuarioPermiso> UsuarioPermisos { get; set; } = null!;

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========================================
            // 3. ROLES
            // ========================================

            modelBuilder.Entity<Rol>().HasData(
                new Rol
                {
                    Id = 1,
                    Name = "Usuario"
                },
                new Rol
                {
                    Id = 2,
                    Name = "Administrador"
                }
            );

            // ========================================
            // 4. PERMISOS
            // ========================================

            modelBuilder.Entity<Permiso>().HasData(

                // CATEGORIAS

                new Permiso
                {
                    Id = 1,
                    Name = "categoria.ver",
                    Description = "Consultar categorías"
                },
                new Permiso
                {
                    Id = 2,
                    Name = "categoria.crear",
                    Description = "Registrar categorías"
                },
                new Permiso
                {
                    Id = 3,
                    Name = "categoria.actualizar",
                    Description = "Actualizar categorías"
                },
                new Permiso
                {
                    Id = 4,
                    Name = "categoria.borrar",
                    Description = "Desactivar categorías"
                },

                // MEDICAMENTOS

                new Permiso
                {
                    Id = 5,
                    Name = "medicamento.ver",
                    Description = "Consultar medicamentos"
                },
                new Permiso
                {
                    Id = 6,
                    Name = "medicamento.crear",
                    Description = "Registrar medicamentos"
                },
                new Permiso
                {
                    Id = 7,
                    Name = "medicamento.actualizar",
                    Description = "Actualizar medicamentos"
                },
                new Permiso
                {
                    Id = 8,
                    Name = "medicamento.borrar",
                    Description = "Desactivar medicamentos"
                },

                // LOTES

                new Permiso
                {
                    Id = 9,
                    Name = "lote.ver",
                    Description = "Consultar lotes de medicamentos"
                },
                new Permiso
                {
                    Id = 10,
                    Name = "lote.crear",
                    Description = "Registrar lotes de medicamentos"
                },
                new Permiso
                {
                    Id = 11,
                    Name = "lote.actualizar",
                    Description = "Actualizar lotes de medicamentos"
                },
                new Permiso
                {
                    Id = 12,
                    Name = "lote.borrar",
                    Description = "Desactivar lotes de medicamentos"
                },

                // ROLES

                new Permiso
                {
                    Id = 13,
                    Name = "rol.ver",
                    Description = "Consultar roles"
                },
                new Permiso
                {
                    Id = 14,
                    Name = "rol.crear",
                    Description = "Crear roles"
                },
                new Permiso
                {
                    Id = 15,
                    Name = "rol.asignar",
                    Description = "Asignar permisos a roles"
                },

                // PERMISOS

                new Permiso
                {
                    Id = 16,
                    Name = "permiso.ver",
                    Description = "Consultar permisos"
                },
                new Permiso
                {
                    Id = 17,
                    Name = "permiso.crear",
                    Description = "Crear permisos"
                },
                new Permiso
                {
                    Id = 18,
                    Name = "permiso.actualizar",
                    Description = "Actualizar permisos"
                },
                new Permiso
                {
                    Id = 19,
                    Name = "permiso.borrar",
                    Description = "Eliminar permisos"
                },

                // USUARIOS

                new Permiso
                {
                    Id = 20,
                    Name = "usuario.ver",
                    Description = "Consultar usuarios"
                },
                new Permiso
                {
                    Id = 21,
                    Name = "usuario.asignar",
                    Description = "Asignar permisos a usuarios"
                },
                new Permiso
                {
                    Id = 22,
                    Name = "usuario.cambiar.rol",
                    Description = "Cambiar el rol de usuarios"
                },

                // PANEL ADMINISTRATIVO

                new Permiso
                {
                    Id = 23,
                    Name = "admin.panel",
                    Description = "Acceder al panel administrativo"
                },

                // SUCURSALES

                new Permiso
                {
                    Id = 24,
                    Name = "sucursal.ver",
                    Description = "Consultar sucursales"
                },
                new Permiso
                {
                    Id = 25,
                    Name = "sucursal.crear",
                    Description = "Registrar sucursales"
                },
                new Permiso
                {
                    Id = 26,
                    Name = "sucursal.actualizar",
                    Description = "Actualizar sucursales"
                },
                new Permiso
                {
                    Id = 27,
                    Name = "sucursal.borrar",
                    Description = "Desactivar sucursales"
                },

                // ALMACENES

                new Permiso
                {
                    Id = 28,
                    Name = "almacen.ver",
                    Description = "Consultar almacenes"
                },
                new Permiso
                {
                    Id = 29,
                    Name = "almacen.crear",
                    Description = "Registrar almacenes"
                },
                new Permiso
                {
                    Id = 30,
                    Name = "almacen.actualizar",
                    Description = "Actualizar almacenes"
                },
                new Permiso
                {
                    Id = 31,
                    Name = "almacen.borrar",
                    Description = "Desactivar almacenes"
                },

                // PROVEEDORES

                new Permiso
                {
                    Id = 32,
                    Name = "proveedor.ver",
                    Description = "Consultar proveedores"
                },
                new Permiso
                {
                    Id = 33,
                    Name = "proveedor.crear",
                    Description = "Registrar proveedores"
                },
                new Permiso
                {
                    Id = 34,
                    Name = "proveedor.actualizar",
                    Description = "Actualizar proveedores"
                },
                new Permiso
                {
                    Id = 35,
                    Name = "proveedor.borrar",
                    Description = "Desactivar proveedores"
                },

                // COMPRAS

                new Permiso
                {
                    Id = 36,
                    Name = "compra.ver",
                    Description = "Consultar compras"
                },
                new Permiso
                {
                    Id = 37,
                    Name = "compra.crear",
                    Description = "Registrar compras"
                },
                new Permiso
                {
                    Id = 38,
                    Name = "compra.recibir",
                    Description = "Confirmar recepción de compras"
                },
                new Permiso
                {
                    Id = 39,
                    Name = "compra.anular",
                    Description = "Anular compras en borrador"
                },

                // ====================================
                // NUEVOS PERMISOS: CLIENTES
                // ====================================

                new Permiso
                {
                    Id = 40,
                    Name = "cliente.ver",
                    Description = "Consultar clientes"
                },
                new Permiso
                {
                    Id = 41,
                    Name = "cliente.crear",
                    Description = "Registrar clientes"
                },
                new Permiso
                {
                    Id = 42,
                    Name = "cliente.actualizar",
                    Description = "Actualizar clientes"
                },
                new Permiso
                {
                    Id = 43,
                    Name = "cliente.borrar",
                    Description = "Desactivar clientes"
                },

                // ====================================
                // VENTAS - NUEVOS PERMISOS
                // ====================================

                new Permiso
                {
                    Id = 44,
                    Name = "venta.ver",
                    Description = "Consultar ventas"
                },
                new Permiso
                {
                    Id = 45,
                    Name = "venta.crear",
                    Description = "Registrar ventas"
                },
                new Permiso
                {
                    Id = 46,
                    Name = "venta.confirmar",
                    Description = "Confirmar ventas y descontar existencias"
                },

                new Permiso
                {
                    Id = 47,
                    Name = "venta.anular",
                    Description = "Anular ventas en borrador"
                },

                // ====================================
                // AJUSTES DE INVENTARIO - PERMISOS
                // ====================================

                new Permiso
                {
                    Id = 48,
                    Name = "ajuste.ver",
                    Description = "Consultar ajustes de inventario"
                },
                new Permiso
                {
                    Id = 49,
                    Name = "ajuste.crear",
                    Description = "Registrar ajustes de inventario"
                },
                new Permiso
                {
                    Id = 50,
                    Name = "ajuste.aplicar",
                    Description = "Aplicar ajustes y modificar existencias"
                },
                new Permiso
                {
                    Id = 51,
                    Name = "ajuste.anular",
                    Description = "Anular ajustes en borrador"
                },

                // LABORATORIOS
                new Permiso
                {
                    Id = 52,
                    Name = "laboratorio.ver",
                    Description = "Consultar laboratorios"
                },
                new Permiso
                {
                    Id = 53,
                    Name = "laboratorio.crear",
                    Description = "Registrar laboratorios"
                },
                new Permiso
                {
                    Id = 54,
                    Name = "laboratorio.actualizar",
                    Description = "Actualizar laboratorios"
                },
                new Permiso
                {
                    Id = 55,
                    Name = "laboratorio.borrar",
                    Description = "Desactivar laboratorios"
                },

                // PRESENTACIONES
                new Permiso
                {
                    Id = 56,
                    Name = "presentacion.ver",
                    Description = "Consultar presentaciones"
                },
                new Permiso
                {
                    Id = 57,
                    Name = "presentacion.crear",
                    Description = "Registrar presentaciones"
                },
                new Permiso
                {
                    Id = 58,
                    Name = "presentacion.actualizar",
                    Description = "Actualizar presentaciones"
                },
                new Permiso
                {
                    Id = 59,
                    Name = "presentacion.borrar",
                    Description = "Desactivar presentaciones"
                },

                // METODOS DE PAGO
                new Permiso
                {
                    Id = 60,
                    Name = "metodopago.ver",
                    Description = "Consultar métodos de pago"
                },
                new Permiso
                {
                    Id = 61,
                    Name = "metodopago.crear",
                    Description = "Registrar métodos de pago"
                },
                new Permiso
                {
                    Id = 62,
                    Name = "metodopago.actualizar",
                    Description = "Actualizar métodos de pago"
                },
                new Permiso
                {
                    Id = 63,
                    Name = "metodopago.borrar",
                    Description = "Desactivar métodos de pago"
                }

            );

            // ========================================
            // 5. PERMISOS DEL ADMINISTRADOR
            // ========================================

            modelBuilder.Entity<RolePermission>().HasData(

                // CATEGORIAS
                new { RoleId = 2, PermissionId = 1 },
                new { RoleId = 2, PermissionId = 2 },
                new { RoleId = 2, PermissionId = 3 },
                new { RoleId = 2, PermissionId = 4 },

                // MEDICAMENTOS
                new { RoleId = 2, PermissionId = 5 },
                new { RoleId = 2, PermissionId = 6 },
                new { RoleId = 2, PermissionId = 7 },
                new { RoleId = 2, PermissionId = 8 },

                // LOTES
                new { RoleId = 2, PermissionId = 9 },
                new { RoleId = 2, PermissionId = 10 },
                new { RoleId = 2, PermissionId = 11 },
                new { RoleId = 2, PermissionId = 12 },

                // ROLES
                new { RoleId = 2, PermissionId = 13 },
                new { RoleId = 2, PermissionId = 14 },
                new { RoleId = 2, PermissionId = 15 },

                // PERMISOS
                new { RoleId = 2, PermissionId = 16 },
                new { RoleId = 2, PermissionId = 17 },
                new { RoleId = 2, PermissionId = 18 },
                new { RoleId = 2, PermissionId = 19 },

                // USUARIOS
                new { RoleId = 2, PermissionId = 20 },
                new { RoleId = 2, PermissionId = 21 },
                new { RoleId = 2, PermissionId = 22 },

                // PANEL
                new { RoleId = 2, PermissionId = 23 },

                // SUCURSALES
                new { RoleId = 2, PermissionId = 24 },
                new { RoleId = 2, PermissionId = 25 },
                new { RoleId = 2, PermissionId = 26 },
                new { RoleId = 2, PermissionId = 27 },

                // ALMACENES
                new { RoleId = 2, PermissionId = 28 },
                new { RoleId = 2, PermissionId = 29 },
                new { RoleId = 2, PermissionId = 30 },
                new { RoleId = 2, PermissionId = 31 },

                // PROVEEDORES
                new { RoleId = 2, PermissionId = 32 },
                new { RoleId = 2, PermissionId = 33 },
                new { RoleId = 2, PermissionId = 34 },
                new { RoleId = 2, PermissionId = 35 },

                // COMPRAS
                new { RoleId = 2, PermissionId = 36 },
                new { RoleId = 2, PermissionId = 37 },
                new { RoleId = 2, PermissionId = 38 },
                new { RoleId = 2, PermissionId = 39 },

                // CLIENTES - NUEVOS
                new { RoleId = 2, PermissionId = 40 },
                new { RoleId = 2, PermissionId = 41 },
                new { RoleId = 2, PermissionId = 42 },
                new { RoleId = 2, PermissionId = 43 },

                // ====================================
                // VENTAS - ADMINISTRADOR
                // ====================================
                new { RoleId = 2, PermissionId = 44 },
                new { RoleId = 2, PermissionId = 45 },
                new { RoleId = 2, PermissionId = 46 },
                new { RoleId = 2, PermissionId = 47 },

                // ====================================
                // AJUSTES DE INVENTARIO - ADMINISTRADOR
                // ====================================

                new { RoleId = 2, PermissionId = 48 },
                new { RoleId = 2, PermissionId = 49 },
                new { RoleId = 2, PermissionId = 50 },
                new { RoleId = 2, PermissionId = 51 },

                // LABORATORIOS
                new { RoleId = 2, PermissionId = 52 },
                new { RoleId = 2, PermissionId = 53 },
                new { RoleId = 2, PermissionId = 54 },
                new { RoleId = 2, PermissionId = 55 },

                // PRESENTACIONES
                new { RoleId = 2, PermissionId = 56 },
                new { RoleId = 2, PermissionId = 57 },
                new { RoleId = 2, PermissionId = 58 },
                new { RoleId = 2, PermissionId = 59 },

                // METODOS DE PAGO
                new { RoleId = 2, PermissionId = 60 },
                new { RoleId = 2, PermissionId = 61 },
                new { RoleId = 2, PermissionId = 62 },
                new { RoleId = 2, PermissionId = 63 }

            );


            // ========================================
            // 6. MEDICAMENTOS
            // ========================================

            modelBuilder.Entity<Medicamento>(entity =>
            {
                // Codigo interno unico del medicamento.
                entity.HasIndex(m => m.Codigo)
                    .IsUnique();

                // Precio de venta del catalogo.
                entity.Property(m => m.PrecioVenta)
                    .HasPrecision(18, 2)
                    .HasDefaultValue(0m);

                // Los medicamentos nuevos quedan activos por defecto.
                entity.Property(m => m.Activo)
                    .HasDefaultValue(true);

                // Relacion obligatoria con la categoria.
                entity.HasOne(m => m.Categoria)
                    .WithMany()
                    .HasForeignKey(m => m.CategoriaId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relacion opcional con el laboratorio fabricante.
                entity.HasOne(m => m.Laboratorio)
                    .WithMany()
                    .HasForeignKey(m => m.LaboratorioId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relacion opcional con el catalogo de presentaciones.
                entity.HasOne(m => m.PresentacionCatalogo)
                    .WithMany()
                    .HasForeignKey(m => m.PresentacionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ========================================
            // 7. LOTES
            // ========================================

            modelBuilder.Entity<Lote>(entity =>
            {
                entity.HasOne(l => l.Medicamento)
                    .WithMany()
                    .HasForeignKey(l => l.MedicamentoId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(l => l.Almacen)
                    .WithMany()
                    .HasForeignKey(l => l.AlmacenId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(l => new
                {
                    l.MedicamentoId,
                    l.AlmacenId,
                    l.NumeroLote
                })
                .IsUnique();

                entity.Property(l => l.CostoUnitario)
                    .HasPrecision(18, 2);

                entity.Property(l => l.FechaIngreso)
                    .HasColumnType("date");

                entity.Property(l => l.FechaVencimiento)
                    .HasColumnType("date");
            });

            // ========================================
            // 8. USUARIOS
            // ========================================

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasIndex(u => u.Email)
                    .IsUnique();
            });

            // ========================================
            // 9. ROLES Y PERMISOS
            // ========================================

            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(rp => new
                {
                    rp.RoleId,
                    rp.PermissionId
                });

                entity.HasOne(rp => rp.Role)
                    .WithMany(r => r.RolePermissions)
                    .HasForeignKey(rp => rp.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rp => rp.Permission)
                    .WithMany(p => p.RolePermissions)
                    .HasForeignKey(rp => rp.PermissionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ========================================
            // 10. USUARIOS Y PERMISOS
            // ========================================

            modelBuilder.Entity<UsuarioPermiso>(entity =>
            {
                entity.HasKey(up => new
                {
                    up.UsuarioId,
                    up.PermissionId
                });

                entity.HasOne(up => up.Usuario)
                    .WithMany(u => u.UsuarioPermisos)
                    .HasForeignKey(up => up.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(up => up.Permiso)
                    .WithMany(p => p.UsuarioPermisos)
                    .HasForeignKey(up => up.PermissionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ========================================
            // 11. SUCURSALES
            // ========================================

            modelBuilder.Entity<Sucursal>(entity =>
            {
                entity.HasIndex(s => s.Codigo)
                    .IsUnique();
            });

            // ========================================
            // 12. ALMACENES
            // ========================================

            modelBuilder.Entity<Almacen>(entity =>
            {
                entity.HasIndex(a => new
                {
                    a.SucursalId,
                    a.Codigo
                })
                .IsUnique();

                entity.HasOne(a => a.Sucursal)
                    .WithMany(s => s.Almacenes)
                    .HasForeignKey(a => a.SucursalId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ========================================
            // 13. PROVEEDORES
            // ========================================

            modelBuilder.Entity<Proveedor>(entity =>
            {
                entity.HasIndex(p => p.Codigo)
                    .IsUnique();
            });

            // ========================================
            // 14. COMPRAS
            // ========================================

            modelBuilder.Entity<Compra>(entity =>
            {
                entity.HasOne(c => c.Proveedor)
                    .WithMany()
                    .HasForeignKey(c => c.ProveedorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Almacen)
                    .WithMany()
                    .HasForeignKey(c => c.AlmacenId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(c => new
                {
                    c.ProveedorId,
                    c.NumeroDocumento
                })
                .IsUnique();

                entity.Property(c => c.Subtotal)
                    .HasPrecision(18, 2);

                entity.Property(c => c.Impuesto)
                    .HasPrecision(18, 2);

                entity.Property(c => c.Total)
                    .HasPrecision(18, 2);

                entity.Property(c => c.FechaCompra)
                    .HasColumnType("date");

                entity.Property(c => c.FechaRecepcion)
                    .HasColumnType("datetime2");
            });

            // ========================================
            // 15. DETALLE DE COMPRAS
            // ========================================

            modelBuilder.Entity<DetalleCompra>(entity =>
            {
                entity.HasOne(d => d.Compra)
                    .WithMany(c => c.Detalles)
                    .HasForeignKey(d => d.CompraId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Medicamento)
                    .WithMany()
                    .HasForeignKey(d => d.MedicamentoId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Lote)
                    .WithMany()
                    .HasForeignKey(d => d.LoteId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(d => d.CostoUnitario)
                    .HasPrecision(18, 2);

                entity.Property(d => d.Subtotal)
                    .HasPrecision(18, 2);

                entity.Property(d => d.FechaVencimiento)
                    .HasColumnType("date");
            });

            // ========================================
            // 16. CLIENTES - NUEVA CONFIGURACION
            // ========================================

            modelBuilder.Entity<Cliente>(entity =>
            {
                // Ningun cliente puede compartir
                // el mismo codigo con otro.

                entity.HasIndex(c => c.Codigo)
                    .IsUnique();
            });


            // ========================================
            // 17. VENTAS
            // ========================================

            modelBuilder.Entity<Venta>(entity =>
            {
                // Documento unico de venta.

                entity.HasIndex(v => v.NumeroDocumento)
                    .IsUnique();

                // Una venta pertenece a un cliente.

                entity.HasOne(v => v.Cliente)
                    .WithMany()
                    .HasForeignKey(v => v.ClienteId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Una venta sale de un almacen.

                entity.HasOne(v => v.Almacen)
                    .WithMany()
                    .HasForeignKey(v => v.AlmacenId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Importes monetarios.

                entity.Property(v => v.Subtotal)
                    .HasPrecision(18, 2);

                entity.Property(v => v.Impuesto)
                    .HasPrecision(18, 2);

                entity.Property(v => v.Total)
                    .HasPrecision(18, 2);

                // Fechas.

                entity.Property(v => v.FechaVenta)
                    .HasColumnType("date");

                entity.Property(v => v.FechaConfirmacion)
                    .HasColumnType("datetime2");
            });


            // ========================================
            // 18. DETALLE DE VENTAS
            // ========================================

            modelBuilder.Entity<DetalleVenta>(entity =>
            {
                // Relacion con la venta.

                entity.HasOne(d => d.Venta)
                    .WithMany(v => v.Detalles)
                    .HasForeignKey(d => d.VentaId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relacion con el medicamento.

                entity.HasOne(d => d.Medicamento)
                    .WithMany()
                    .HasForeignKey(d => d.MedicamentoId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relacion con el lote utilizado.

                entity.HasOne(d => d.Lote)
                    .WithMany()
                    .HasForeignKey(d => d.LoteId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Importes monetarios.

                entity.Property(d => d.PrecioUnitario)
                    .HasPrecision(18, 2);

                entity.Property(d => d.Subtotal)
                    .HasPrecision(18, 2);

                // Indice para consultas de lotes
                // asociados a los detalles.

                entity.HasIndex(d => d.LoteId);
            });

            // ========================================
            // 19. AJUSTES DE INVENTARIO
            // ========================================

            modelBuilder.Entity<AjusteInventario>(entity =>
            {
                entity.ToTable("AjustesInventario");

                entity.HasOne(a => a.Almacen)
                    .WithMany()
                    .HasForeignKey(a => a.AlmacenId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Usuario)
                    .WithMany()
                    .HasForeignKey(a => a.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.ConfirmadoPorUsuario)
                    .WithMany()
                    .HasForeignKey(a => a.ConfirmadoPorUsuarioId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(a => a.TipoAjuste)
                    .HasMaxLength(30)
                    .IsRequired();

                entity.Property(a => a.Estado)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(a => a.Motivo)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(a => a.Observaciones)
                    .HasMaxLength(250);

                entity.Property(a => a.FechaAjuste)
                    .HasColumnType("datetime2");

                entity.Property(a => a.FechaConfirmacion)
                    .HasColumnType("datetime2");

                entity.HasIndex(a => new
                {
                    a.AlmacenId,
                    a.Estado
                });

                entity.HasIndex(a => a.UsuarioId);

                entity.HasIndex(a => a.ConfirmadoPorUsuarioId);
            });

            // ========================================
            // 20. DETALLE DE AJUSTES DE INVENTARIO
            // ========================================

            modelBuilder.Entity<DetalleAjusteInventario>(entity =>
            {
                entity.ToTable("DetalleAjustes", table =>
                {
                    table.HasCheckConstraint(
                        "CK_DetalleAjustes_ExistenciaSistema",
                        "[ExistenciaSistema] >= 0");

                    table.HasCheckConstraint(
                        "CK_DetalleAjustes_CantidadFisica",
                        "[CantidadFisica] >= 0");

                    table.HasCheckConstraint(
                        "CK_DetalleAjustes_Diferencia",
                        "[Diferencia] = [CantidadFisica] - [ExistenciaSistema]");
                });

                entity.HasOne(d => d.AjusteInventario)
                    .WithMany(a => a.Detalles)
                    .HasForeignKey(d => d.AjusteInventarioId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Lote)
                    .WithMany()
                    .HasForeignKey(d => d.LoteId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(d => d.Observacion)
                    .HasMaxLength(250);

                entity.HasIndex(d => new
                {
                    d.AjusteInventarioId,
                    d.LoteId
                }).IsUnique();

                entity.HasIndex(d => d.LoteId);
            });

            // ========================================
            // 21. MOVIMIENTOS DE INVENTARIO
            // ========================================

            modelBuilder.Entity<MovimientoInventario>(entity =>
            {
                entity.ToTable("MovimientosInventario");

                entity.Property(m => m.TipoMovimiento)
                    .HasMaxLength(30)
                    .IsRequired();

                entity.Property(m => m.Estado)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(m => m.Motivo)
                    .HasMaxLength(150);

                entity.Property(m => m.Referencia)
                    .HasMaxLength(100);

                entity.Property(m => m.Observaciones)
                    .HasMaxLength(250);

                entity.Property(m => m.FechaMovimiento)
                    .HasColumnType("datetime2");

                entity.HasOne(m => m.AlmacenOrigen)
                    .WithMany()
                    .HasForeignKey(m => m.AlmacenOrigenId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.AlmacenDestino)
                    .WithMany()
                    .HasForeignKey(m => m.AlmacenDestinoId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Usuario)
                    .WithMany()
                    .HasForeignKey(m => m.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Compra)
                    .WithOne()
                    .HasForeignKey<MovimientoInventario>(m => m.CompraId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Venta)
                    .WithOne()
                    .HasForeignKey<MovimientoInventario>(m => m.VentaId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.AjusteInventario)
                    .WithOne(a => a.MovimientoInventario)
                    .HasForeignKey<MovimientoInventario>(m => m.AjusteInventarioId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(m => m.AlmacenOrigenId);

                entity.HasIndex(m => m.AlmacenDestinoId);

                entity.HasIndex(m => m.UsuarioId);

                entity.HasIndex(m => m.FechaMovimiento);
            });

            // ========================================
            // 22. DETALLE DE MOVIMIENTOS
            // ========================================

            modelBuilder.Entity<DetalleMovimiento>(entity =>
            {
                entity.ToTable("DetalleMovimientos", table =>
                {
                    table.HasCheckConstraint(
                        "CK_DetalleMovimientos_AlMenosUnLote",
                        "[LoteOrigenId] IS NOT NULL OR [LoteDestinoId] IS NOT NULL");

                    table.HasCheckConstraint(
                        "CK_DetalleMovimientos_Cantidad",
                        "[Cantidad] > 0");
                });

                entity.HasOne(d => d.MovimientoInventario)
                    .WithMany(m => m.Detalles)
                    .HasForeignKey(d => d.MovimientoInventarioId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.LoteOrigen)
                    .WithMany()
                    .HasForeignKey(d => d.LoteOrigenId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.LoteDestino)
                    .WithMany()
                    .HasForeignKey(d => d.LoteDestinoId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(d => d.Observacion)
                    .HasMaxLength(250);

                entity.HasIndex(d => d.MovimientoInventarioId);

                entity.HasIndex(d => d.LoteOrigenId);

                entity.HasIndex(d => d.LoteDestinoId);
            });

            // ========================================
            // 23. LABORATORIOS
            // ========================================

            modelBuilder.Entity<Laboratorio>(entity =>
            {
                entity.ToTable("Laboratorios");

                entity.Property(l => l.Nombre)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(l => l.Pais)
                    .HasMaxLength(100);

                entity.Property(l => l.Contacto)
                    .HasMaxLength(100);

                entity.Property(l => l.Telefono)
                    .HasMaxLength(30);

                entity.Property(l => l.Email)
                    .HasMaxLength(150);

                entity.Property(l => l.Direccion)
                    .HasMaxLength(250);

                entity.Property(l => l.Activo)
                    .HasDefaultValue(true);
            });

            // ========================================
            // 24. PRESENTACIONES
            // ========================================

            modelBuilder.Entity<Presentacion>(entity =>
            {
                entity.ToTable("Presentaciones", table =>
                {
                    table.HasCheckConstraint(
                        "CK_Presentaciones_CantidadContenido",
                        "[CantidadContenido] IS NULL OR [CantidadContenido] > 0");
                });

                entity.Property(p => p.Nombre)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(p => p.Descripcion)
                    .HasMaxLength(250);

                entity.Property(p => p.CantidadContenido)
                    .HasPrecision(18, 3);

                entity.Property(p => p.UnidadContenido)
                    .HasMaxLength(30);

                entity.Property(p => p.Activo)
                    .HasDefaultValue(true);

                entity.HasIndex(p => p.Nombre)
                    .IsUnique();
            });

            // ========================================
            // 25. METODOS DE PAGO
            // ========================================

            modelBuilder.Entity<MetodoPago>(entity =>
            {
                entity.ToTable("MetodosPago");

                entity.Property(m => m.Nombre)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(m => m.Descripcion)
                    .HasMaxLength(150);

                entity.Property(m => m.RequiereReferencia)
                    .HasDefaultValue(false);

                entity.Property(m => m.Activo)
                    .HasDefaultValue(true);

                entity.HasIndex(m => m.Nombre)
                    .IsUnique();
            });

        }
    }
}