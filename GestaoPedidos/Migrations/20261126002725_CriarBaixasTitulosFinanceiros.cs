using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GestaoPedidos.Migrations
{
    /// <inheritdoc />
    public partial class CriarBaixasTitulosFinanceiros : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BaixasTitulosFinanceiros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TituloFinanceiroId = table.Column<int>(type: "integer", nullable: false),
                    OperacaoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    Forma = table.Column<int>(type: "integer", nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Estornada = table.Column<bool>(type: "boolean", nullable: false),
                    DataEstorno = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioEstornoId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaixasTitulosFinanceiros", x => x.Id);
                    table.CheckConstraint("CK_BaixasTitulosFinanceiros_Valor_Positivo", "\"Valor\" > 0");
                    table.ForeignKey(
                        name: "FK_BaixasTitulosFinanceiros_TitulosFinanceiros_TituloFinanceir~",
                        column: x => x.TituloFinanceiroId,
                        principalTable: "TitulosFinanceiros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BaixasTitulosFinanceiros_Usuarios_UsuarioEstornoId",
                        column: x => x.UsuarioEstornoId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BaixasTitulosFinanceiros_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BaixasTitulosFinanceiros_TituloFinanceiroId_OperacaoId",
                table: "BaixasTitulosFinanceiros",
                columns: new[] { "TituloFinanceiroId", "OperacaoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BaixasTitulosFinanceiros_UsuarioEstornoId",
                table: "BaixasTitulosFinanceiros",
                column: "UsuarioEstornoId");

            migrationBuilder.CreateIndex(
                name: "IX_BaixasTitulosFinanceiros_UsuarioId",
                table: "BaixasTitulosFinanceiros",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BaixasTitulosFinanceiros");
        }
    }
}
