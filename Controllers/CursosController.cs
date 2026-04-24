using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using PARCIAL_PROGRA.Data;
using PARCIAL_PROGRA.Models;
using System.Text.Json;

namespace PARCIAL_PROGRA.Controllers;

public class CursosController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IDistributedCache _cache;

    public CursosController(ApplicationDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    // GET: /Cursos/Catalogo
    public async Task<IActionResult> Catalogo(string? nombre, int? creditosMin, int? creditosMax, string? horario)
    {
        // Cache Redis para listado de cursos activos (60 segundos)
        var cacheKey = "cursos_activos";
        var cachedData = await _cache.GetStringAsync(cacheKey);
        
        List<Curso> cursos;
        if (cachedData != null)
        {
            cursos = JsonSerializer.Deserialize<List<Curso>>(cachedData) ?? new List<Curso>();
        }
        else
        {
            cursos = await _context.Cursos.Where(c => c.Activo).ToListAsync();
            var jsonData = JsonSerializer.Serialize(cursos);
            await _cache.SetStringAsync(cacheKey, jsonData, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
            });
        }

        // Aplicar filtros en memoria
        var query = cursos.AsQueryable();

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

        var cursosFiltrados = query.ToList();
        
        // Guardar en sesión el último curso visitado (para Pregunta 4)
        if (cursosFiltrados.Any())
        {
            HttpContext.Session.SetString("UltimoCursoVisitado", cursosFiltrados.First().Nombre);
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