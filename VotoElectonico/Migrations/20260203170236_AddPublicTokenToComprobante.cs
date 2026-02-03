using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VotoElectonico.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicTokenToComprobante : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PublicToken",
                table: "ComprobantesVoto",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublicTokenExpiraUtc",
                table: "ComprobantesVoto",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublicToken",
                table: "ComprobantesVoto");

            migrationBuilder.DropColumn(
                name: "PublicTokenExpiraUtc",
                table: "ComprobantesVoto");
        }
    }
}
