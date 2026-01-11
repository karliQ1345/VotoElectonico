using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VotoElectonico.Migrations
{
    /// <inheritdoc />
    public partial class AddMoreTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Candidatos_PartidoPolitico_PartidoPoliticoId",
                table: "Candidatos");

            migrationBuilder.DropForeignKey(
                name: "FK_DetalleVoto_Candidatos_CandidatoId",
                table: "DetalleVoto");

            migrationBuilder.DropForeignKey(
                name: "FK_DetalleVoto_PartidoPolitico_PartidoPoliticoId",
                table: "DetalleVoto");

            migrationBuilder.DropForeignKey(
                name: "FK_DetalleVoto_Votos_VotoId",
                table: "DetalleVoto");

            migrationBuilder.DropForeignKey(
                name: "FK_PartidoPolitico_ProcesosElectorales_ProcesoElectoralId",
                table: "PartidoPolitico");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PartidoPolitico",
                table: "PartidoPolitico");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DetalleVoto",
                table: "DetalleVoto");

            migrationBuilder.RenameTable(
                name: "PartidoPolitico",
                newName: "PartidosPoliticos");

            migrationBuilder.RenameTable(
                name: "DetalleVoto",
                newName: "DetalleVotos");

            migrationBuilder.RenameIndex(
                name: "IX_PartidoPolitico_ProcesoElectoralId",
                table: "PartidosPoliticos",
                newName: "IX_PartidosPoliticos_ProcesoElectoralId");

            migrationBuilder.RenameIndex(
                name: "IX_DetalleVoto_VotoId",
                table: "DetalleVotos",
                newName: "IX_DetalleVotos_VotoId");

            migrationBuilder.RenameIndex(
                name: "IX_DetalleVoto_PartidoPoliticoId",
                table: "DetalleVotos",
                newName: "IX_DetalleVotos_PartidoPoliticoId");

            migrationBuilder.RenameIndex(
                name: "IX_DetalleVoto_CandidatoId",
                table: "DetalleVotos",
                newName: "IX_DetalleVotos_CandidatoId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PartidosPoliticos",
                table: "PartidosPoliticos",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DetalleVotos",
                table: "DetalleVotos",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "TwoFactorSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    CodigoHash = table.Column<string>(type: "text", nullable: false),
                    CreadoUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiraUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Intentos = table.Column<int>(type: "integer", nullable: false),
                    Usado = table.Column<bool>(type: "boolean", nullable: false),
                    Ip = table.Column<string>(type: "text", nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TwoFactorSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TwoFactorSessions_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TwoFactorSessions_UsuarioId_Usado_ExpiraUtc",
                table: "TwoFactorSessions",
                columns: new[] { "UsuarioId", "Usado", "ExpiraUtc" });

            migrationBuilder.AddForeignKey(
                name: "FK_Candidatos_PartidosPoliticos_PartidoPoliticoId",
                table: "Candidatos",
                column: "PartidoPoliticoId",
                principalTable: "PartidosPoliticos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleVotos_Candidatos_CandidatoId",
                table: "DetalleVotos",
                column: "CandidatoId",
                principalTable: "Candidatos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleVotos_PartidosPoliticos_PartidoPoliticoId",
                table: "DetalleVotos",
                column: "PartidoPoliticoId",
                principalTable: "PartidosPoliticos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleVotos_Votos_VotoId",
                table: "DetalleVotos",
                column: "VotoId",
                principalTable: "Votos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PartidosPoliticos_ProcesosElectorales_ProcesoElectoralId",
                table: "PartidosPoliticos",
                column: "ProcesoElectoralId",
                principalTable: "ProcesosElectorales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Candidatos_PartidosPoliticos_PartidoPoliticoId",
                table: "Candidatos");

            migrationBuilder.DropForeignKey(
                name: "FK_DetalleVotos_Candidatos_CandidatoId",
                table: "DetalleVotos");

            migrationBuilder.DropForeignKey(
                name: "FK_DetalleVotos_PartidosPoliticos_PartidoPoliticoId",
                table: "DetalleVotos");

            migrationBuilder.DropForeignKey(
                name: "FK_DetalleVotos_Votos_VotoId",
                table: "DetalleVotos");

            migrationBuilder.DropForeignKey(
                name: "FK_PartidosPoliticos_ProcesosElectorales_ProcesoElectoralId",
                table: "PartidosPoliticos");

            migrationBuilder.DropTable(
                name: "TwoFactorSessions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PartidosPoliticos",
                table: "PartidosPoliticos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DetalleVotos",
                table: "DetalleVotos");

            migrationBuilder.RenameTable(
                name: "PartidosPoliticos",
                newName: "PartidoPolitico");

            migrationBuilder.RenameTable(
                name: "DetalleVotos",
                newName: "DetalleVoto");

            migrationBuilder.RenameIndex(
                name: "IX_PartidosPoliticos_ProcesoElectoralId",
                table: "PartidoPolitico",
                newName: "IX_PartidoPolitico_ProcesoElectoralId");

            migrationBuilder.RenameIndex(
                name: "IX_DetalleVotos_VotoId",
                table: "DetalleVoto",
                newName: "IX_DetalleVoto_VotoId");

            migrationBuilder.RenameIndex(
                name: "IX_DetalleVotos_PartidoPoliticoId",
                table: "DetalleVoto",
                newName: "IX_DetalleVoto_PartidoPoliticoId");

            migrationBuilder.RenameIndex(
                name: "IX_DetalleVotos_CandidatoId",
                table: "DetalleVoto",
                newName: "IX_DetalleVoto_CandidatoId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PartidoPolitico",
                table: "PartidoPolitico",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DetalleVoto",
                table: "DetalleVoto",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Candidatos_PartidoPolitico_PartidoPoliticoId",
                table: "Candidatos",
                column: "PartidoPoliticoId",
                principalTable: "PartidoPolitico",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleVoto_Candidatos_CandidatoId",
                table: "DetalleVoto",
                column: "CandidatoId",
                principalTable: "Candidatos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleVoto_PartidoPolitico_PartidoPoliticoId",
                table: "DetalleVoto",
                column: "PartidoPoliticoId",
                principalTable: "PartidoPolitico",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleVoto_Votos_VotoId",
                table: "DetalleVoto",
                column: "VotoId",
                principalTable: "Votos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PartidoPolitico_ProcesosElectorales_ProcesoElectoralId",
                table: "PartidoPolitico",
                column: "ProcesoElectoralId",
                principalTable: "ProcesosElectorales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
