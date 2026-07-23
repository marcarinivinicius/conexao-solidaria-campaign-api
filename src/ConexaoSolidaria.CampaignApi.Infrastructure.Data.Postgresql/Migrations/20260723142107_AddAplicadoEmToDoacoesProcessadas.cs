using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConexaoSolidaria.CampaignApi.Infrastructure.Data.Postgresql.Migrations
{
    /// <inheritdoc />
    public partial class AddAplicadoEmToDoacoesProcessadas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AplicadoEm",
                table: "doacoes_processadas",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AplicadoEm",
                table: "doacoes_processadas");
        }
    }
}
