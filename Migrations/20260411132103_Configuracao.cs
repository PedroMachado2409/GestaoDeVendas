using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GestaoPedidos.Migrations
{
    /// <inheritdoc />
    public partial class Configuracao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Configuracoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NomeLoja = table.Column<string>(type: "text", nullable: false),
                    EmailOrigem = table.Column<string>(type: "text", nullable: false),
                    Smtp = table.Column<string>(type: "text", nullable: false),
                    Usuario = table.Column<string>(type: "text", nullable: false),
                    Senha = table.Column<string>(type: "text", nullable: false),
                    Porta = table.Column<int>(type: "integer", nullable: false),
                    ConexaoSSl = table.Column<bool>(type: "boolean", nullable: false),
                    PermiteEstoqueNegativo = table.Column<bool>(type: "boolean", nullable: false),
                    HabilitaEnvioDeEmail = table.Column<bool>(type: "boolean", nullable: false),
                    EnviaXMLPorEmail = table.Column<bool>(type: "boolean", nullable: false),
                    GeraXmlDoPedido = table.Column<string>(type: "text", nullable: false),
                    DiretorioXML = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configuracoes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PedidoItens_ProdutoId",
                table: "PedidoItens",
                column: "ProdutoId");

            migrationBuilder.AddForeignKey(
                name: "FK_PedidoItens_Produtos_ProdutoId",
                table: "PedidoItens",
                column: "ProdutoId",
                principalTable: "Produtos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PedidoItens_Produtos_ProdutoId",
                table: "PedidoItens");

            migrationBuilder.DropTable(
                name: "Configuracoes");

            migrationBuilder.DropIndex(
                name: "IX_PedidoItens_ProdutoId",
                table: "PedidoItens");
        }
    }
}
