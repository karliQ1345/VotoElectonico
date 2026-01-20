namespace VotoElectonico.DTOs.Votacion;

public class EmitirVotoResponseDto
{
    public bool Ok { get; set; }
    public string Mensaje { get; set; } = "";

    // requisito: "Su papeleta ha sido enviada a su correo"
    public bool PapeletaEnviada { get; set; }
    public string? EmailEnmascarado { get; set; }
}

