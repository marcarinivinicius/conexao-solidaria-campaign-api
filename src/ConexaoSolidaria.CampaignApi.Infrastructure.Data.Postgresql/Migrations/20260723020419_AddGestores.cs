using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConexaoSolidaria.CampaignApi.Infrastructure.Data.Postgresql.Migrations
{
    /// <inheritdoc />
    public partial class AddGestores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "gestores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NomeCompleto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SenhaHash = table.Column<string>(type: "text", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gestores", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_gestores_Email",
                table: "gestores",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gestores");
        }
    }
}
