using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PARCIAL_PROGRA.Models;

namespace PARCIAL_PROGRA.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Curso> Cursos { get; set; }
    public DbSet<Matricula> Matriculas { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Restricción: Código único en Curso
        builder.Entity<Curso>()
            .HasIndex(c => c.Codigo)
            .IsUnique();

        // Restricción: Un usuario no puede estar matriculado más de una vez en el mismo curso
        builder.Entity<Matricula>()
            .HasIndex(m => new { m.CursoId, m.UsuarioId })
            .IsUnique();

        // Restricción: Créditos > 0 (configurada en la anotación, pero reforzamos aquí)
        builder.Entity<Curso>()
            .Property(c => c.Creditos)
            .IsRequired();

        // Restricción: HorarioInicio < HorarioFin (se valida en aplicación)
    }
}
