using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MGMBlazor.web.Migrations
{
    /// <inheritdoc />
    public partial class TornarNotaOpcional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cobrancas_NotasFiscaisEmitidas_NotaFiscalEmitidaId",
                table: "Cobrancas");

            migrationBuilder.AlterColumn<int>(
                name: "NotaFiscalEmitidaId",
                table: "Cobrancas",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Cobrancas_NotasFiscaisEmitidas_NotaFiscalEmitidaId",
                table: "Cobrancas",
                column: "NotaFiscalEmitidaId",
                principalTable: "NotasFiscaisEmitidas",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cobrancas_NotasFiscaisEmitidas_NotaFiscalEmitidaId",
                table: "Cobrancas");

            migrationBuilder.AlterColumn<int>(
                name: "NotaFiscalEmitidaId",
                table: "Cobrancas",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Cobrancas_NotasFiscaisEmitidas_NotaFiscalEmitidaId",
                table: "Cobrancas",
                column: "NotaFiscalEmitidaId",
                principalTable: "NotasFiscaisEmitidas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
