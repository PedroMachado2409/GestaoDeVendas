using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoPedidos.Migrations
{
    /// <inheritdoc />
    public partial class AlterarVencimentoTituloParaDateTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "TitulosFinanceiros"
                ALTER COLUMN "DataVencimento" TYPE timestamp with time zone
                USING ("DataVencimento"::timestamp AT TIME ZONE 'UTC');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "TitulosFinanceiros"
                ALTER COLUMN "DataVencimento" TYPE date
                USING (("DataVencimento" AT TIME ZONE 'UTC')::date);
                """);
        }
    }
}
