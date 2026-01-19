using VotoElectonico.Models.Enums;

namespace VotoElectonico.Models
{
    public class ProcesoElectoral
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = null!;
        public DateTime InicioUtc { get; set; }
        public DateTime FinUtc { get; set; }
        public ProcesoEstado Estado { get; set; } = ProcesoEstado.Pendiente;

        public DateTime CreadoUtc { get; set; } = DateTime.UtcNow;

        public List<Eleccion> Elecciones { get; set; } = new();
    }
}
