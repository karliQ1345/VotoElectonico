using VotoElectonico.Models.Enums;

namespace VotoElectonico.Models
{
    public class Eleccion
    {
        public Guid Id { get; set; }

        public Guid ProcesoElectoralId { get; set; }
        public ProcesoElectoral ProcesoElectoral { get; set; } = null!;

        public EleccionTipo Tipo { get; set; }
        public string Titulo { get; set; } = null!;         // "Elección Presidencial", etc.

        // Para Asambleístas: límite máximo de selección individual
        public int? MaxSeleccionIndividual { get; set; }    // ej: 8

        public bool Activa { get; set; } = true;

        public List<Candidato> Candidatos { get; set; } = new();
        public List<PartidoLista> Listas { get; set; } = new();
    }
}
