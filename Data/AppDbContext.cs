using Microsoft.EntityFrameworkCore;
using PlataformaEscolar.API.Models;

namespace PlataformaEscolar.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Curso> Cursos { get; set; }
        public DbSet<Inscripcion> Inscripciones { get; set; }
        public DbSet<Tarea> Tareas { get; set; }
        public DbSet<Entrega> Entregas { get; set; }
        public DbSet<Calificacion> Calificaciones { get; set; }
        public DbSet<Comentario> Comentarios { get; set; }
        public DbSet<Anuncio> Anuncios { get; set; }
        public DbSet<Notificacion> Notificaciones { get; set; } // ✅ agregado
        public DbSet<Archivo> Archivos { get; set; }             // opcional según BD
        public DbSet<TareaArchivo> TareaArchivos { get; set; }   // opcional
        public DbSet<EntregaArchivo> EntregaArchivos { get; set; } // opcional

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Relación única: un alumno solo una entrega por tarea
            modelBuilder.Entity<Entrega>()
                .HasIndex(e => new { e.TareaId, e.AlumnoId })
                .IsUnique();

            // Relación 1 a 1: Entrega - Calificación
            modelBuilder.Entity<Calificacion>()
                .HasOne(c => c.Entrega)
                .WithOne(e => e.Calificacion)
                .HasForeignKey<Calificacion>(c => c.EntregaId);

            base.OnModelCreating(modelBuilder);
        }
    }
}