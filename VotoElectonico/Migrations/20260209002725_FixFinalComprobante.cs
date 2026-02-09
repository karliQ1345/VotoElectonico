using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VotoElectonico.Migrations
{
    /// <inheritdoc />
    public partial class FixFinalComprobante : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ComprobantesVoto_PublicToken",
                table: "ComprobantesVoto",
                column: "PublicToken",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ComprobantesVoto_PublicToken",
                table: "ComprobantesVoto");
        }
    }
}
