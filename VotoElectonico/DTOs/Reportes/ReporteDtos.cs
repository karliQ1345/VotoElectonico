namespace VotoElectonico.DTOs.Reportes
{
    public class ReporteGeneralDto
    {
        public int ProcesoElectoralId { get; set; }
        public int TotalVotos { get; set; }
        public int TotalVotantesQueYaVotaron { get; set; }

        public List<ItemConteoDto> PorTipo { get; set; } = new();
        public List<ItemConteoDto> PorPartido { get; set; } = new();
        public List<ItemConteoDto> PorCandidato { get; set; } = new(); // en general puede ir vacío
    }

    public class ItemConteoDto
    {
        public string Clave { get; set; } = default!;
        public int Conteo { get; set; }
    }
}
