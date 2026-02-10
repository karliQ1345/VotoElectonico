namespace VotoElectonico.DTOs.Votacion;

public class BoletaDataDto
{
    public string ProcesoElectoralId { get; set; } = null!;
    public string EleccionId { get; set; } = null!;
    public string TipoEleccion { get; set; } = null!; // "Nominal" o "Plurinominal"
    public string Titulo { get; set; } = null!;

    public int? MaxSeleccionIndividual { get; set; }

    public List<BoletaListaDto> Listas { get; set; } = new();
    public List<BoletaCandidatoDto> Candidatos { get; set; } = new();
}

public class BoletaListaDto
{
    public string PartidoListaId { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string Codigo { get; set; } = null!;
    public string? LogoUrl { get; set; }
}

public class BoletaCandidatoDto
{
    public string CandidatoId { get; set; } = null!;
    public string NombreCompleto { get; set; } = null!;
    public string? Cargo { get; set; }
    public string FotoUrl { get; set; } = null!;
    public string? PartidoListaId { get; set; }
}

