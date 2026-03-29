using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoPedidos.Migrations
{
    /// <inheritdoc />
    public partial class Origem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NomeOrigem",
                table: "Titulos",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NomeOrigem",
                table: "Titulos");
        }
    }
}
