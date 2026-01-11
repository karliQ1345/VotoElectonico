namespace VotoElectonico.DTOs.Candidatos
{
    public class CandidatoListDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }

        public string NombreEnPapeleta { get; set; } = default!;
        public string? FotoUrl { get; set; }
        public int? OrdenEnLista { get; set; }

        public int PartidoPoliticoId { get; set; }
        public string PartidoNombre { get; set; } = default!;
        public int ProcesoElectoralId { get; set; }
    }
}
