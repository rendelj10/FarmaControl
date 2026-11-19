
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
        // MODULOS PRINCIPALES
        // ========================================

        public DbSet<Categoria> Categorias { get; set; } = null!;

        public DbSet<Medicamento> Medicamentos { get; set; } = null!;

        public DbSet<Lote> Lotes { get; set; } = null!;

        // ========================================
        // MODULO DE SEGURIDAD
        // ========================================

        public DbSet<Usuario> Usuarios { get; set; } = null!;

        public DbSet<Rol> Roles { get; set; } = null!;

        public DbSet<Permiso> Permissions { get; set; } = null!;

        public DbSet<RolePermission> RolePermissions { get; set; } = null!;

        public DbSet<UsuarioPermiso> UsuarioPermisos { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========================================
            // 1. ROLES DEL SISTEMA
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
            // 2. PERMISOS DEL SISTEMA
            // ========================================

            modelBuilder.Entity<Permiso>().HasData(

                // PERMISOS DE CATEGORIAS

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

                // PERMISOS DE MEDICAMENTOS

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

                // PERMISOS DE LOTES

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

                // PERMISOS DE ROLES

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

                // PERMISOS DE ADMINISTRACION DE PERMISOS

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

                // PERMISOS DE USUARIOS

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

                // PERMISO DEL PANEL ADMINISTRATIVO

                new Permiso
                {
                    Id = 23,
                    Name = "admin.panel",
                    Description = "Acceder al panel administrativo"
                }
            );

            // ========================================
            // 3. PERMISOS ASIGNADOS AL ADMINISTRADOR
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

                // PANEL ADMINISTRATIVO

                new { RoleId = 2, PermissionId = 23 }
            );

            // ========================================
            // 4. RELACION MEDICAMENTOS - CATEGORIAS
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
            // 5. RELACION LOTES - MEDICAMENTOS
            // ========================================

            modelBuilder.Entity<Lote>(entity =>
            {
                entity.HasOne(l => l.Medicamento)
                    .WithMany()
                    .HasForeignKey(l => l.MedicamentoId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(l => l.CostoUnitario)
                    .HasPrecision(18, 2);

                entity.Property(l => l.FechaIngreso)
                    .HasColumnType("date");

                entity.Property(l => l.FechaVencimiento)
                    .HasColumnType("date");
            });

            // ========================================
            // 6. CONFIGURACION DE USUARIOS
            // ========================================

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasIndex(u => u.Email)
                    .IsUnique();
            });

            // ========================================
            // 7. RELACION ROLES - PERMISOS
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
            // 8. RELACION USUARIOS - PERMISOS
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
        }
    }
}
