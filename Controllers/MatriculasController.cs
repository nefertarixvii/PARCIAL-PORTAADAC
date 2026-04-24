using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PARCIAL_PROGRA.Data;
using PARCIAL_PROGRA.Models;

namespace PARCIAL_PROGRA.Controllers;

public class MatriculasController : Controller
{
    private readonly ApplicationDbContext _context;

    public MatriculasController(ApplicationDbContext context)
    {
        _context = context;
    }

    // POST: /Matriculas/Inscribirse/5
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Inscribirse(int cursoId)
    {
        var usuarioId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(usuarioId))
        {
            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }

        // Obtener el curso
        var curso = await _context.Cursos.FindAsync(cursoId);
        if (curso == null)
        {
            TempData["Error"] = "El curso no existe.";
            return RedirectToAction("Catalogo", "Cursos");
        }

        // Validación: Créditos > 0
        if (curso.Creditos <= 0)
        {
            TempData["Error"] = "Los créditos del curso deben ser mayores a 0.";
            return RedirectToAction("Detalle", "Cursos", new { id = cursoId });
        }

        // Validación: HorarioInicio < HorarioFin
        if (curso.HorarioInicio >= curso.HorarioFin)
        {
            TempData["Error"] = "El horario de inicio debe ser anterior al horario de fin.";
            return RedirectToAction("Detalle", "Cursos", new { id = cursoId });
        }

        // Validación: No exceder CupoMaximo
        var matriculasConfirmadas = await _context.Matriculas
            .CountAsync(m => m.CursoId == cursoId && m.Estado == EstadoMatricula.Confirmada);
        
        if (matriculasConfirmadas >= curso.CupoMaximo)
        {
            TempData["Error"] = "El curso ha alcanzado su cupo máximo.";
            return RedirectToAction("Detalle", "Cursos", new { id = cursoId });
        }

        // Validación: Usuario no matriculado previamente en el mismo curso
        var matriculaExistente = await _context.Matriculas
            .FirstOrDefaultAsync(m => m.CursoId == cursoId && m.UsuarioId == usuarioId);
        
        if (matriculaExistente != null)
        {
            TempData["Error"] = "Ya estás matriculado en este curso.";
            return RedirectToAction("Detalle", "Cursos", new { id = cursoId });
        }

        // Validación: No solaparse con otro curso matriculado en el mismo horario
        var matriculasUsuario = await _context.Matriculas
            .Include(m => m.Curso)
            .Where(m => m.UsuarioId == usuarioId && m.Estado == EstadoMatricula.Confirmada)
            .ToListAsync();

        foreach (var mat in matriculasUsuario)
        {
            if (mat.Curso != null)
            {
                bool haySolapamiento = curso.HorarioInicio < mat.Curso.HorarioFin && 
                                       curso.HorarioFin > mat.Curso.HorarioInicio;
                
                if (haySolapamiento)
                {
                    TempData["Error"] = $"Ya tienes matriculado el curso '{mat.Curso.Nombre}' en un horario que se solapa.";
                    return RedirectToAction("Detalle", "Cursos", new { id = cursoId });
                }
            }
        }

        // Crear matrícula en estado Pendiente
        var nuevaMatricula = new Matricula
        {
            CursoId = cursoId,
            UsuarioId = usuarioId,
            FechaRegistro = DateTime.Now,
            Estado = EstadoMatricula.Pendiente
        };

        _context.Matriculas.Add(nuevaMatricula);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Te has inscrito correctamente. Tu matrícula está pendiente de confirmación.";
        return RedirectToAction("Detalle", "Cursos", new { id = cursoId });
    }

    // GET: /Matriculas/MisMatriculas
    [Authorize]
    public async Task<IActionResult> MisMatriculas()
    {
        var usuarioId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(usuarioId))
        {
            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }

        var matriculas = await _context.Matriculas
            .Include(m => m.Curso)
            .Where(m => m.UsuarioId == usuarioId)
            .ToListAsync();

        return View(matriculas);
    }
}