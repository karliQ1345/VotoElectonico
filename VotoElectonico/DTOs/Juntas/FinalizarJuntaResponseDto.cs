namespace VotoElectonico.DTOs.Juntas
{
    public class FinalizarJuntaResponseDto
    {
        public bool Ok { get; set; }
        public string Mensaje { get; set; } = "";
        public string JuntaCodigo { get; set; } = "";
        public DateTime? CerradaUtc { get; set; }
    }
}
