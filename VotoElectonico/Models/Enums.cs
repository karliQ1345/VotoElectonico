namespace VotoElectonico.Models
{
    public enum RolUsuario
    {
        // Vota + Gestiona Procesos + Ve TODOS los reportes
        Administrador = 0,

        // Solo Vota + Ve reportes genéricos
        Votante = 1,

        // Vota + Ve reportes DETALLADOS + Aparece en papeleta
        Candidato = 2
    }

    public enum TipoEleccion
    {
        Presidencial = 0,
        Asambleistas = 1
    }

    public enum Genero
    {
        Masculino = 0,
        Femenino = 1,
        Otro = 2
    }
}
