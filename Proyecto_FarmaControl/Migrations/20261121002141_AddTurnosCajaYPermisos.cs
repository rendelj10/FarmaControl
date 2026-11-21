using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Proyecto_FarmaControl.Migrations
{
    /// <inheritdoc />
    public partial class AddTurnosCajaYPermisos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TurnosCaja",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SucursalId = table.Column<int>(type: "int", nullable: false),
                    UsuarioAperturaId = table.Column<int>(type: "int", nullable: false),
                    UsuarioCierreId = table.Column<int>(type: "int", nullable: true),
                    FechaApertura = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaCierre = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MontoInicial = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontoEsperado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    MontoContado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Diferencia = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TurnosCaja", x => x.Id);
                    table.CheckConstraint("CK_TurnosCaja_Estado", "[Estado] IN (N'Abierto', N'Cerrado')");
                    table.CheckConstraint("CK_TurnosCaja_MontoContado", "[MontoContado] IS NULL OR [MontoContado] >= 0");
                    table.CheckConstraint("CK_TurnosCaja_MontoEsperado", "[MontoEsperado] IS NULL OR [MontoEsperado] >= 0");
                    table.CheckConstraint("CK_TurnosCaja_MontoInicial", "[MontoInicial] >= 0");
                    table.ForeignKey(
                        name: "FK_TurnosCaja_Sucursales_SucursalId",
                        column: x => x.SucursalId,
                        principalTable: "Sucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TurnosCaja_Usuarios_UsuarioAperturaId",
                        column: x => x.UsuarioAperturaId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TurnosCaja_Usuarios_UsuarioCierreId",
                        column: x => x.UsuarioCierreId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 64, "Consultar turnos de caja", "turnocaja.ver" },
                    { 65, "Abrir turnos de caja", "turnocaja.abrir" },
                    { 66, "Cerrar turnos de caja", "turnocaja.cerrar" }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 64, 2 },
                    { 65, 2 },
                    { 66, 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TurnosCaja_SucursalId_FechaApertura",
                table: "TurnosCaja",
                columns: new[] { "SucursalId", "FechaApertura" });

            migrationBuilder.CreateIndex(
                name: "IX_TurnosCaja_UsuarioAperturaId",
                table: "TurnosCaja",
                column: "UsuarioAperturaId",
                unique: true,
                filter: "[Estado] = N'Abierto'");

            migrationBuilder.CreateIndex(
                name: "IX_TurnosCaja_UsuarioCierreId",
                table: "TurnosCaja",
                column: "UsuarioCierreId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TurnosCaja");

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 64, 2 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 65, 2 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 66, 2 });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 64);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 65);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 66);
        }
    }
}
