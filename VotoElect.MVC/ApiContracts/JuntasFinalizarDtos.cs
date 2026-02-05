namespace VotoElect.MVC.ApiContracts
{
    public class FinalizarJuntaRequestDto
    {
        public string ProcesoElectoralId { get; set; } = "";
    }

    public class FinalizarJuntaResponseDto
    {
        public bool Ok { get; set; }
        public string Mensaje { get; set; } = "";
        public string JuntaCodigo { get; set; } = "";
        public DateTime? CerradaUtc { get; set; }
    }
}
