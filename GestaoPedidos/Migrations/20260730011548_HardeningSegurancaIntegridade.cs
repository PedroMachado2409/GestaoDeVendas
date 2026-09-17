using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoPedidos.Migrations
{
    /// <inheritdoc />
    public partial class HardeningSegurancaIntegridade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "Usuarios"
                SET "Email" = lower(btrim("Email"));

                UPDATE "Clientes"
                SET "Email" = lower(btrim("Email")),
                    "Cpf" = regexp_replace("Cpf", '\D', '', 'g');

                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM "Usuarios"
                        GROUP BY "Email" HAVING count(*) > 1
                    ) THEN
                        RAISE EXCEPTION 'Existem e-mails duplicados em Usuarios. Corrija-os antes da migração.';
                    END IF;

                    IF EXISTS (
                        SELECT 1 FROM "Clientes"
                        GROUP BY "Email" HAVING count(*) > 1
                    ) THEN
                        RAISE EXCEPTION 'Existem e-mails duplicados em Clientes. Corrija-os antes da migração.';
                    END IF;

                    IF EXISTS (
                        SELECT 1 FROM "Clientes"
                        GROUP BY "Cpf" HAVING count(*) > 1
                    ) THEN
                        RAISE EXCEPTION 'Existem CPFs duplicados em Clientes. Corrija-os antes da migração.';
                    END IF;

                    IF EXISTS (
                        SELECT 1 FROM "Clientes" WHERE length("Cpf") <> 11
                    ) THEN
                        RAISE EXCEPTION 'Existem CPFs inválidos em Clientes.';
                    END IF;

                    IF EXISTS (
                        SELECT 1 FROM "Produtos"
                        WHERE "Estoque" < 0 OR "QuantidadeReservada" < 0 OR "Preco" <= 0
                    ) THEN
                        RAISE EXCEPTION 'Existem produtos com estoque, reserva ou preço inválidos.';
                    END IF;

                    IF EXISTS (
                        SELECT 1 FROM "PedidoItens"
                        WHERE "Quantidade" <= 0 OR "Preco" <= 0
                    ) THEN
                        RAISE EXCEPTION 'Existem itens de pedido com quantidade ou preço inválidos.';
                    END IF;

                    IF EXISTS (
                        SELECT 1 FROM "PedidoItens"
                        GROUP BY "PedidoId", "ProdutoId" HAVING count(*) > 1
                    ) THEN
                        RAISE EXCEPTION 'Existem produtos repetidos no mesmo pedido.';
                    END IF;

                    IF EXISTS (
                        SELECT 1
                        FROM "Pedidos" p
                        LEFT JOIN "Clientes" c ON c."Id" = p."ClienteId"
                        WHERE c."Id" IS NULL
                    ) THEN
                        RAISE EXCEPTION 'Existem pedidos associados a clientes inexistentes.';
                    END IF;

                    IF EXISTS (
                        SELECT 1
                        FROM "PedidoItens" i
                        LEFT JOIN "Produtos" p ON p."Id" = i."ProdutoId"
                        WHERE p."Id" IS NULL
                    ) THEN
                        RAISE EXCEPTION 'Existem itens associados a produtos inexistentes.';
                    END IF;

                    IF EXISTS (
                        SELECT 1 FROM "Usuarios"
                        WHERE length("Nome") > 120 OR length("Email") > 254 OR length("Senha") > 100
                    ) OR EXISTS (
                        SELECT 1 FROM "Clientes"
                        WHERE length("Nome") > 120 OR length("Email") > 254
                    ) OR EXISTS (
                        SELECT 1 FROM "Produtos"
                        WHERE length("Nome") > 160 OR length("Marca") > 120
                    ) THEN
                        RAISE EXCEPTION 'Existem textos maiores que os limites do novo modelo.';
                    END IF;
                END $$;
                """);

            migrationBuilder.DropIndex(
                name: "IX_PedidoItens_PedidoId",
                table: "PedidoItens");

            migrationBuilder.AlterColumn<string>(
                name: "Senha",
                table: "Usuarios",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Usuarios",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Usuarios",
                type: "character varying(254)",
                maxLength: 254,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "VersaoToken",
                table: "Usuarios",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AlterColumn<decimal>(
                name: "Preco",
                table: "Produtos",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Produtos",
                type: "character varying(160)",
                maxLength: 160,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Marca",
                table: "Produtos",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "Versao",
                table: "Produtos",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()");

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Clientes",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Clientes",
                type: "character varying(254)",
                maxLength: 254,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Cpf",
                table: "Clientes",
                type: "character(11)",
                fixedLength: true,
                maxLength: 11,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Produtos_Estoque_NaoNegativo",
                table: "Produtos",
                sql: "\"Estoque\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Produtos_Preco_Positivo",
                table: "Produtos",
                sql: "\"Preco\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Produtos_Reserva_NaoNegativa",
                table: "Produtos",
                sql: "\"QuantidadeReservada\" >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_ClienteId",
                table: "Pedidos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoItens_PedidoId_ProdutoId",
                table: "PedidoItens",
                columns: new[] { "PedidoId", "ProdutoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PedidoItens_ProdutoId",
                table: "PedidoItens",
                column: "ProdutoId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PedidoItens_Preco_Positivo",
                table: "PedidoItens",
                sql: "\"Preco\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PedidoItens_Quantidade_Positiva",
                table: "PedidoItens",
                sql: "\"Quantidade\" > 0");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Cpf",
                table: "Clientes",
                column: "Cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Email",
                table: "Clientes",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PedidoItens_Produtos_ProdutoId",
                table: "PedidoItens",
                column: "ProdutoId",
                principalTable: "Produtos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Clientes_ClienteId",
                table: "Pedidos",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PedidoItens_Produtos_ProdutoId",
                table: "PedidoItens");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Clientes_ClienteId",
                table: "Pedidos");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Produtos_Estoque_NaoNegativo",
                table: "Produtos");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Produtos_Preco_Positivo",
                table: "Produtos");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Produtos_Reserva_NaoNegativa",
                table: "Produtos");

            migrationBuilder.DropIndex(
                name: "IX_Pedidos_ClienteId",
                table: "Pedidos");

            migrationBuilder.DropIndex(
                name: "IX_PedidoItens_PedidoId_ProdutoId",
                table: "PedidoItens");

            migrationBuilder.DropIndex(
                name: "IX_PedidoItens_ProdutoId",
                table: "PedidoItens");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PedidoItens_Preco_Positivo",
                table: "PedidoItens");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PedidoItens_Quantidade_Positiva",
                table: "PedidoItens");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_Cpf",
                table: "Clientes");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_Email",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "VersaoToken",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Versao",
                table: "Produtos");

            migrationBuilder.AlterColumn<string>(
                name: "Senha",
                table: "Usuarios",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Usuarios",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(120)",
                oldMaxLength: 120);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Usuarios",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(254)",
                oldMaxLength: 254);

            migrationBuilder.AlterColumn<decimal>(
                name: "Preco",
                table: "Produtos",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Produtos",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(160)",
                oldMaxLength: 160);

            migrationBuilder.AlterColumn<string>(
                name: "Marca",
                table: "Produtos",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(120)",
                oldMaxLength: 120);

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Clientes",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(120)",
                oldMaxLength: 120);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Clientes",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(254)",
                oldMaxLength: 254);

            migrationBuilder.AlterColumn<string>(
                name: "Cpf",
                table: "Clientes",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character(11)",
                oldFixedLength: true,
                oldMaxLength: 11);

            migrationBuilder.CreateIndex(
                name: "IX_PedidoItens_PedidoId",
                table: "PedidoItens",
                column: "PedidoId");
        }
    }
}
