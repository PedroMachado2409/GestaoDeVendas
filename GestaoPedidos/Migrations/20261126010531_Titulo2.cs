using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoPedidos.Migrations
{
    /// <inheritdoc />
    public partial class Titulo2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdOrigem",
                table: "TitulosFinanceiros",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomeOrigem",
                table: "TitulosFinanceiros",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdOrigem",
                table: "TitulosFinanceiros");

            migrationBuilder.DropColumn(
                name: "NomeOrigem",
                table: "TitulosFinanceiros");
        }
    }
}
