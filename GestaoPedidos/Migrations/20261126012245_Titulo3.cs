using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoPedidos.Migrations
{
    /// <inheritdoc />
    public partial class Titulo3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NomeOrigem",
                table: "TitulosFinanceiros");

            migrationBuilder.AddColumn<int>(
                name: "OrigemTitulo",
                table: "TitulosFinanceiros",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrigemTitulo",
                table: "TitulosFinanceiros");

            migrationBuilder.AddColumn<string>(
                name: "NomeOrigem",
                table: "TitulosFinanceiros",
                type: "text",
                nullable: true);
        }
    }
}
