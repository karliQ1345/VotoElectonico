namespace VotoElectonico.Models
{
    public class VotoAnonimo
    {
        public Guid Id { get; set; }

        public Guid ProcesoElectoralId { get; set; }
        public Guid EleccionId { get; set; }
        public Guid JuntaId { get; set; }

        public DateTime EmitidoUtc { get; set; } = DateTime.UtcNow;

        // Criptografía (AES-GCM recomendado)
        public string CipherTextBase64 { get; set; } = null!;
        public string NonceBase64 { get; set; } = null!;
        public string TagBase64 { get; set; } = null!;
        public string KeyVersion { get; set; } = "v1";

        // Para auditoría técnica (sin identidad)
        public string? DeviceId { get; set; }
    }
}
