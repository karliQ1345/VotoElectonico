using VotoElectonico.Models;

namespace VotoElectonico.DTOs.Procesos
{
    public class ProcesoElectoralDetailDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = default!;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        public EstadoProcesoElectoral Estado { get; set; }
        public TipoEleccion Tipo { get; set; }

        public ModalidadPresidencial? ModalidadPresidencial { get; set; }

        public bool PermitePlancha { get; set; }
        public bool PermiteNominal { get; set; }
        public int? MaxSeleccionNominal { get; set; }

        public int PartidosRegistrados { get; set; }
        public int VotosRegistrados { get; set; }
        public int TotalVotantesQueYaVotaron { get; set; }
    }

}
