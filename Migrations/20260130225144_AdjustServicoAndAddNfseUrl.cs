using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MGMBlazor.web.Migrations
{
    /// <inheritdoc />
    public partial class AdjustServicoAndAddNfseUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Valor",
                table: "Servicos");

            migrationBuilder.AddColumn<string>(
                name: "LinkPdf",
                table: "NotasFiscaisEmitidas",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LinkPdf",
                table: "NotasFiscaisEmitidas");

            migrationBuilder.AddColumn<decimal>(
                name: "Valor",
                table: "Servicos",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
