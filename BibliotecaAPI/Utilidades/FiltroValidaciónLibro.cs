using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Utilidades
{
    public class FiltroValidaciónLibro : IAsyncActionFilter
    {
        private readonly ApplicationDbContext dbContext;

        public FiltroValidaciónLibro(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // validar que el tipo que trae la aplicación sea LibroCreacionDTO
            if (!context.ActionArguments.TryGetValue("libroCreacionDTO", out var value) || value is not LibroCreacionDTO libroCreacionDTO)
            {
                context.ModelState.AddModelError(string.Empty, "El modelo enviado no es válido");
                context.Result = context.ModelState.ContruirProblemDetail();
                return;
            }
            
            if (libroCreacionDTO.AutoresIds == null || libroCreacionDTO.AutoresIds.Count == 0)
            {
                context.ModelState.AddModelError(nameof(libroCreacionDTO.AutoresIds), "No se puede crear un libro sin autores.");
                context.Result = context.ModelState.ContruirProblemDetail();
                return;
            }

            var autoresIds = await dbContext.Autores
                .Where(x => libroCreacionDTO.AutoresIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync();

            if (libroCreacionDTO.AutoresIds.Count != autoresIds.Count)
            {
                var autoresNoExisten = libroCreacionDTO.AutoresIds.Except(autoresIds);
                var autoresNoExistenString = string.Join(",", autoresNoExisten);
                var mensajeDeError = $"No existen los siguientes autores (ids): {autoresNoExistenString}";
                context.ModelState.AddModelError(nameof(libroCreacionDTO.AutoresIds), mensajeDeError);
                context.Result = context.ModelState.ContruirProblemDetail();
                return;
            }
            
            await next();
        }
    }
}
