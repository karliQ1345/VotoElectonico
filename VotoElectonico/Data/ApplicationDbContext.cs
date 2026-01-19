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
        //Auth
        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<UsuarioRol> UsuarioRoles => Set<UsuarioRol>();
        public DbSet<TwoFactorSesion> TwoFactorSesiones => Set<TwoFactorSesion>();
        //Padron/Juntas
        public DbSet<Junta> Juntas => Set<Junta>();
        public DbSet<PadronRegistro> PadronRegistros => Set<PadronRegistro>();
        public DbSet<CodigoVotacion> CodigosVotacion => Set<CodigoVotacion>();
        //Proceso Electoral
        public DbSet<ProcesoElectoral> ProcesosElectorales => Set<ProcesoElectoral>();
        public DbSet<Eleccion> Elecciones => Set<Eleccion>();
        public DbSet<PartidoLista> PartidosListas => Set<PartidoLista>();
        public DbSet<Candidato> Candidatos => Set<Candidato>();
        //Votacion
        public DbSet<VotoAnonimo> VotosAnonimos => Set<VotoAnonimo>();
        public DbSet<ComprobanteVoto> ComprobantesVoto => Set<ComprobanteVoto>();
        //Reportes
        public DbSet<EnvioResultadosJunta> EnviosResultadosJunta => Set<EnvioResultadosJunta>();
        public DbSet<ResultadoAgregado> ResultadosAgregados => Set<ResultadoAgregado>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.Cedula).HasMaxLength(10).IsRequired();
                e.HasIndex(x => x.Cedula).IsUnique();

                e.Property(x => x.NombreCompleto).HasMaxLength(200).IsRequired();
                e.Property(x => x.Email).HasMaxLength(320).IsRequired();
                e.HasIndex(x => x.Email);

                e.Property(x => x.Provincia).HasMaxLength(80);
                e.Property(x => x.Canton).HasMaxLength(80);
                e.Property(x => x.Parroquia).HasMaxLength(80);
                e.Property(x => x.Genero).HasMaxLength(20);

                e.Property(x => x.FotoUrl).HasMaxLength(500);
                e.Property(x => x.Telefono).HasMaxLength(30);
            });
            modelBuilder.Entity<UsuarioRol>(e =>
            {
                e.HasKey(x => new { x.UsuarioId, x.Rol });

                e.HasOne(x => x.Usuario)
                    .WithMany(x => x.Roles)
                    .HasForeignKey(x => x.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<TwoFactorSesion>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.CodigoHash).HasMaxLength(500).IsRequired();
                e.Property(x => x.BrevoMessageId).HasMaxLength(100);

                e.HasIndex(x => new { x.UsuarioId, x.CreadoUtc });

                e.HasOne(x => x.Usuario)
                    .WithMany()
                    .HasForeignKey(x => x.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<Junta>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.Codigo).HasMaxLength(30).IsRequired();
                e.HasIndex(x => x.Codigo).IsUnique();

                e.Property(x => x.Provincia).HasMaxLength(80).IsRequired();
                e.Property(x => x.Canton).HasMaxLength(80).IsRequired();
                e.Property(x => x.Parroquia).HasMaxLength(80).IsRequired();
                e.Property(x => x.Recinto).HasMaxLength(120);

                e.HasOne(x => x.JefeJuntaUsuario)
                    .WithMany()
                    .HasForeignKey(x => x.JefeJuntaUsuarioId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<ProcesoElectoral>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Nombre).HasMaxLength(200).IsRequired();

                e.HasIndex(x => x.Estado);
                e.HasIndex(x => new { x.InicioUtc, x.FinUtc });
            });
            modelBuilder.Entity<Eleccion>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.Titulo).HasMaxLength(200).IsRequired();
                e.HasIndex(x => new { x.ProcesoElectoralId, x.Tipo });

                e.HasOne(x => x.ProcesoElectoral)
                    .WithMany(x => x.Elecciones)
                    .HasForeignKey(x => x.ProcesoElectoralId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<PartidoLista>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
                e.Property(x => x.Codigo).HasMaxLength(50).IsRequired();
                e.Property(x => x.LogoUrl).HasMaxLength(500);

                e.HasIndex(x => new { x.EleccionId, x.Codigo }).IsUnique();

                e.HasOne(x => x.Eleccion)
                    .WithMany(x => x.Listas)
                    .HasForeignKey(x => x.EleccionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<Candidato>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.NombreCompleto).HasMaxLength(200).IsRequired();
                e.Property(x => x.Cargo).HasMaxLength(100);
                e.Property(x => x.FotoUrl).HasMaxLength(500).IsRequired();

                e.HasIndex(x => new { x.EleccionId, x.Activo });

                e.HasOne(x => x.Eleccion)
                    .WithMany(x => x.Candidatos)
                    .HasForeignKey(x => x.EleccionId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.PartidoLista)
                    .WithMany(x => x.Candidatos)
                    .HasForeignKey(x => x.PartidoListaId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
            modelBuilder.Entity<PadronRegistro>(e =>
            {
                e.HasKey(x => x.Id);

                e.HasIndex(x => new { x.ProcesoElectoralId, x.UsuarioId }).IsUnique();
                e.HasIndex(x => new { x.ProcesoElectoralId, x.JuntaId });

                e.HasOne(x => x.ProcesoElectoral)
                    .WithMany()
                    .HasForeignKey(x => x.ProcesoElectoralId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Usuario)
                    .WithMany()
                    .HasForeignKey(x => x.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Junta)
                    .WithMany()
                    .HasForeignKey(x => x.JuntaId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne<Usuario>()
                    .WithMany()
                    .HasForeignKey(x => x.VerificadoPorJefeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<CodigoVotacion>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.CodigoHash).HasMaxLength(500).IsRequired();

                e.HasIndex(x => new { x.ProcesoElectoralId, x.UsuarioId }).IsUnique();
                e.HasIndex(x => new { x.ProcesoElectoralId, x.Usado });

                e.HasOne(x => x.ProcesoElectoral)
                    .WithMany()
                    .HasForeignKey(x => x.ProcesoElectoralId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Usuario)
                    .WithMany()
                    .HasForeignKey(x => x.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<VotoAnonimo>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.CipherTextBase64).IsRequired();
                e.Property(x => x.NonceBase64).IsRequired();
                e.Property(x => x.TagBase64).IsRequired();
                e.Property(x => x.KeyVersion).HasMaxLength(20).IsRequired();
                e.Property(x => x.DeviceId).HasMaxLength(100);

                e.HasIndex(x => new { x.ProcesoElectoralId, x.EleccionId });
                e.HasIndex(x => new { x.JuntaId, x.EmitidoUtc });

                e.HasOne<ProcesoElectoral>()
                    .WithMany()
                    .HasForeignKey(x => x.ProcesoElectoralId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne<Eleccion>()
                    .WithMany()
                    .HasForeignKey(x => x.EleccionId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne<Junta>()
                    .WithMany()
                    .HasForeignKey(x => x.JuntaId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<ComprobanteVoto>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.PdfUrl).HasMaxLength(500);
                e.Property(x => x.BrevoMessageId).HasMaxLength(100);
                e.Property(x => x.ErrorEnvio).HasMaxLength(500);

                e.HasIndex(x => new { x.ProcesoElectoralId, x.UsuarioId });
                e.HasIndex(x => x.EstadoEnvio);

                e.HasOne<Usuario>()
                    .WithMany()
                    .HasForeignKey(x => x.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne<Junta>()
                    .WithMany()
                    .HasForeignKey(x => x.JuntaId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne<Usuario>()
                    .WithMany()
                    .HasForeignKey(x => x.JefeJuntaUsuarioId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne<ProcesoElectoral>()
                    .WithMany()
                    .HasForeignKey(x => x.ProcesoElectoralId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne<Eleccion>()
                    .WithMany()
                    .HasForeignKey(x => x.EleccionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<EnvioResultadosJunta>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.PayloadCifradoBase64).IsRequired();
                e.Property(x => x.KeyVersion).HasMaxLength(20).IsRequired();
                e.Property(x => x.HashSha256).HasMaxLength(100);
                e.Property(x => x.Observacion).HasMaxLength(500);

                e.HasIndex(x => new { x.ProcesoElectoralId, x.JuntaId, x.EnviadoUtc });

                e.HasOne<ProcesoElectoral>()
                    .WithMany()
                    .HasForeignKey(x => x.ProcesoElectoralId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne<Junta>()
                    .WithMany()
                    .HasForeignKey(x => x.JuntaId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne<Usuario>()
                    .WithMany()
                    .HasForeignKey(x => x.EnviadoPorJefeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<ResultadoAgregado>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.DimensionValor).HasMaxLength(100).IsRequired();
                e.Property(x => x.Opcion).HasMaxLength(80).IsRequired();

                e.HasIndex(x => new { x.ProcesoElectoralId, x.EleccionId, x.Dimension, x.DimensionValor });

                e.HasOne<ProcesoElectoral>()
                    .WithMany()
                    .HasForeignKey(x => x.ProcesoElectoralId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne<Eleccion>()
                    .WithMany()
                    .HasForeignKey(x => x.EleccionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
