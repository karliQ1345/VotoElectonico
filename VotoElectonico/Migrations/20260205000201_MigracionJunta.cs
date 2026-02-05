using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VotoElectonico.Migrations
{
    /// <inheritdoc />
    public partial class MigracionJunta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Cerrada",
                table: "Juntas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "CerradaPorJefeId",
                table: "Juntas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CerradaUtc",
                table: "Juntas",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cerrada",
                table: "Juntas");

            migrationBuilder.DropColumn(
                name: "CerradaPorJefeId",
                table: "Juntas");

            migrationBuilder.DropColumn(
                name: "CerradaUtc",
                table: "Juntas");
        }
    }
}
