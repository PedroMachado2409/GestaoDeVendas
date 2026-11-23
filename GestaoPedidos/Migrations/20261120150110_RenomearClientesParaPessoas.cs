using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoPedidos.Migrations
{
    public partial class RenomearClientesParaPessoas : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Clientes_ClienteId", table: "Pedidos");
            migrationBuilder.DropPrimaryKey(name: "PK_Clientes", table: "Clientes");

            migrationBuilder.RenameTable(name: "Clientes", newName: "Pessoas");
            migrationBuilder.RenameIndex(
                name: "IX_Clientes_Cpf", table: "Pessoas", newName: "IX_Pessoas_Cpf");
            migrationBuilder.RenameIndex(
                name: "IX_Clientes_Email", table: "Pessoas", newName: "IX_Pessoas_Email");

            migrationBuilder.AddColumn<int>(
                name: "TipoPessoa", table: "Pessoas", type: "integer",
                nullable: false, defaultValue: 0);

            migrationBuilder.AddPrimaryKey(name: "PK_Pessoas", table: "Pessoas", column: "Id");
            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Pessoas_ClienteId", table: "Pedidos", column: "ClienteId",
                principalTable: "Pessoas", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Pessoas_ClienteId", table: "Pedidos");
            migrationBuilder.DropPrimaryKey(name: "PK_Pessoas", table: "Pessoas");
            migrationBuilder.DropColumn(name: "TipoPessoa", table: "Pessoas");

            migrationBuilder.RenameTable(name: "Pessoas", newName: "Clientes");
            migrationBuilder.RenameIndex(
                name: "IX_Pessoas_Cpf", table: "Clientes", newName: "IX_Clientes_Cpf");
            migrationBuilder.RenameIndex(
                name: "IX_Pessoas_Email", table: "Clientes", newName: "IX_Clientes_Email");

            migrationBuilder.AddPrimaryKey(name: "PK_Clientes", table: "Clientes", column: "Id");
            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Clientes_ClienteId", table: "Pedidos", column: "ClienteId",
                principalTable: "Clientes", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        }
    }
}
