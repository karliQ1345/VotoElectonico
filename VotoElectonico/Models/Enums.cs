namespace VotoElectonico.Models
{
    public enum RolUsuario
    {
        Administrador = 0, // Login: Cédula con 'A' + Clave
        Votante = 1        // Login: Cédula normal (sin clave obligatoria)
    }

    public enum TipoEleccion
    {
        Presidencial = 0, // Eliges 1 binomio
        Asambleistas = 1  // Eliges lista (Plancha) o candidatos (Entre listas)
    }

    public enum Genero
    {
        Masculino = 0,
        Femenino = 1,
        Otro = 2
    }
}
