using VotoElect.MVC.ApiContracts;

namespace VotoElect.MVC.ViewModels;

public class JefeJuntaPanelVm
{
    public string ProcesoElectoralId { get; set; } = "";
    public JefePanelDto? Panel { get; set; }

    public string CedulaVotante { get; set; } = "";
    public JefeVerificarVotanteResponseDto? Resultado { get; set; }

    public string? Error { get; set; }
}
