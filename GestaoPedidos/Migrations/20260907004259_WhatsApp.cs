using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GestaoPedidos.Migrations
{
    /// <inheritdoc />
    public partial class WhatsApp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MensagensWhatsApp",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdMensagem = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Remetente = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    NomeRemetente = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    TipoMensagem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Texto = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    DataMensagem = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataRecebimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MensagensWhatsApp", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MensagensWhatsApp_IdMensagem",
                table: "MensagensWhatsApp",
                column: "IdMensagem",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MensagensWhatsApp");
        }
    }
}
