using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Proyecto_FarmaControl.Migrations
{
    /// <inheritdoc />
    public partial class RelacionLotesAlmacenes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Lotes_MedicamentoId",
                table: "Lotes");

            migrationBuilder.DropColumn(
                name: "Almacen",
                table: "Lotes");

            migrationBuilder.AddColumn<int>(
                name: "AlmacenId",
                table: "Lotes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Lotes_AlmacenId",
                table: "Lotes",
                column: "AlmacenId");

            migrationBuilder.CreateIndex(
                name: "IX_Lotes_MedicamentoId_AlmacenId_NumeroLote",
                table: "Lotes",
                columns: new[] { "MedicamentoId", "AlmacenId", "NumeroLote" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Lotes_Almacenes_AlmacenId",
                table: "Lotes",
                column: "AlmacenId",
                principalTable: "Almacenes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lotes_Almacenes_AlmacenId",
                table: "Lotes");

            migrationBuilder.DropIndex(
                name: "IX_Lotes_AlmacenId",
                table: "Lotes");

            migrationBuilder.DropIndex(
                name: "IX_Lotes_MedicamentoId_AlmacenId_NumeroLote",
                table: "Lotes");

            migrationBuilder.DropColumn(
                name: "AlmacenId",
                table: "Lotes");

            migrationBuilder.AddColumn<string>(
                name: "Almacen",
                table: "Lotes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Lotes_MedicamentoId",
                table: "Lotes",
                column: "MedicamentoId");
        }
    }
}
