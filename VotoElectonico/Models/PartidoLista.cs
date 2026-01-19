namespace VotoElectonico.Models
{
    public class PartidoLista
    {
        public Guid Id { get; set; }

        public Guid EleccionId { get; set; }
        public Eleccion Eleccion { get; set; } = null!;

        public string Nombre { get; set; } = null!;         // Partido/Lista
        public string Codigo { get; set; } = null!;         // "Lista 5"
        public string? LogoUrl { get; set; }                // opcional

        public List<Candidato> Candidatos { get; set; } = new();
    }
}
