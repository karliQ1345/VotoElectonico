namespace VotoElectonico.Models
{
    public class CodigoVotacion
    {
        public Guid Id { get; set; }

        public Guid ProcesoElectoralId { get; set; }
        public ProcesoElectoral ProcesoElectoral { get; set; } = null!;

        public Guid UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public string CodigoHash { get; set; } = null!;
        public bool Usado { get; set; } = false;
        public DateTime? UsadoUtc { get; set; }

        public DateTime CreadoUtc { get; set; } = DateTime.UtcNow;
        public DateTime? MostradoAlJefeUtc { get; set; }    // cuando el jefe lo “entrega”
    }
}
