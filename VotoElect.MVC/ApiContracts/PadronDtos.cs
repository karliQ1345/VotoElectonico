using VotoElectonico.Models.Enums; 

namespace VotoElect.MVC.ApiContracts

{
    public class PadronExcelRowDto
    {
        public RolTipo Rol { get; set; }
        public string Cedula { get; set; } = "";
        public string NombreCompleto { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Provincia { get; set; }
        public string? Canton { get; set; }
        public string? Parroquia { get; set; }
        public string? Genero { get; set; }
        public string? FotoUrl { get; set; }
        public string JuntaCodigo { get; set; } = "";
    }

    public class CargaPadronResponseDto
    {
        public int Total { get; set; }
        public int Insertados { get; set; }
        public int Actualizados { get; set; }
        public int ConError { get; set; }
        public List<string> Errores { get; set; } = new();
    }
}
