using VotoElect.MVC.ApiContracts;

namespace VotoElect.MVC.ViewModels;

public class VotantesCodigoVm
{
    public string CodigoUnico { get; set; } = "";
    public string? Error { get; set; }
}

public class VotantesPapeletaVm
{
    public BoletaDataDto Boleta { get; set; } = new();
    public string? Error { get; set; }
}

public class VotantesConfirmarVm
{
    public string ProcesoElectoralId { get; set; } = "";
    public string EleccionId { get; set; } = "";
    public string TipoEleccion { get; set; } = "";

    // Presidente
    public string? OpcionPresidente { get; set; }

    // Asambleistas
    public string? PartidoListaId { get; set; }
    public List<string> CandidatoIds { get; set; } = new();

    public string Resumen { get; set; } = "";
    public string? Error { get; set; }
}

public class VotantesComprobanteVm
{
    public bool PapeletaEnviada { get; set; }
    public string? EmailEnmascarado { get; set; }
    public string? Mensaje { get; set; }
}
