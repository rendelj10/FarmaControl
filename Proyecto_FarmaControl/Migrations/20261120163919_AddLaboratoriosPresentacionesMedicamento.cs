using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Proyecto_FarmaControl.Migrations
{
    /// <inheritdoc />
    public partial class AddLaboratoriosPresentacionesMedicamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "Activo",
                table: "Medicamentos",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<int>(
                name: "LaboratorioId",
                table: "Medicamentos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioVenta",
                table: "Medicamentos",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "PresentacionId",
                table: "Medicamentos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Laboratorios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Pais = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Contacto = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Laboratorios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Presentaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CantidadContenido = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    UnidadContenido = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Presentaciones", x => x.Id);
                    table.CheckConstraint("CK_Presentaciones_CantidadContenido", "[CantidadContenido] IS NULL OR [CantidadContenido] > 0");
                });

            // Migra las presentaciones que ya existen en Medicamentos.Presentacion.
            // Se limpian espacios al inicio y al final, se omiten valores vacíos
            // y se crea una sola presentación por cada nombre distinto.
            migrationBuilder.Sql(
                """
                INSERT INTO dbo.Presentaciones (Nombre, Activo)
                SELECT DISTINCT
                    LTRIM(RTRIM(m.Presentacion)),
                    CAST(1 AS bit)
                FROM dbo.Medicamentos AS m
                WHERE m.Presentacion IS NOT NULL
                  AND LTRIM(RTRIM(m.Presentacion)) <> N''
                  AND NOT EXISTS
                  (
                      SELECT 1
                      FROM dbo.Presentaciones AS p
                      WHERE p.Nombre = LTRIM(RTRIM(m.Presentacion))
                  );

                UPDATE m
                SET m.PresentacionId = p.Id
                FROM dbo.Medicamentos AS m
                INNER JOIN dbo.Presentaciones AS p
                    ON p.Nombre = LTRIM(RTRIM(m.Presentacion))
                WHERE m.Presentacion IS NOT NULL
                  AND LTRIM(RTRIM(m.Presentacion)) <> N''
                  AND m.PresentacionId IS NULL;
                """);

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 52, "Consultar laboratorios", "laboratorio.ver" },
                    { 53, "Registrar laboratorios", "laboratorio.crear" },
                    { 54, "Actualizar laboratorios", "laboratorio.actualizar" },
                    { 55, "Desactivar laboratorios", "laboratorio.borrar" },
                    { 56, "Consultar presentaciones", "presentacion.ver" },
                    { 57, "Registrar presentaciones", "presentacion.crear" },
                    { 58, "Actualizar presentaciones", "presentacion.actualizar" },
                    { 59, "Desactivar presentaciones", "presentacion.borrar" }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 52, 2 },
                    { 53, 2 },
                    { 54, 2 },
                    { 55, 2 },
                    { 56, 2 },
                    { 57, 2 },
                    { 58, 2 },
                    { 59, 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Medicamentos_LaboratorioId",
                table: "Medicamentos",
                column: "LaboratorioId");

            migrationBuilder.CreateIndex(
                name: "IX_Medicamentos_PresentacionId",
                table: "Medicamentos",
                column: "PresentacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Presentaciones_Nombre",
                table: "Presentaciones",
                column: "Nombre",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Medicamentos_Laboratorios_LaboratorioId",
                table: "Medicamentos",
                column: "LaboratorioId",
                principalTable: "Laboratorios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Medicamentos_Presentaciones_PresentacionId",
                table: "Medicamentos",
                column: "PresentacionId",
                principalTable: "Presentaciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Medicamentos_Laboratorios_LaboratorioId",
                table: "Medicamentos");

            migrationBuilder.DropForeignKey(
                name: "FK_Medicamentos_Presentaciones_PresentacionId",
                table: "Medicamentos");

            migrationBuilder.DropTable(
                name: "Laboratorios");

            migrationBuilder.DropTable(
                name: "Presentaciones");

            migrationBuilder.DropIndex(
                name: "IX_Medicamentos_LaboratorioId",
                table: "Medicamentos");

            migrationBuilder.DropIndex(
                name: "IX_Medicamentos_PresentacionId",
                table: "Medicamentos");

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 52, 2 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 53, 2 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 54, 2 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 55, 2 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 56, 2 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 57, 2 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 58, 2 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 59, 2 });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 53);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 54);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 55);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 56);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 57);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 58);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 59);

            migrationBuilder.DropColumn(
                name: "LaboratorioId",
                table: "Medicamentos");

            migrationBuilder.DropColumn(
                name: "PrecioVenta",
                table: "Medicamentos");

            migrationBuilder.DropColumn(
                name: "PresentacionId",
                table: "Medicamentos");

            migrationBuilder.AlterColumn<bool>(
                name: "Activo",
                table: "Medicamentos",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);
        }
    }
}
