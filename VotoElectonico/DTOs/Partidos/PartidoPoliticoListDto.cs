namespace VotoElectonico.DTOs.Partidos
{
    public class PartidoPoliticoListDto
    {
        public int Id { get; set; }
        public int ProcesoElectoralId { get; set; }

        public string NombreLista { get; set; } = default!;
        public int NumeroLista { get; set; }
        public string? LogoUrl { get; set; }

        public int TotalCandidatos { get; set; }
    }
}
