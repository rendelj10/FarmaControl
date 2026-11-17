using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Proyecto_FarmaControl.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicamentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Medicamentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PrincipioActivo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Concentracion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FormaFarmaceutica = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Presentacion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CategoriaId = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medicamentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Medicamentos_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Medicamentos_CategoriaId",
                table: "Medicamentos",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Medicamentos_Codigo",
                table: "Medicamentos",
                column: "Codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Medicamentos");
        }
    }
}
