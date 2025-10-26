using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BibliotecaAPI.Utilidades
{
    // filtro global
    public class FiltroTiempoEjecucion : IAsyncActionFilter
    {
        private readonly ILogger<FiltroTiempoEjecucion> logger;

        public FiltroTiempoEjecucion(ILogger<FiltroTiempoEjecucion> logger)
        {
            this.logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Antes de la ejecución de la acción
            var stopWatch = Stopwatch.StartNew();
            // context.ActionDescriptor.DisplayName retorna el nombre de la acción
            logger.LogInformation($"Inicio Acción: {context.ActionDescriptor.DisplayName}");

            await next();

            // Después de la ejecución de la acción
            stopWatch.Stop();
            logger.LogInformation($"Fin Acción: {context.ActionDescriptor.DisplayName} - Tiempo {stopWatch.ElapsedMilliseconds} ms");
        }
    }
}
