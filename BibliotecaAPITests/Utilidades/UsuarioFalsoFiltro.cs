using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BibliotecaAPITests.Utilidades
{
    public class UsuarioFalsoFiltro : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Antes de la acción
            context.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity (new List<Claim>
            {
                new Claim("email", "ale@prueba.com")
                // el segundo parametro es el authenticationType que define al usuario como que está registrado
            }, "prueba"));

            await next();

            // después de la acción
        }
    }
}
