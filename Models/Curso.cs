using System.ComponentModel.DataAnnotations;

namespace PARCIAL_PROGRA.Models;

public class Curso
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(20)]
    public string Codigo { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;
    
    [Range(1, int.MaxValue, ErrorMessage = "Los créditos deben ser mayores a 0")]
    public int Creditos { get; set; }
    
    public int CupoMaximo { get; set; }
    
    public TimeSpan HorarioInicio { get; set; }
    
    public TimeSpan HorarioFin { get; set; }
    
    public bool Activo { get; set; } = true;
    
    // Navegación
    public virtual ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
}