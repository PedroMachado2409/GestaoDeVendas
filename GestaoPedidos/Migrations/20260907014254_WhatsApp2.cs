using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoPedidos.Migrations
{
    /// <inheritdoc />
    public partial class WhatsApp2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataRecebimento",
                table: "MensagensWhatsApp");

            migrationBuilder.DropColumn(
                name: "Texto",
                table: "MensagensWhatsApp");

            migrationBuilder.RenameColumn(
                name: "Remetente",
                table: "MensagensWhatsApp",
                newName: "NumeroRemetente");

            migrationBuilder.RenameColumn(
                name: "IdMensagem",
                table: "MensagensWhatsApp",
                newName: "IdMensagemWhatsApp");

            migrationBuilder.RenameIndex(
                name: "IX_MensagensWhatsApp_IdMensagem",
                table: "MensagensWhatsApp",
                newName: "IX_MensagensWhatsApp_IdMensagemWhatsApp");

            migrationBuilder.AlterColumn<string>(
                name: "NomeRemetente",
                table: "MensagensWhatsApp",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Conteudo",
                table: "MensagensWhatsApp",
                type: "character varying(5000)",
                maxLength: 5000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumberId",
                table: "MensagensWhatsApp",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_MensagensWhatsApp_DataMensagem",
                table: "MensagensWhatsApp",
                column: "DataMensagem");

            migrationBuilder.CreateIndex(
                name: "IX_MensagensWhatsApp_NumeroRemetente",
                table: "MensagensWhatsApp",
                column: "NumeroRemetente");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MensagensWhatsApp_DataMensagem",
                table: "MensagensWhatsApp");

            migrationBuilder.DropIndex(
                name: "IX_MensagensWhatsApp_NumeroRemetente",
                table: "MensagensWhatsApp");

            migrationBuilder.DropColumn(
                name: "Conteudo",
                table: "MensagensWhatsApp");

            migrationBuilder.DropColumn(
                name: "PhoneNumberId",
                table: "MensagensWhatsApp");

            migrationBuilder.RenameColumn(
                name: "NumeroRemetente",
                table: "MensagensWhatsApp",
                newName: "Remetente");

            migrationBuilder.RenameColumn(
                name: "IdMensagemWhatsApp",
                table: "MensagensWhatsApp",
                newName: "IdMensagem");

            migrationBuilder.RenameIndex(
                name: "IX_MensagensWhatsApp_IdMensagemWhatsApp",
                table: "MensagensWhatsApp",
                newName: "IX_MensagensWhatsApp_IdMensagem");

            migrationBuilder.AlterColumn<string>(
                name: "NomeRemetente",
                table: "MensagensWhatsApp",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataRecebimento",
                table: "MensagensWhatsApp",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Texto",
                table: "MensagensWhatsApp",
                type: "character varying(5000)",
                maxLength: 5000,
                nullable: true);
        }
    }
}
