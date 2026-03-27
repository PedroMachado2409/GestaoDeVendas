using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoPedidos.Migrations
{
    /// <inheritdoc />
    public partial class QuantidadeReservadaProduto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuantidadeReservada",
                table: "Produtos",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuantidadeReservada",
                table: "Produtos");
        }
    }
}
