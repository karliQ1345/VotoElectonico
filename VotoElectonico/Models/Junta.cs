namespace VotoElectonico.Models
{
    public class Junta
    {
        public Guid Id { get; set; }
        public string Codigo { get; set; } = null!;         // ej: "J-0123"
        public string Provincia { get; set; } = null!;
        public string Canton { get; set; } = null!;
        public string Parroquia { get; set; } = null!;
        public string? Recinto { get; set; }

        // Jefe asignado a la junta
        public Guid JefeJuntaUsuarioId { get; set; }
        public Usuario JefeJuntaUsuario { get; set; } = null!;

        public bool Activa { get; set; } = true;
        public bool Cerrada { get; set; } = false;
        public DateTime? CerradaUtc { get; set; }
        public Guid? CerradaPorJefeId { get; set; }
    }
}
