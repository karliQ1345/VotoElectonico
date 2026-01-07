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

            // 1) Cedula única
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Cedula)
                .IsUnique();

            // 2) Un candidato por usuario (1 a 1 real)
            modelBuilder.Entity<Candidato>()
                .HasIndex(c => c.UsuarioId)
                .IsUnique();

            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.InfoCandidatura)
                .WithOne(c => c.Usuario)
                .HasForeignKey<Candidato>(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3) Un voto por usuario por proceso (control "solo una vez")
            modelBuilder.Entity<HistorialVotacion>()
                .HasIndex(h => new { h.UsuarioId, h.ProcesoElectoralId })
                .IsUnique();

            // 4) Relación PartidoPolitico -> ProcesoElectoral
            modelBuilder.Entity<PartidoPolitico>()
                .HasOne(p => p.Proceso)
                .WithMany(pe => pe.PartidosInscritos)
                .HasForeignKey(p => p.ProcesoElectoralId)
                .OnDelete(DeleteBehavior.Cascade);

            // 5) Relación Voto -> ProcesoElectoral
            modelBuilder.Entity<Voto>()
                .HasOne(v => v.Proceso)
                .WithMany(pe => pe.UrnaDeVotos)
                .HasForeignKey(v => v.ProcesoElectoralId)
                .OnDelete(DeleteBehavior.Cascade);

            // 6) DetalleVoto: relación a Voto
            modelBuilder.Entity<DetalleVoto>()
                .HasOne(d => d.Voto)
                .WithMany(v => v.Detalles)
                .HasForeignKey(d => d.VotoId)
                .OnDelete(DeleteBehavior.Cascade);

            // (Opcional) Índices para reportes rápidos
            modelBuilder.Entity<DetalleVoto>().HasIndex(d => d.CandidatoId);
            modelBuilder.Entity<DetalleVoto>().HasIndex(d => d.PartidoPoliticoId);
        }
    }
}
