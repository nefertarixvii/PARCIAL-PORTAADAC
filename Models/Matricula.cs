using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace PARCIAL_PROGRA.Models;

public enum EstadoMatricula
{
    Pendiente,
    Confirmada,
    Cancelada
}

public class Matricula
{
    public int Id { get; set; }
    
    public int CursoId { get; set; }
    
    public string UsuarioId { get; set; } = string.Empty;
    
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
    
    public EstadoMatricula Estado { get; set; } = EstadoMatricula.Pendiente;
    
    // Navegación
    public virtual Curso? Curso { get; set; }
    public virtual IdentityUser? Usuario { get; set; }
}