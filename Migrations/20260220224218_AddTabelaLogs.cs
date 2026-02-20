using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MGMBlazor.web.Migrations
{
    /// <inheritdoc />
    public partial class AddTabelaLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LogsAuditoria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DataHora = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Usuario = table.Column<string>(type: "text", nullable: false),
                    Operacao = table.Column<string>(type: "text", nullable: false),
                    Detalhes = table.Column<string>(type: "text", nullable: false),
                    Tela = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogsAuditoria", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LogsAuditoria");
        }
    }
}
