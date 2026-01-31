namespace VotoElectonico.DTOs.Procesos
{
    public class ProcesoActivoDto
    {
        public string ProcesoElectoralId { get; set; } = "";
        public string Nombre { get; set; } = "";
        public DateTime InicioUtc { get; set; }
        public DateTime FinUtc { get; set; }
        public string Estado { get; set; } = "";
    }
}
