using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PARCIAL_PROGRA.Data;
using PARCIAL_PROGRA.Models;

namespace PARCIAL_PROGRA.Controllers;

[Authorize(Roles = "Coordinador")]
public class CoordinadorController : Controller
{
    private readonly ApplicationDbContext _context;

    public CoordinadorController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Coordinador
    public async Task<IActionResult> Index()
    {
        var cursos = await _context.Cursos.ToListAsync();
        return View(cursos);
    }

    // GET: /Coordinador/Nuevo
    public IActionResult Nuevo()
    {
        return View();
    }

    // POST: /Coordinador/Nuevo
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Nuevo([Bind("Codigo,Nombre,Creditos,CupoMaximo,HorarioInicio,HorarioFin,Activo")] Curso curso)
    {
        // Validación server-side: Créditos > 0
        if (curso.Creditos <= 0)
        {
            ModelState.AddModelError("Creditos", "Los créditos deben ser mayores a 0");
        }

        // Validación server-side: HorarioInicio < HorarioFin
        if (curso.HorarioInicio >= curso.HorarioFin)
        {
            ModelState.AddModelError("HorarioFin", "El horario de fin debe ser posterior al horario de inicio");
        }

        // Validación: Código único
        if (await _context.Cursos.AnyAsync(c => c.Codigo == curso.Codigo))
        {
            ModelState.AddModelError("Codigo", "Ya existe un curso con este código");
        }

        if (ModelState.IsValid)
        {
            _context.Cursos.Add(curso);
            await _context.SaveChangesAsync();
            
            // Invalidar cache de cursos
            InvalidateCache();
            
            TempData["Success"] = "Curso creado correctamente";
            return RedirectToAction(nameof(Index));
        }

        return View(curso);
    }

    // GET: /Coordinador/Editar/5
    public async Task<IActionResult> Editar(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var curso = await _context.Cursos.FindAsync(id);
        if (curso == null)
        {
            return NotFound();
        }

        return View(curso);
    }

    // POST: /Coordinador/Editar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, [Bind("Id,Codigo,Nombre,Creditos,CupoMaximo,HorarioInicio,HorarioFin,Activo")] Curso curso)
    {
        if (id != curso.Id)
        {
            return NotFound();
        }

        // Validación server-side: Créditos > 0
        if (curso.Creditos <= 0)
        {
            ModelState.AddModelError("Creditos", "Los créditos deben ser mayores a 0");
        }

        // Validación server-side: HorarioInicio < HorarioFin
        if (curso.HorarioInicio >= curso.HorarioFin)
        {
            ModelState.AddModelError("HorarioFin", "El horario de fin debe ser posterior al horario de inicio");
        }

        // Validación: Código único (excluyendo el curso actual)
        if (await _context.Cursos.AnyAsync(c => c.Codigo == curso.Codigo && c.Id != id))
        {
            ModelState.AddModelError("Codigo", "Ya existe un curso con este código");
        }

        if (ModelState.IsValid)
        {
            _context.Update(curso);
            await _context.SaveChangesAsync();
            
            // Invalidar cache de cursos
            InvalidateCache();
            
            TempData["Success"] = "Curso actualizado correctamente";
            return RedirectToAction(nameof(Index));
        }

        return View(curso);
    }

    // GET: /Coordinador/Desactivar/5
    public async Task<IActionResult> Desactivar(int id)
    {
        var curso = await _context.Cursos.FindAsync(id);
        if (curso == null)
        {
            return NotFound();
        }

        curso.Activo = false;
        await _context.SaveChangesAsync();
        
        // Invalidar cache de cursos
        InvalidateCache();
        
        TempData["Success"] = "Curso desactivado correctamente";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Coordinador/Matriculas/5
    public async Task<IActionResult> Matriculas(int? cursoId)
    {
        if (cursoId == null)
        {
            return NotFound();
        }

        var curso = await _context.Cursos.FindAsync(cursoId);
        if (curso == null)
        {
            return NotFound();
        }

        var matriculas = await _context.Matriculas
            .Include(m => m.Usuario)
            .Where(m => m.CursoId == cursoId)
            .ToListAsync();

        ViewData["Curso"] = curso;
        return View(matriculas);
    }

    // GET: /Coordinador/Confirmar/5
    public async Task<IActionResult> Confirmar(int id)
    {
        var matricula = await _context.Matriculas.FindAsync(id);
        if (matricula == null)
        {
            return NotFound();
        }

        // Verificar que no se exceda el cupo
        var curso = await _context.Cursos.FindAsync(matricula.CursoId);
        var matriculasConfirmadas = await _context.Matriculas
            .CountAsync(m => m.CursoId == matricula.CursoId && m.Estado == EstadoMatricula.Confirmada && m.Id != id);

        if (curso != null && matriculasConfirmadas >= curso.CupoMaximo)
        {
            TempData["Error"] = "No se puede confirmar. El curso ha alcanzado su cupo máximo.";
            return RedirectToAction("Matriculas", new { cursoId = matricula.CursoId });
        }

        matricula.Estado = EstadoMatricula.Confirmada;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Matrícula confirmada";
        return RedirectToAction("Matriculas", new { cursoId = matricula.CursoId });
    }

    // GET: /Coordinador/Cancelar/5
    public async Task<IActionResult> Cancelar(int id)
    {
        var matricula = await _context.Matriculas.FindAsync(id);
        if (matricula == null)
        {
            return NotFound();
        }

        var cursoId = matricula.CursoId;
        matricula.Estado = EstadoMatricula.Cancelada;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Matrícula cancelada";
        return RedirectToAction("Matriculas", new { cursoId });
    }

    private void InvalidateCache()
    {
        // Invalidar cache de cursos activos
        // En una implementación real, usarías el servicio de cache
    }
}