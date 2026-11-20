using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Proyecto_FarmaControl.Migrations
{
    /// <inheritdoc />
    public partial class AjustesInventarioConteoFisicoYMovimientos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ============================================================
            // 1. VALIDAR Y CONSERVAR LAS TABLAS DEL MODULO ANTERIOR
            // ============================================================

            migrationBuilder.Sql(@"
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
            ");

            // Se renombran las tablas anteriores. No se elimina ni transforma
            // ningun registro historico.
            migrationBuilder.RenameTable(
                name: "AjustesInventario",
                newName: "AjustesInventarioHistorico");

            migrationBuilder.RenameTable(
                name: "DetalleAjustesInventario",
                newName: "DetalleAjustesInventarioHistorico");

            // Se renombran los indices historicos para distinguirlos de los
            // indices que tendra el nuevo modulo.
            migrationBuilder.RenameIndex(
                name: "IX_AjustesInventario_AlmacenId_Estado",
                table: "AjustesInventarioHistorico",
                newName: "IX_AjustesInventarioHistorico_AlmacenId_Estado");

            migrationBuilder.RenameIndex(
                name: "IX_AjustesInventario_NumeroDocumento",
                table: "AjustesInventarioHistorico",
                newName: "IX_AjustesInventarioHistorico_NumeroDocumento");

            migrationBuilder.RenameIndex(
                name: "IX_DetalleAjustesInventario_AjusteInventarioId_LoteId",
                table: "DetalleAjustesInventarioHistorico",
                newName: "IX_DetalleAjustesInventarioHistorico_AjusteInventarioId_LoteId");

            migrationBuilder.RenameIndex(
                name: "IX_DetalleAjustesInventario_LoteId",
                table: "DetalleAjustesInventarioHistorico",
                newName: "IX_DetalleAjustesInventarioHistorico_LoteId");

            migrationBuilder.RenameIndex(
                name: "IX_DetalleAjustesInventario_MedicamentoId",
                table: "DetalleAjustesInventarioHistorico",
                newName: "IX_DetalleAjustesInventarioHistorico_MedicamentoId");

            // Se reconstruyen las restricciones de las tablas historicas
            // con nombres que no colisionen con las nuevas tablas.
            migrationBuilder.DropForeignKey(
                name: "FK_AjustesInventario_Almacenes_AlmacenId",
                table: "AjustesInventarioHistorico");

            migrationBuilder.DropForeignKey(
                name: "FK_DetalleAjustesInventario_AjustesInventario_AjusteInventarioId",
                table: "DetalleAjustesInventarioHistorico");

            migrationBuilder.DropForeignKey(
                name: "FK_DetalleAjustesInventario_Lotes_LoteId",
                table: "DetalleAjustesInventarioHistorico");

            migrationBuilder.DropForeignKey(
                name: "FK_DetalleAjustesInventario_Medicamentos_MedicamentoId",
                table: "DetalleAjustesInventarioHistorico");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AjustesInventario",
                table: "AjustesInventarioHistorico");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DetalleAjustesInventario",
                table: "DetalleAjustesInventarioHistorico");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AjustesInventarioHistorico",
                table: "AjustesInventarioHistorico",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DetalleAjustesInventarioHistorico",
                table: "DetalleAjustesInventarioHistorico",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AjustesInventarioHistorico_Almacenes_AlmacenId",
                table: "AjustesInventarioHistorico",
                column: "AlmacenId",
                principalTable: "Almacenes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleAjustesInventarioHistorico_AjustesInventarioHistorico_AjusteInventarioId",
                table: "DetalleAjustesInventarioHistorico",
                column: "AjusteInventarioId",
                principalTable: "AjustesInventarioHistorico",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleAjustesInventarioHistorico_Lotes_LoteId",
                table: "DetalleAjustesInventarioHistorico",
                column: "LoteId",
                principalTable: "Lotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleAjustesInventarioHistorico_Medicamentos_MedicamentoId",
                table: "DetalleAjustesInventarioHistorico",
                column: "MedicamentoId",
                principalTable: "Medicamentos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // ============================================================
            // 2. CREAR LA NUEVA TABLA DE AJUSTES DE INVENTARIO
            // ============================================================

            migrationBuilder.CreateTable(
                name: "AjustesInventario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlmacenId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    ConfirmadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    FechaAjuste = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaConfirmacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TipoAjuste = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AjustesInventario", x => x.Id);

                    table.ForeignKey(
                        name: "FK_AjustesInventario_Almacenes_AlmacenId",
                        column: x => x.AlmacenId,
                        principalTable: "Almacenes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_AjustesInventario_Usuarios_ConfirmadoPorUsuarioId",
                        column: x => x.ConfirmadoPorUsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_AjustesInventario_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // ============================================================
            // 3. CREAR LOS DETALLES DEL CONTEO FISICO
            // ============================================================

            migrationBuilder.CreateTable(
                name: "DetalleAjustes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AjusteInventarioId = table.Column<int>(type: "int", nullable: false),
                    LoteId = table.Column<int>(type: "int", nullable: false),
                    ExistenciaSistema = table.Column<int>(type: "int", nullable: false),
                    CantidadFisica = table.Column<int>(type: "int", nullable: false),
                    Diferencia = table.Column<int>(type: "int", nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleAjustes", x => x.Id);

                    table.ForeignKey(
                        name: "FK_DetalleAjustes_AjustesInventario_AjusteInventarioId",
                        column: x => x.AjusteInventarioId,
                        principalTable: "AjustesInventario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_DetalleAjustes_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_DetalleAjustes_CantidadFisica",
                table: "DetalleAjustes",
                sql: "[CantidadFisica] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DetalleAjustes_Diferencia",
                table: "DetalleAjustes",
                sql: "[Diferencia] = [CantidadFisica] - [ExistenciaSistema]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DetalleAjustes_ExistenciaSistema",
                table: "DetalleAjustes",
                sql: "[ExistenciaSistema] >= 0");

            // ============================================================
            // 4. CREAR EL REGISTRO GENERAL DE MOVIMIENTOS
            // ============================================================

            migrationBuilder.CreateTable(
                name: "MovimientosInventario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipoMovimiento = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AlmacenOrigenId = table.Column<int>(type: "int", nullable: true),
                    AlmacenDestinoId = table.Column<int>(type: "int", nullable: true),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    CompraId = table.Column<int>(type: "int", nullable: true),
                    VentaId = table.Column<int>(type: "int", nullable: true),
                    AjusteInventarioId = table.Column<int>(type: "int", nullable: true),
                    FechaMovimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Referencia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosInventario", x => x.Id);

                    table.ForeignKey(
                        name: "FK_MovimientosInventario_AjustesInventario_AjusteInventarioId",
                        column: x => x.AjusteInventarioId,
                        principalTable: "AjustesInventario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_MovimientosInventario_Almacenes_AlmacenDestinoId",
                        column: x => x.AlmacenDestinoId,
                        principalTable: "Almacenes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_MovimientosInventario_Almacenes_AlmacenOrigenId",
                        column: x => x.AlmacenOrigenId,
                        principalTable: "Almacenes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_MovimientosInventario_Compras_CompraId",
                        column: x => x.CompraId,
                        principalTable: "Compras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_MovimientosInventario_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_MovimientosInventario_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // ============================================================
            // 5. CREAR LOS DETALLES DE CADA MOVIMIENTO
            // ============================================================

            migrationBuilder.CreateTable(
                name: "DetalleMovimientos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MovimientoInventarioId = table.Column<int>(type: "int", nullable: false),
                    LoteOrigenId = table.Column<int>(type: "int", nullable: true),
                    LoteDestinoId = table.Column<int>(type: "int", nullable: true),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleMovimientos", x => x.Id);

                    table.CheckConstraint(
                        "CK_DetalleMovimientos_AlMenosUnLote",
                        "[LoteOrigenId] IS NOT NULL OR [LoteDestinoId] IS NOT NULL");

                    table.CheckConstraint(
                        "CK_DetalleMovimientos_Cantidad",
                        "[Cantidad] > 0");

                    table.ForeignKey(
                        name: "FK_DetalleMovimientos_Lotes_LoteDestinoId",
                        column: x => x.LoteDestinoId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_DetalleMovimientos_Lotes_LoteOrigenId",
                        column: x => x.LoteOrigenId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_DetalleMovimientos_MovimientosInventario_MovimientoInventarioId",
                        column: x => x.MovimientoInventarioId,
                        principalTable: "MovimientosInventario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // ============================================================
            // 6. INDICES DE AJUSTES Y DETALLES
            // ============================================================

            migrationBuilder.CreateIndex(
                name: "IX_AjustesInventario_AlmacenId_Estado",
                table: "AjustesInventario",
                columns: new[] { "AlmacenId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_AjustesInventario_ConfirmadoPorUsuarioId",
                table: "AjustesInventario",
                column: "ConfirmadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_AjustesInventario_UsuarioId",
                table: "AjustesInventario",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleAjustes_AjusteInventarioId_LoteId",
                table: "DetalleAjustes",
                columns: new[] { "AjusteInventarioId", "LoteId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetalleAjustes_LoteId",
                table: "DetalleAjustes",
                column: "LoteId");

            // ============================================================
            // 7. INDICES DE MOVIMIENTOS
            // ============================================================

            migrationBuilder.CreateIndex(
                name: "IX_DetalleMovimientos_LoteDestinoId",
                table: "DetalleMovimientos",
                column: "LoteDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleMovimientos_LoteOrigenId",
                table: "DetalleMovimientos",
                column: "LoteOrigenId");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleMovimientos_MovimientoInventarioId",
                table: "DetalleMovimientos",
                column: "MovimientoInventarioId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_AjusteInventarioId",
                table: "MovimientosInventario",
                column: "AjusteInventarioId",
                unique: true,
                filter: "[AjusteInventarioId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_AlmacenDestinoId",
                table: "MovimientosInventario",
                column: "AlmacenDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_AlmacenOrigenId",
                table: "MovimientosInventario",
                column: "AlmacenOrigenId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_CompraId",
                table: "MovimientosInventario",
                column: "CompraId",
                unique: true,
                filter: "[CompraId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_FechaMovimiento",
                table: "MovimientosInventario",
                column: "FechaMovimiento");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_UsuarioId",
                table: "MovimientosInventario",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_VentaId",
                table: "MovimientosInventario",
                column: "VentaId",
                unique: true,
                filter: "[VentaId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No se permite revertir si el nuevo modulo ya contiene datos.
            // Esto evita eliminar ajustes o movimientos creados despues de aplicar
            // esta migracion.
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM dbo.AjustesInventario)
                   OR EXISTS (SELECT 1 FROM dbo.DetalleAjustes)
                   OR EXISTS (SELECT 1 FROM dbo.MovimientosInventario)
                   OR EXISTS (SELECT 1 FROM dbo.DetalleMovimientos)
                BEGIN
                    THROW 51003,
                        N'No se puede revertir: existen ajustes o movimientos del nuevo modulo. Respalde y migre antes los datos.',
                        1;
                END;
            ");

            // Eliminar primero las tablas dependientes del nuevo modulo.
            migrationBuilder.DropTable(
                name: "DetalleMovimientos");

            migrationBuilder.DropTable(
                name: "MovimientosInventario");

            migrationBuilder.DropTable(
                name: "DetalleAjustes");

            migrationBuilder.DropTable(
                name: "AjustesInventario");

            // Restaurar las restricciones de las tablas historicas.
            migrationBuilder.DropForeignKey(
                name: "FK_AjustesInventarioHistorico_Almacenes_AlmacenId",
                table: "AjustesInventarioHistorico");

            migrationBuilder.DropForeignKey(
                name: "FK_DetalleAjustesInventarioHistorico_AjustesInventarioHistorico_AjusteInventarioId",
                table: "DetalleAjustesInventarioHistorico");

            migrationBuilder.DropForeignKey(
                name: "FK_DetalleAjustesInventarioHistorico_Lotes_LoteId",
                table: "DetalleAjustesInventarioHistorico");

            migrationBuilder.DropForeignKey(
                name: "FK_DetalleAjustesInventarioHistorico_Medicamentos_MedicamentoId",
                table: "DetalleAjustesInventarioHistorico");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AjustesInventarioHistorico",
                table: "AjustesInventarioHistorico");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DetalleAjustesInventarioHistorico",
                table: "DetalleAjustesInventarioHistorico");

            // Devolver los nombres originales a los indices historicos.
            migrationBuilder.RenameIndex(
                name: "IX_AjustesInventarioHistorico_AlmacenId_Estado",
                table: "AjustesInventarioHistorico",
                newName: "IX_AjustesInventario_AlmacenId_Estado");

            migrationBuilder.RenameIndex(
                name: "IX_AjustesInventarioHistorico_NumeroDocumento",
                table: "AjustesInventarioHistorico",
                newName: "IX_AjustesInventario_NumeroDocumento");

            migrationBuilder.RenameIndex(
                name: "IX_DetalleAjustesInventarioHistorico_AjusteInventarioId_LoteId",
                table: "DetalleAjustesInventarioHistorico",
                newName: "IX_DetalleAjustesInventario_AjusteInventarioId_LoteId");

            migrationBuilder.RenameIndex(
                name: "IX_DetalleAjustesInventarioHistorico_LoteId",
                table: "DetalleAjustesInventarioHistorico",
                newName: "IX_DetalleAjustesInventario_LoteId");

            migrationBuilder.RenameIndex(
                name: "IX_DetalleAjustesInventarioHistorico_MedicamentoId",
                table: "DetalleAjustesInventarioHistorico",
                newName: "IX_DetalleAjustesInventario_MedicamentoId");

            // Restaurar los nombres anteriores de las tablas.
            migrationBuilder.RenameTable(
                name: "AjustesInventarioHistorico",
                newName: "AjustesInventario");

            migrationBuilder.RenameTable(
                name: "DetalleAjustesInventarioHistorico",
                newName: "DetalleAjustesInventario");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AjustesInventario",
                table: "AjustesInventario",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DetalleAjustesInventario",
                table: "DetalleAjustesInventario",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AjustesInventario_Almacenes_AlmacenId",
                table: "AjustesInventario",
                column: "AlmacenId",
                principalTable: "Almacenes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleAjustesInventario_AjustesInventario_AjusteInventarioId",
                table: "DetalleAjustesInventario",
                column: "AjusteInventarioId",
                principalTable: "AjustesInventario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleAjustesInventario_Lotes_LoteId",
                table: "DetalleAjustesInventario",
                column: "LoteId",
                principalTable: "Lotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleAjustesInventario_Medicamentos_MedicamentoId",
                table: "DetalleAjustesInventario",
                column: "MedicamentoId",
                principalTable: "Medicamentos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
