using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PARCIAL_PROGRA.Data;
using PARCIAL_PROGRA.Models;

namespace PARCIAL_PROGRA.Controllers;

public class CursosController : Controller
{
    private readonly ApplicationDbContext _context;

    public CursosController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Cursos/Catalogo
    public async Task<IActionResult> Catalogo(string? nombre, int? creditosMin, int? creditosMax, string? horario)
    {
        var query = _context.Cursos.Where(c => c.Activo).AsQueryable();

        // Filtro por nombre
        if (!string.IsNullOrEmpty(nombre))
        {
            query = query.Where(c => c.Nombre.Contains(nombre));
        }

        // Filtro por rango de créditos
        if (creditosMin.HasValue)
        {
            query = query.Where(c => c.Creditos >= creditosMin.Value);
        }
        if (creditosMax.HasValue)
        {
            query = query.Where(c => c.Creditos <= creditosMax.Value);
        }

        // Filtro por horario
        if (!string.IsNullOrEmpty(horario))
        {
            query = horario switch
            {
                "manana" => query.Where(c => c.HorarioInicio < new TimeSpan(12, 0, 0)),
                "tarde" => query.Where(c => c.HorarioInicio >= new TimeSpan(12, 0, 0) && c.HorarioInicio < new TimeSpan(18, 0, 0)),
                "noche" => query.Where(c => c.HorarioInicio >= new TimeSpan(18, 0, 0)),
                _ => query
            };
        }

        var cursos = await query.ToListAsync();
        
        // Guardar en sesión el último curso visitado (para Pregunta 4)
        if (cursos.Any())
        {
            HttpContext.Session.SetString("UltimoCursoVisitado", cursos.First().Nombre);
        }

        ViewData["Nombre"] = nombre;
        ViewData["CreditosMin"] = creditosMin;
        ViewData["CreditosMax"] = creditosMax;
        ViewData["Horario"] = horario;

        return View(cursos);
    }

    // GET: /Cursos/Detalle/5
    public async Task<IActionResult> Detalle(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var curso = await _context.Cursos
            .Include(c => c.Matriculas)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (curso == null)
        {
            return NotFound();
        }

        // Guardar en sesión el último curso visitado
        HttpContext.Session.SetString("UltimoCursoId", curso.Id.ToString());
        HttpContext.Session.SetString("UltimoCursoNombre", curso.Nombre);

        return View(curso);
    }
}