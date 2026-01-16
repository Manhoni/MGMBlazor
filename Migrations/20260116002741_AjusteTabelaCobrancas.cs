using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MGMBlazor.web.Migrations
{
    /// <inheritdoc />
    public partial class AjusteTabelaCobrancas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cobrancas_NotasFiscaisEmitidas_NotaFiscalId",
                table: "Cobrancas");

            migrationBuilder.RenameColumn(
                name: "NotaFiscalId",
                table: "Cobrancas",
                newName: "NotaFiscalEmitidaId");

            migrationBuilder.RenameIndex(
                name: "IX_Cobrancas_NotaFiscalId",
                table: "Cobrancas",
                newName: "IX_Cobrancas_NotaFiscalEmitidaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cobrancas_NotasFiscaisEmitidas_NotaFiscalEmitidaId",
                table: "Cobrancas",
                column: "NotaFiscalEmitidaId",
                principalTable: "NotasFiscaisEmitidas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cobrancas_NotasFiscaisEmitidas_NotaFiscalEmitidaId",
                table: "Cobrancas");

            migrationBuilder.RenameColumn(
                name: "NotaFiscalEmitidaId",
                table: "Cobrancas",
                newName: "NotaFiscalId");

            migrationBuilder.RenameIndex(
                name: "IX_Cobrancas_NotaFiscalEmitidaId",
                table: "Cobrancas",
                newName: "IX_Cobrancas_NotaFiscalId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cobrancas_NotasFiscaisEmitidas_NotaFiscalId",
                table: "Cobrancas",
                column: "NotaFiscalId",
                principalTable: "NotasFiscaisEmitidas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
