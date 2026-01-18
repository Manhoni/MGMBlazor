using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MGMBlazor.web.Migrations
{
    /// <inheritdoc />
    public partial class SepararRpsDeVendaId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RpsNumero",
                table: "NotasFiscaisEmitidas",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RpsNumero",
                table: "NotasFiscaisEmitidas");
        }
    }
}
