namespace VotoElect.MVC.ApiContracts;

public class IniciarVotacionRequestDto
{
    public string ProcesoElectoralId { get; set; } = null!;
    public string Cedula { get; set; } = null!;
    public string CodigoUnico { get; set; } = null!;
}

public class IniciarVotacionResponseDto
{
    public bool Habilitado { get; set; }
    public string Mensaje { get; set; } = "";
    public string JuntaCodigo { get; set; } = "";
    public string? Recinto { get; set; }
}

public class BoletaDataDto
{
    public string ProcesoElectoralId { get; set; } = null!;
    public string EleccionId { get; set; } = null!;
    public string TipoEleccion { get; set; } = null!;
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

public class EmitirVotoRequestDto
{
    public string ProcesoElectoralId { get; set; } = null!;
    public string EleccionId { get; set; } = null!;
    public string Cedula { get; set; } = null!;
    public string CodigoUnico { get; set; } = null!;

    public string? OpcionPresidente { get; set; }
    public string? PartidoListaId { get; set; }
    public List<string>? CandidatoIds { get; set; }
}

public class EmitirVotoResponseDto
{
    public bool Ok { get; set; }
    public string Mensaje { get; set; } = "";
    public bool PapeletaEnviada { get; set; }
    public string? EmailEnmascarado { get; set; }
}

