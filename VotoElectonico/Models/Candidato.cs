using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VotoElectonico.Models
{
    public class Candidato
    {
        [Key]
        public int Id { get; set; }

        // RELACIÓN 1 a 1 con USUARIO
        // El candidato "ES" un usuario, por ende ya tiene cédula, nombre real, y puede votar.
        [ForeignKey("Usuario")]
        public int UsuarioId { get; set; }
        public virtual Usuario Usuario { get; set; }

        // --- DATOS DE CAMPAÑA (Lo que sale en la papeleta) ---
        // A veces el nombre en papeleta es distinto al legal (Apodos, etc)
        public string NombreEnPapeleta { get; set; }
        public string FotoUrl { get; set; }

        public int? OrdenEnLista { get; set; } // Para asambleístas

        // RELACIÓN CON PARTIDO
        public int PartidoPoliticoId { get; set; }
        [ForeignKey("PartidoPoliticoId")]
        public virtual PartidoPolitico Partido { get; set; }
    }
}
