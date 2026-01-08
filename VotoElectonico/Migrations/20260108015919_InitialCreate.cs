using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VotoElectonico.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcesosElectorales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Titulo = table.Column<string>(type: "text", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    ModalidadPresidencial = table.Column<int>(type: "integer", nullable: true),
                    PermitePlancha = table.Column<bool>(type: "boolean", nullable: false),
                    PermiteNominal = table.Column<bool>(type: "boolean", nullable: false),
                    MaxSeleccionNominal = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcesosElectorales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Cedula = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Rol = table.Column<int>(type: "integer", nullable: false),
                    CorreoElectronico = table.Column<string>(type: "text", nullable: false),
                    NombresCompletos = table.Column<string>(type: "text", nullable: false),
                    Genero = table.Column<int>(type: "integer", nullable: false),
                    Provincia = table.Column<string>(type: "text", nullable: false),
                    Canton = table.Column<string>(type: "text", nullable: false),
                    Parroquia = table.Column<string>(type: "text", nullable: false),
                    FotoUrl = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PartidoPolitico",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NombreLista = table.Column<string>(type: "text", nullable: false),
                    NumeroLista = table.Column<int>(type: "integer", nullable: false),
                    LogoUrl = table.Column<string>(type: "text", nullable: true),
                    ProcesoElectoralId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartidoPolitico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartidoPolitico_ProcesosElectorales_ProcesoElectoralId",
                        column: x => x.ProcesoElectoralId,
                        principalTable: "ProcesosElectorales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Votos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProcesoElectoralId = table.Column<int>(type: "integer", nullable: false),
                    FechaIngresoUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HashSeguridad = table.Column<string>(type: "text", nullable: false),
                    GeneroVotante = table.Column<int>(type: "integer", nullable: true),
                    ProvinciaVotante = table.Column<string>(type: "text", nullable: true),
                    CantonVotante = table.Column<string>(type: "text", nullable: true),
                    ParroquiaVotante = table.Column<string>(type: "text", nullable: true),
                    PayloadCifradoBase64 = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Votos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Votos_ProcesosElectorales_ProcesoElectoralId",
                        column: x => x.ProcesoElectoralId,
                        principalTable: "ProcesosElectorales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HistorialVotaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    ProcesoElectoralId = table.Column<int>(type: "integer", nullable: false),
                    FechaSufragioUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CodigoCertificado = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialVotaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistorialVotaciones_ProcesosElectorales_ProcesoElectoralId",
                        column: x => x.ProcesoElectoralId,
                        principalTable: "ProcesosElectorales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HistorialVotaciones_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Candidatos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    NombreEnPapeleta = table.Column<string>(type: "text", nullable: false),
                    FotoUrl = table.Column<string>(type: "text", nullable: true),
                    OrdenEnLista = table.Column<int>(type: "integer", nullable: true),
                    PartidoPoliticoId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Candidatos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Candidatos_PartidoPolitico_PartidoPoliticoId",
                        column: x => x.PartidoPoliticoId,
                        principalTable: "PartidoPolitico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Candidatos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetalleVoto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VotoId = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    PartidoPoliticoId = table.Column<int>(type: "integer", nullable: true),
                    CandidatoId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleVoto", x => x.Id);
                    table.CheckConstraint("CK_DetalleVoto_SiNoBlancoSinIds", "(\"Tipo\" NOT IN (2,3,4)) OR (\"CandidatoId\" IS NULL AND \"PartidoPoliticoId\" IS NULL)");
                    table.CheckConstraint("CK_DetalleVoto_SoloUno", "NOT (\"PartidoPoliticoId\" IS NOT NULL AND \"CandidatoId\" IS NOT NULL)");
                    table.CheckConstraint("CK_DetalleVoto_TipoCandidato", "(\"Tipo\" <> 0) OR (\"CandidatoId\" IS NOT NULL AND \"PartidoPoliticoId\" IS NULL)");
                    table.CheckConstraint("CK_DetalleVoto_TipoPartido", "(\"Tipo\" <> 1) OR (\"PartidoPoliticoId\" IS NOT NULL AND \"CandidatoId\" IS NULL)");
                    table.CheckConstraint("CK_DetalleVoto_TipoSinIds", "(\"Tipo\" IN (2,3,4)) OR (\"Tipo\" IN (0,1)) OR (1=1)");
                    table.ForeignKey(
                        name: "FK_DetalleVoto_Candidatos_CandidatoId",
                        column: x => x.CandidatoId,
                        principalTable: "Candidatos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DetalleVoto_PartidoPolitico_PartidoPoliticoId",
                        column: x => x.PartidoPoliticoId,
                        principalTable: "PartidoPolitico",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DetalleVoto_Votos_VotoId",
                        column: x => x.VotoId,
                        principalTable: "Votos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Candidatos_PartidoPoliticoId",
                table: "Candidatos",
                column: "PartidoPoliticoId");

            migrationBuilder.CreateIndex(
                name: "IX_Candidatos_UsuarioId",
                table: "Candidatos",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetalleVoto_CandidatoId",
                table: "DetalleVoto",
                column: "CandidatoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleVoto_PartidoPoliticoId",
                table: "DetalleVoto",
                column: "PartidoPoliticoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleVoto_VotoId",
                table: "DetalleVoto",
                column: "VotoId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialVotaciones_ProcesoElectoralId",
                table: "HistorialVotaciones",
                column: "ProcesoElectoralId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialVotaciones_UsuarioId_ProcesoElectoralId",
                table: "HistorialVotaciones",
                columns: new[] { "UsuarioId", "ProcesoElectoralId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartidoPolitico_ProcesoElectoralId",
                table: "PartidoPolitico",
                column: "ProcesoElectoralId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Cedula",
                table: "Usuarios",
                column: "Cedula",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Votos_ProcesoElectoralId",
                table: "Votos",
                column: "ProcesoElectoralId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetalleVoto");

            migrationBuilder.DropTable(
                name: "HistorialVotaciones");

            migrationBuilder.DropTable(
                name: "Candidatos");

            migrationBuilder.DropTable(
                name: "Votos");

            migrationBuilder.DropTable(
                name: "PartidoPolitico");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "ProcesosElectorales");
        }
    }
}
