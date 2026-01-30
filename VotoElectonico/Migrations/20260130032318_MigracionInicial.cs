using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VotoElectonico.Migrations
{
    /// <inheritdoc />
    public partial class MigracionInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcesosElectorales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    InicioUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FinUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    CreadoUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcesosElectorales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Cedula = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    NombreCompleto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    Telefono = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Provincia = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Canton = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Parroquia = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Genero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    FotoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    CreadoUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Elecciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcesoElectoralId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MaxSeleccionIndividual = table.Column<int>(type: "integer", nullable: true),
                    Activa = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Elecciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Elecciones_ProcesosElectorales_ProcesoElectoralId",
                        column: x => x.ProcesoElectoralId,
                        principalTable: "ProcesosElectorales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CodigosVotacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcesoElectoralId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    CodigoHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Usado = table.Column<bool>(type: "boolean", nullable: false),
                    UsadoUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreadoUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MostradoAlJefeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodigosVotacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CodigosVotacion_ProcesosElectorales_ProcesoElectoralId",
                        column: x => x.ProcesoElectoralId,
                        principalTable: "ProcesosElectorales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CodigosVotacion_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Juntas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Provincia = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Canton = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Parroquia = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Recinto = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    JefeJuntaUsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Activa = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Juntas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Juntas_Usuarios_JefeJuntaUsuarioId",
                        column: x => x.JefeJuntaUsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TwoFactorSesiones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Canal = table.Column<int>(type: "integer", nullable: false),
                    CodigoHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreadoUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiraUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Usado = table.Column<bool>(type: "boolean", nullable: false),
                    UsadoUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Intentos = table.Column<int>(type: "integer", nullable: false),
                    MaxIntentos = table.Column<int>(type: "integer", nullable: false),
                    BrevoMessageId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TwoFactorSesiones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TwoFactorSesiones_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioRoles",
                columns: table => new
                {
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rol = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioRoles", x => new { x.UsuarioId, x.Rol });
                    table.ForeignKey(
                        name: "FK_UsuarioRoles_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PartidosListas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EleccionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartidosListas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartidosListas_Elecciones_EleccionId",
                        column: x => x.EleccionId,
                        principalTable: "Elecciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResultadosAgregados",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcesoElectoralId = table.Column<Guid>(type: "uuid", nullable: false),
                    EleccionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Dimension = table.Column<int>(type: "integer", nullable: false),
                    DimensionValor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Opcion = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    CandidatoId = table.Column<Guid>(type: "uuid", nullable: true),
                    PartidoListaId = table.Column<Guid>(type: "uuid", nullable: true),
                    Votos = table.Column<long>(type: "bigint", nullable: false),
                    ActualizadoUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResultadosAgregados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResultadosAgregados_Elecciones_EleccionId",
                        column: x => x.EleccionId,
                        principalTable: "Elecciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResultadosAgregados_ProcesosElectorales_ProcesoElectoralId",
                        column: x => x.ProcesoElectoralId,
                        principalTable: "ProcesosElectorales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComprobantesVoto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcesoElectoralId = table.Column<Guid>(type: "uuid", nullable: false),
                    EleccionId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    JuntaId = table.Column<Guid>(type: "uuid", nullable: false),
                    JefeJuntaUsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    GeneradoUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PdfUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EstadoEnvio = table.Column<int>(type: "integer", nullable: false),
                    EnviadoUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BrevoMessageId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ErrorEnvio = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComprobantesVoto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComprobantesVoto_Elecciones_EleccionId",
                        column: x => x.EleccionId,
                        principalTable: "Elecciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComprobantesVoto_Juntas_JuntaId",
                        column: x => x.JuntaId,
                        principalTable: "Juntas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComprobantesVoto_ProcesosElectorales_ProcesoElectoralId",
                        column: x => x.ProcesoElectoralId,
                        principalTable: "ProcesosElectorales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComprobantesVoto_Usuarios_JefeJuntaUsuarioId",
                        column: x => x.JefeJuntaUsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComprobantesVoto_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EnviosResultadosJunta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcesoElectoralId = table.Column<Guid>(type: "uuid", nullable: false),
                    JuntaId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnviadoPorJefeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnviadoUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PayloadCifradoBase64 = table.Column<string>(type: "text", nullable: false),
                    KeyVersion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    HashSha256 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    Observacion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnviosResultadosJunta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnviosResultadosJunta_Juntas_JuntaId",
                        column: x => x.JuntaId,
                        principalTable: "Juntas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EnviosResultadosJunta_ProcesosElectorales_ProcesoElectoralId",
                        column: x => x.ProcesoElectoralId,
                        principalTable: "ProcesosElectorales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EnviosResultadosJunta_Usuarios_EnviadoPorJefeId",
                        column: x => x.EnviadoPorJefeId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PadronRegistros",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcesoElectoralId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    JuntaId = table.Column<Guid>(type: "uuid", nullable: false),
                    YaVoto = table.Column<bool>(type: "boolean", nullable: false),
                    VotoUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerificadoUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerificadoPorJefeId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PadronRegistros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PadronRegistros_Juntas_JuntaId",
                        column: x => x.JuntaId,
                        principalTable: "Juntas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PadronRegistros_ProcesosElectorales_ProcesoElectoralId",
                        column: x => x.ProcesoElectoralId,
                        principalTable: "ProcesosElectorales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PadronRegistros_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PadronRegistros_Usuarios_VerificadoPorJefeId",
                        column: x => x.VerificadoPorJefeId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VotosAnonimos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcesoElectoralId = table.Column<Guid>(type: "uuid", nullable: false),
                    EleccionId = table.Column<Guid>(type: "uuid", nullable: false),
                    JuntaId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmitidoUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CipherTextBase64 = table.Column<string>(type: "text", nullable: false),
                    NonceBase64 = table.Column<string>(type: "text", nullable: false),
                    TagBase64 = table.Column<string>(type: "text", nullable: false),
                    KeyVersion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DeviceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VotosAnonimos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VotosAnonimos_Elecciones_EleccionId",
                        column: x => x.EleccionId,
                        principalTable: "Elecciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VotosAnonimos_Juntas_JuntaId",
                        column: x => x.JuntaId,
                        principalTable: "Juntas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VotosAnonimos_ProcesosElectorales_ProcesoElectoralId",
                        column: x => x.ProcesoElectoralId,
                        principalTable: "ProcesosElectorales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Candidatos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EleccionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartidoListaId = table.Column<Guid>(type: "uuid", nullable: true),
                    NombreCompleto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Cargo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FotoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Candidatos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Candidatos_Elecciones_EleccionId",
                        column: x => x.EleccionId,
                        principalTable: "Elecciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Candidatos_PartidosListas_PartidoListaId",
                        column: x => x.PartidoListaId,
                        principalTable: "PartidosListas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Candidatos_EleccionId_Activo",
                table: "Candidatos",
                columns: new[] { "EleccionId", "Activo" });

            migrationBuilder.CreateIndex(
                name: "IX_Candidatos_PartidoListaId",
                table: "Candidatos",
                column: "PartidoListaId");

            migrationBuilder.CreateIndex(
                name: "IX_CodigosVotacion_ProcesoElectoralId_Usado",
                table: "CodigosVotacion",
                columns: new[] { "ProcesoElectoralId", "Usado" });

            migrationBuilder.CreateIndex(
                name: "IX_CodigosVotacion_ProcesoElectoralId_UsuarioId",
                table: "CodigosVotacion",
                columns: new[] { "ProcesoElectoralId", "UsuarioId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CodigosVotacion_UsuarioId",
                table: "CodigosVotacion",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ComprobantesVoto_EleccionId",
                table: "ComprobantesVoto",
                column: "EleccionId");

            migrationBuilder.CreateIndex(
                name: "IX_ComprobantesVoto_EstadoEnvio",
                table: "ComprobantesVoto",
                column: "EstadoEnvio");

            migrationBuilder.CreateIndex(
                name: "IX_ComprobantesVoto_JefeJuntaUsuarioId",
                table: "ComprobantesVoto",
                column: "JefeJuntaUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ComprobantesVoto_JuntaId",
                table: "ComprobantesVoto",
                column: "JuntaId");

            migrationBuilder.CreateIndex(
                name: "IX_ComprobantesVoto_ProcesoElectoralId_UsuarioId",
                table: "ComprobantesVoto",
                columns: new[] { "ProcesoElectoralId", "UsuarioId" });

            migrationBuilder.CreateIndex(
                name: "IX_ComprobantesVoto_UsuarioId",
                table: "ComprobantesVoto",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Elecciones_ProcesoElectoralId_Tipo",
                table: "Elecciones",
                columns: new[] { "ProcesoElectoralId", "Tipo" });

            migrationBuilder.CreateIndex(
                name: "IX_EnviosResultadosJunta_EnviadoPorJefeId",
                table: "EnviosResultadosJunta",
                column: "EnviadoPorJefeId");

            migrationBuilder.CreateIndex(
                name: "IX_EnviosResultadosJunta_JuntaId",
                table: "EnviosResultadosJunta",
                column: "JuntaId");

            migrationBuilder.CreateIndex(
                name: "IX_EnviosResultadosJunta_ProcesoElectoralId_JuntaId_EnviadoUtc",
                table: "EnviosResultadosJunta",
                columns: new[] { "ProcesoElectoralId", "JuntaId", "EnviadoUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Juntas_Codigo",
                table: "Juntas",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Juntas_JefeJuntaUsuarioId",
                table: "Juntas",
                column: "JefeJuntaUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_PadronRegistros_JuntaId",
                table: "PadronRegistros",
                column: "JuntaId");

            migrationBuilder.CreateIndex(
                name: "IX_PadronRegistros_ProcesoElectoralId_JuntaId",
                table: "PadronRegistros",
                columns: new[] { "ProcesoElectoralId", "JuntaId" });

            migrationBuilder.CreateIndex(
                name: "IX_PadronRegistros_ProcesoElectoralId_UsuarioId",
                table: "PadronRegistros",
                columns: new[] { "ProcesoElectoralId", "UsuarioId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PadronRegistros_UsuarioId",
                table: "PadronRegistros",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_PadronRegistros_VerificadoPorJefeId",
                table: "PadronRegistros",
                column: "VerificadoPorJefeId");

            migrationBuilder.CreateIndex(
                name: "IX_PartidosListas_EleccionId_Codigo",
                table: "PartidosListas",
                columns: new[] { "EleccionId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcesosElectorales_Estado",
                table: "ProcesosElectorales",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_ProcesosElectorales_InicioUtc_FinUtc",
                table: "ProcesosElectorales",
                columns: new[] { "InicioUtc", "FinUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ResultadosAgregados_EleccionId",
                table: "ResultadosAgregados",
                column: "EleccionId");

            migrationBuilder.CreateIndex(
                name: "IX_ResultadosAgregados_ProcesoElectoralId_EleccionId_Dimension~",
                table: "ResultadosAgregados",
                columns: new[] { "ProcesoElectoralId", "EleccionId", "Dimension", "DimensionValor" });

            migrationBuilder.CreateIndex(
                name: "IX_TwoFactorSesiones_UsuarioId_CreadoUtc",
                table: "TwoFactorSesiones",
                columns: new[] { "UsuarioId", "CreadoUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Cedula",
                table: "Usuarios",
                column: "Cedula",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_VotosAnonimos_EleccionId",
                table: "VotosAnonimos",
                column: "EleccionId");

            migrationBuilder.CreateIndex(
                name: "IX_VotosAnonimos_JuntaId_EmitidoUtc",
                table: "VotosAnonimos",
                columns: new[] { "JuntaId", "EmitidoUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_VotosAnonimos_ProcesoElectoralId_EleccionId",
                table: "VotosAnonimos",
                columns: new[] { "ProcesoElectoralId", "EleccionId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Candidatos");

            migrationBuilder.DropTable(
                name: "CodigosVotacion");

            migrationBuilder.DropTable(
                name: "ComprobantesVoto");

            migrationBuilder.DropTable(
                name: "EnviosResultadosJunta");

            migrationBuilder.DropTable(
                name: "PadronRegistros");

            migrationBuilder.DropTable(
                name: "ResultadosAgregados");

            migrationBuilder.DropTable(
                name: "TwoFactorSesiones");

            migrationBuilder.DropTable(
                name: "UsuarioRoles");

            migrationBuilder.DropTable(
                name: "VotosAnonimos");

            migrationBuilder.DropTable(
                name: "PartidosListas");

            migrationBuilder.DropTable(
                name: "Juntas");

            migrationBuilder.DropTable(
                name: "Elecciones");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "ProcesosElectorales");
        }
    }
}
