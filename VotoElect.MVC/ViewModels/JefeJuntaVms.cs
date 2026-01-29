using VotoElect.MVC.ApiContracts;

namespace VotoElect.MVC.ViewModels;

public class JefePanelVm
{
    public string ProcesoElectoralId { get; set; } = "";
    public JefePanelDto? Panel { get; set; }

    public JefeVerificarVotanteResponseDto? Verificacion { get; set; }

    public string? Error { get; set; }
    public string? Ok { get; set; }
}


