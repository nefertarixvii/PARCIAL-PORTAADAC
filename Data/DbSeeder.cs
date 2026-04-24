using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PARCIAL_PROGRA.Models;

namespace PARCIAL_PROGRA.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        // Crear usuario coordinador si no existe
        var coordinatorEmail = "coordinador@universidad.com";
        var coordinatorUser = await userManager.FindByEmailAsync(coordinatorEmail);
        
        if (coordinatorUser == null)
        {
            coordinatorUser = new IdentityUser
            {
                UserName = coordinatorEmail,
                Email = coordinatorEmail,
                EmailConfirmed = true
            };
            
            var result = await userManager.CreateAsync(coordinatorUser, "Coordinador123!");
            if (result.Succeeded)
            {
                // Agregar rol usando AddToRoleAsync (el rol se crea automáticamente si no existe)
                await userManager.AddToRoleAsync(coordinatorUser, "Coordinador");
            }
        }

        // Seed de Cursos si no existen
        if (!await context.Cursos.AnyAsync())
        {
            var cursos = new List<Curso>
            {
                new Curso
                {
                    Codigo = "CS101",
                    Nombre = "Introducción a la Programación",
                    Creditos = 4,
                    CupoMaximo = 30,
                    HorarioInicio = new TimeSpan(8, 0, 0),
                    HorarioFin = new TimeSpan(10, 0, 0),
                    Activo = true
                },
                new Curso
                {
                    Codigo = "CS201",
                    Nombre = "Estructuras de Datos",
                    Creditos = 4,
                    CupoMaximo = 25,
                    HorarioInicio = new TimeSpan(10, 0, 0),
                    HorarioFin = new TimeSpan(12, 0, 0),
                    Activo = true
                },
                new Curso
                {
                    Codigo = "CS301",
                    Nombre = "Bases de Datos",
                    Creditos = 3,
                    CupoMaximo = 20,
                    HorarioInicio = new TimeSpan(14, 0, 0),
                    HorarioFin = new TimeSpan(16, 0, 0),
                    Activo = true
                }
            };

            context.Cursos.AddRange(cursos);
            await context.SaveChangesAsync();
        }
    }
}