using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoPedidos.Migrations
{
    /// <inheritdoc />
    public partial class TipoMovimentacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Versao",
                table: "Produtos");

            migrationBuilder.AddColumn<int>(
                name: "QuantidadeCompradaPendente",
                table: "Produtos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TipoMovimentacao",
                table: "Pedidos",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuantidadeCompradaPendente",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "TipoMovimentacao",
                table: "Pedidos");

            migrationBuilder.AddColumn<Guid>(
                name: "Versao",
                table: "Produtos",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");
        }
    }
}
