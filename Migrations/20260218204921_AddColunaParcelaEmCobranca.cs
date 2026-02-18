using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MGMBlazor.web.Migrations
{
    /// <inheritdoc />
    public partial class AddColunaParcelaEmCobranca : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumeroParcela",
                table: "Cobrancas",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumeroParcela",
                table: "Cobrancas");
        }
    }
}
