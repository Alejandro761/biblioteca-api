using Microsoft.AspNetCore.Mvc.Filters;

namespace BibliotecaAPI.Utilidades
{
    // ActionFilterAttribute es un filtro de acción para aceptar argumentos
    public class FiltroAgregarCabecerasAttribute: ActionFilterAttribute
    {
        private readonly string nombre;
        private readonly string valor;

        public FiltroAgregarCabecerasAttribute(string nombre, string valor)
        {
            this.nombre = nombre;
            this.valor = valor;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            //antes de la ejecución de la acción
            context.HttpContext.Response.Headers.Append(nombre, valor);
            base.OnActionExecuting(context);
            
            //después de la ejecución de la acción
        }
    }
}
