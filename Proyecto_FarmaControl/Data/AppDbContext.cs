
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
        // 1. MODULOS DE NEGOCIO
        // ========================================

        public DbSet<Categoria> Categorias { get; set; } = null!;

        public DbSet<Medicamento> Medicamentos { get; set; } = null!;

        public DbSet<Lote> Lotes { get; set; } = null!;

        public DbSet<Sucursal> Sucursales { get; set; } = null!;

        public DbSet<Almacen> Almacenes { get; set; } = null!;

        public DbSet<Proveedor> Proveedores { get; set; } = null!;

        // COMPRAS Y DETALLES

        public DbSet<Compra> Compras { get; set; } = null!;

        public DbSet<DetalleCompra> DetalleCompras { get; set; } = null!;

        // ========================================
        // 2. MODULO DE SEGURIDAD
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
            // 3. ROLES DEL SISTEMA
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
            // 4. PERMISOS DEL SISTEMA
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

                // ========================================
                // 5. NUEVOS PERMISOS DE COMPRAS
                // ========================================

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
                }
            );

            // ========================================
            // 6. PERMISOS DEL ADMINISTRADOR
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

                // ADMINISTRACION
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
                new { RoleId = 2, PermissionId = 39 }
            );

            // ========================================
            // 7. MEDICAMENTOS - CATEGORIAS
            // ========================================

            modelBuilder.Entity<Medicamento>(entity =>
            {
                entity.HasIndex(m => m.Codigo)
                    .IsUnique();

                entity.HasOne(m => m.Categoria)
                    .WithMany()
                    .HasForeignKey(m => m.CategoriaId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ========================================
            // 8. LOTES - MEDICAMENTOS Y ALMACENES
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
            // 9. USUARIOS
            // ========================================

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasIndex(u => u.Email)
                    .IsUnique();
            });

            // ========================================
            // 10. ROLES - PERMISOS
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
            // 11. USUARIOS - PERMISOS
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
            // 12. SUCURSALES
            // ========================================

            modelBuilder.Entity<Sucursal>(entity =>
            {
                entity.HasIndex(s => s.Codigo)
                    .IsUnique();
            });

            // ========================================
            // 13. ALMACENES - SUCURSALES
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
            // 14. PROVEEDORES
            // ========================================

            modelBuilder.Entity<Proveedor>(entity =>
            {
                entity.HasIndex(p => p.Codigo)
                    .IsUnique();
            });

            // ========================================
            // 15. COMPRAS - NUEVA TABLA
            // ========================================

            modelBuilder.Entity<Compra>(entity =>
            {
                // Una compra pertenece a un proveedor.

                entity.HasOne(c => c.Proveedor)
                    .WithMany()
                    .HasForeignKey(c => c.ProveedorId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Una compra tiene un almacen destino.

                entity.HasOne(c => c.Almacen)
                    .WithMany()
                    .HasForeignKey(c => c.AlmacenId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Un numero de documento no puede
                // repetirse para el mismo proveedor.

                entity.HasIndex(c => new
                {
                    c.ProveedorId,
                    c.NumeroDocumento
                })
                .IsUnique();

                // Importes monetarios.

                entity.Property(c => c.Subtotal)
                    .HasPrecision(18, 2);

                entity.Property(c => c.Impuesto)
                    .HasPrecision(18, 2);

                entity.Property(c => c.Total)
                    .HasPrecision(18, 2);

                // Fechas.

                entity.Property(c => c.FechaCompra)
                    .HasColumnType("date");

                entity.Property(c => c.FechaRecepcion)
                    .HasColumnType("datetime2");
            });

            // ========================================
            // 16. DETALLES DE COMPRAS - NUEVA TABLA
            // ========================================

            modelBuilder.Entity<DetalleCompra>(entity =>
            {
                // Una compra puede contener
                // varios detalles.

                entity.HasOne(d => d.Compra)
                    .WithMany(c => c.Detalles)
                    .HasForeignKey(d => d.CompraId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Cada detalle corresponde
                // a un medicamento.

                entity.HasOne(d => d.Medicamento)
                    .WithMany()
                    .HasForeignKey(d => d.MedicamentoId)
                    .OnDelete(DeleteBehavior.Restrict);

                // El lote puede ser nulo mientras
                // la compra esta en borrador.
                //
                // Se asignara durante la recepcion.

                entity.HasOne(d => d.Lote)
                    .WithMany()
                    .HasForeignKey(d => d.LoteId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Importes monetarios.

                entity.Property(d => d.CostoUnitario)
                    .HasPrecision(18, 2);

                entity.Property(d => d.Subtotal)
                    .HasPrecision(18, 2);

                // Fecha de vencimiento del lote.

                entity.Property(d => d.FechaVencimiento)
                    .HasColumnType("date");
            });
        }
    }
}
