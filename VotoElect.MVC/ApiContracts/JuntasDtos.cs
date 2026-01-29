namespace VotoElect.MVC.ApiContracts;

// GET api/juntas/panel/{procesoId}
public class JefePanelDto
{
    public string JuntaId { get; set; } = "";
    public string JuntaCodigo { get; set; } = "";
    public string Provincia { get; set; } = "";
    public string Canton { get; set; } = "";
    public string Parroquia { get; set; } = "";
    public bool ProcesoActivo { get; set; }
    public bool BotonIrAVotarDisponible { get; set; }
}

// POST api/juntas/verificar
public class JefeVerificarVotanteRequestDto
{
    public string ProcesoElectoralId { get; set; } = "";
    public string CedulaVotante { get; set; } = "";
}

public class JefeVerificarVotanteResponseDto
{
    public bool Permitido { get; set; }
    public string Mensaje { get; set; } = "";
    public VotanteVerificacionDto? Votante { get; set; }
    public string? CodigoUnico { get; set; }
}

public class VotanteVerificacionDto
{
    public string UsuarioId { get; set; } = "";
    public string Cedula { get; set; } = "";
    public string NombreCompleto { get; set; } = "";

    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? FotoUrl { get; set; }

    public string Provincia { get; set; } = "";
    public string Canton { get; set; } = "";
    public string Parroquia { get; set; } = "";
    public string Genero { get; set; } = "";

    public bool YaVoto { get; set; }
}

