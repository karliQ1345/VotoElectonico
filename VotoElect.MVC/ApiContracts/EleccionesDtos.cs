namespace VotoElect.MVC.ApiContracts;

public class CrearEleccionRequestDto
{
    public string ProcesoElectoralId { get; set; } = "";
    public string Tipo { get; set; } = "";
    public string Titulo { get; set; } = "";
    public int? MaxSeleccionIndividual { get; set; }
}

public class CrearListaRequestDto
{
    public string EleccionId { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Codigo { get; set; } = "";
    public string? LogoUrl { get; set; }
}

public class CrearCandidatoRequestDto
{
    public string EleccionId { get; set; } = "";
    public string NombreCompleto { get; set; } = "";
    public string? Cargo { get; set; }
    public string FotoUrl { get; set; } = "";
    public string? PartidoListaId { get; set; }
}
