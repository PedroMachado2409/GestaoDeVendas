using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoPedidos.Migrations
{
    /// <inheritdoc />
    public partial class RestauraConcorrenciaEOrigemMovimentacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimentacoesEstoque_Produtos_ProdutoId",
                table: "MovimentacoesEstoque");

            migrationBuilder.DropIndex(
                name: "IX_MovimentacoesEstoque_ProdutoId",
                table: "MovimentacoesEstoque");

            // Verificação antes de qualquer alteração: as restrições criadas no
            // fim desta migração falhariam de forma abrupta se a base já tiver
            // registros inválidos. Melhor interromper com mensagem clara.
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM "Produtos" WHERE "QuantidadeCompradaPendente" < 0
                    ) THEN
                        RAISE EXCEPTION 'Existem produtos com QuantidadeCompradaPendente negativa. Corrija-os antes da migração.';
                    END IF;

                    IF EXISTS (
                        SELECT 1 FROM "MovimentacoesEstoque" WHERE "Quantidade" <= 0
                    ) THEN
                        RAISE EXCEPTION 'Existem movimentações de estoque com quantidade não positiva. Corrija-as antes da migração.';
                    END IF;
                END $$;
                """);

            // A coluna nova entra ANTES de a antiga sair, para que a conversão
            // possa ler o texto de origem. Na ordem gerada pelo EF, o
            // NomeOrigem era descartado e toda movimentação existente viraria
            // PedidoDeVenda pelo valor padrão 0.
            migrationBuilder.AddColumn<int>(
                name: "OrigemMovimentacao",
                table: "MovimentacoesEstoque",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                UPDATE "MovimentacoesEstoque"
                SET "OrigemMovimentacao" = CASE
                    WHEN "NomeOrigem" = 'PedidoDeCompra' THEN 1
                    ELSE 0
                END;
                """);

            migrationBuilder.DropColumn(
                name: "NomeOrigem",
                table: "MovimentacoesEstoque");

            migrationBuilder.AddColumn<Guid>(
                name: "Versao",
                table: "Produtos",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AlterColumn<string>(
                name: "ProdutoNome",
                table: "MovimentacoesEstoque",
                type: "character varying(160)",
                maxLength: 160,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Observacao",
                table: "MovimentacoesEstoque",
                type: "character varying(400)",
                maxLength: 400,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Produtos_CompraPendente_NaoNegativa",
                table: "Produtos",
                sql: "\"QuantidadeCompradaPendente\" >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_ProdutoId_DataMovimentacao",
                table: "MovimentacoesEstoque",
                columns: new[] { "ProdutoId", "DataMovimentacao" },
                descending: new[] { false, true });

            migrationBuilder.AddCheckConstraint(
                name: "CK_MovimentacoesEstoque_Quantidade_Positiva",
                table: "MovimentacoesEstoque",
                sql: "\"Quantidade\" > 0");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimentacoesEstoque_Produtos_ProdutoId",
                table: "MovimentacoesEstoque",
                column: "ProdutoId",
                principalTable: "Produtos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimentacoesEstoque_Produtos_ProdutoId",
                table: "MovimentacoesEstoque");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Produtos_CompraPendente_NaoNegativa",
                table: "Produtos");

            migrationBuilder.DropIndex(
                name: "IX_MovimentacoesEstoque_ProdutoId_DataMovimentacao",
                table: "MovimentacoesEstoque");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MovimentacoesEstoque_Quantidade_Positiva",
                table: "MovimentacoesEstoque");

            migrationBuilder.DropColumn(
                name: "Versao",
                table: "Produtos");

            migrationBuilder.AlterColumn<string>(
                name: "ProdutoNome",
                table: "MovimentacoesEstoque",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(160)",
                oldMaxLength: 160);

            migrationBuilder.AlterColumn<string>(
                name: "Observacao",
                table: "MovimentacoesEstoque",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(400)",
                oldMaxLength: 400);

            migrationBuilder.AddColumn<string>(
                name: "NomeOrigem",
                table: "MovimentacoesEstoque",
                type: "text",
                nullable: false,
                defaultValue: "");

            // Converte de volta antes de descartar a coluna de origem, para que
            // a reversão não deixe o histórico com NomeOrigem em branco.
            migrationBuilder.Sql("""
                UPDATE "MovimentacoesEstoque"
                SET "NomeOrigem" = CASE
                    WHEN "OrigemMovimentacao" = 1 THEN 'PedidoDeCompra'
                    ELSE 'PedidoDeVenda'
                END;
                """);

            migrationBuilder.DropColumn(
                name: "OrigemMovimentacao",
                table: "MovimentacoesEstoque");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_ProdutoId",
                table: "MovimentacoesEstoque",
                column: "ProdutoId");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimentacoesEstoque_Produtos_ProdutoId",
                table: "MovimentacoesEstoque",
                column: "ProdutoId",
                principalTable: "Produtos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
