using Microsoft.EntityFrameworkCore;
using VotoElectonico.Models;

namespace VotoElectonico.Data
{
    public class ApplicationDbContext : DbContext
    {
        // El constructor es obligatorio para pasar la conexión de Supabase
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // --- DEFINICIÓN DE TABLAS ---
        // Aquí le dices: "Quiero que estas clases sean tablas en la base de datos"

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<ProcesoElectoral> ProcesosElectorales { get; set; }
        public DbSet<Candidato> Candidatos { get; set; }

        // Las nuevas tablas para el voto anónimo
        public DbSet<HistorialVotacion> HistorialVotaciones { get; set; }
        public DbSet<Voto> Votos { get; set; }

        // --- REGLAS ESPECIALES DE LA BASE DE DATOS ---
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // REGLA: Un Usuario solo puede estar UNA VEZ en el historial de un Proceso.
            // Esto crea un "candado" en la base de datos: 
            // Si el Usuario 5 intenta entrar al Proceso 1 por segunda vez, la base de datos lo bloquea.
            modelBuilder.Entity<HistorialVotacion>()
                .HasIndex(h => new { h.UsuarioId, h.ProcesoElectoralId })
                .IsUnique();
        }
    }
}
