using VotoElect.MVC.ApiContracts;
namespace VotoElect.MVC.ViewModels
{
    public class AdminPadronVm
    {
        public string? Ok { get; set; }
        public string? Error { get; set; }

        public string ProcesoElectoralId { get; set; } = "";
        public List<ProcesoResumenDto> Procesos { get; set; } = new();

        public string? ResultJson { get; set; } // para mostrar el resumen
    }
}
