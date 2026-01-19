using VotoElectonico.Models.Enums;

namespace VotoElectonico.Models
{
    public class ResultadoAgregado
    {
        public Guid Id { get; set; }

        public Guid ProcesoElectoralId { get; set; }
        public Guid EleccionId { get; set; }

        public DimensionReporte Dimension { get; set; }
        public string DimensionValor { get; set; } = null!; // "Pichincha", "Quito", "F", etc.

        // Para Presidente_SiNoBlanco: opcion = "SI"/"NO"/"BLANCO"
        public string Opcion { get; set; } = null!;

        // Para Asambleístas: puedes usar Opcion = CandidatoId o ListaId (según tipo de conteo)
        public Guid? CandidatoId { get; set; }
        public Guid? PartidoListaId { get; set; }

        public long Votos { get; set; }
        public DateTime ActualizadoUtc { get; set; } = DateTime.UtcNow;
    }
}
