namespace VotoElectonico.Models.Enums
{
    public enum RolTipo
    {
        Administrador = 1,
        Votante = 2,
        JefeJunta = 3
    }

    public enum TwoFactorCanal
    {
        Email = 1
    }

    public enum ProcesoEstado
    {
        Pendiente = 1,
        Activo = 2,
        Finalizado = 3
    }

    public enum EleccionTipo
    {
        Nominal = 1,
        Plurinominal = 2
    }

    public enum ComprobanteEstado
    {
        Pendiente = 1,
        Enviado = 2,
        Fallido = 3
    }

    public enum EnvioResultadoEstado
    {
        Recibido = 1,
        Validado = 2,
        Rechazado = 3
    }

    public enum DimensionReporte
    {
        Nacional = 0,
        Provincia = 1,
        Canton = 2,
        Parroquia = 3,
        Genero = 4
    }
}
