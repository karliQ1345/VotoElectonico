namespace VotoElectonico.Models
{
    public enum RolUsuario
    {
        Administrador = 0, // vota + gestiona + ve todo
        Votante = 1,       // vota + reporte general
        Candidato = 2      // vota + reporte detallado + aparece en papeleta
    }

    public enum TipoEleccion
    {
        Presidencial = 0,
        Asambleistas = 1
    }

    public enum EstadoProcesoElectoral
    {
        Pendiente = 0,
        Activo = 1,
        Finalizado = 2,
        Suspendido = 3
    }

    public enum ModalidadPresidencial
    {
        PorCandidato = 0,     // elige 1 candidato
        SiNo = 1,             // SI o NO
        PorCandidatoConBlanco = 2, // elige 1 candidato o BLANCO
        SiNoConBlanco = 3          // SI, NO o BLANCO
    }

    public enum Genero
    {
        Masculino = 0,
        Femenino = 1,
        Otro = 2
    }

    /// <summary>
    /// Define qué representa una fila DetalleVoto.
    /// </summary>
    public enum TipoDetalleVoto
    {
        Candidato = 0,   // voto nominal por candidato/asambleísta
        Partido = 1,     // voto por plancha/partido
        Si = 2,
        No = 3,
        Blanco = 4,
        Nulo = 5
    }

}
