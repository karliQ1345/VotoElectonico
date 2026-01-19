namespace VotoElectonico.Models
{
    public class Candidato
    {
        public Guid Id { get; set; }

        public Guid EleccionId { get; set; }
        public Eleccion Eleccion { get; set; } = null!;

        public Guid? PartidoListaId { get; set; }
        public PartidoLista? PartidoLista { get; set; }

        public string NombreCompleto { get; set; } = null!;
        public string? Cargo { get; set; }                  // "Asambleísta", "Presidente", etc.
        public string FotoUrl { get; set; } = null!;

        public bool Activo { get; set; } = true;
    }
}
