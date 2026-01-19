namespace VotoElectonico.Models
{
    public class PadronRegistro
    {
        public Guid Id { get; set; }

        public Guid ProcesoElectoralId { get; set; }
        public ProcesoElectoral ProcesoElectoral { get; set; } = null!;

        public Guid UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public Guid JuntaId { get; set; }
        public Junta Junta { get; set; } = null!;

        // Control: votó / no votó
        public bool YaVoto { get; set; } = false;
        public DateTime? VotoUtc { get; set; }

        // Para “verificación presencial”
        public DateTime? VerificadoUtc { get; set; }
        public Guid? VerificadoPorJefeId { get; set; }      // UsuarioId (Jefe)
    }
}
