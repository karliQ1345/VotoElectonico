using VotoElect.MVC.ApiContracts;

namespace VotoElect.MVC.ViewModels;

public class AdminCandidatosVm
{
    public string? Error { get; set; }
    public string? Ok { get; set; }

    public List<ProcesoResumenDto> Procesos { get; set; } = new();

    public string ProcesoElectoralId { get; set; } = "";
    public string Tipo { get; set; } = "Plurinominal";
    public string Titulo { get; set; } = "";
    public int? MaxSeleccionIndividual { get; set; }

    public string EleccionId { get; set; } = "";
    public string ListaNombre { get; set; } = "";
    public string ListaCodigo { get; set; } = "";
    public string? ListaLogoUrl { get; set; }

    public string CargaEleccionId { get; set; } = "";

    public string CandidatoNombre { get; set; } = "";
    public string? CandidatoCargo { get; set; }
    public string CandidatoFotoUrl { get; set; } = "";
    public string? CandidatoPartidoListaId { get; set; }
}
