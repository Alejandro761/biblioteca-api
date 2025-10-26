using Microsoft.AspNetCore.Mvc.Filters;

namespace BibliotecaAPI.Utilidades
{
    public class MiFIltroDeAccion : IActionFilter
    {
        private readonly ILogger<MiFIltroDeAccion> logger;

        public MiFIltroDeAccion(ILogger<MiFIltroDeAccion> logger)
        {
            this.logger = logger;
        }
        
        //antes de la cción
        public void OnActionExecuting(ActionExecutingContext context)
        {
            logger.LogInformation("Ejecutando la acción");
        }

        //después de la cción
        public void OnActionExecuted(ActionExecutedContext context)
        {
            logger.LogInformation("Acción ejecutada");
        }
    }
}
