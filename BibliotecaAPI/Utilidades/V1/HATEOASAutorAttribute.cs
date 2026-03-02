using BibliotecaAPI.DTOs;
using BibliotecaAPI.Servicios.V1;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BibliotecaAPI.Utilidades.V1
{
    public class HATEOASAutorAttribute: HATEOASFilterAttribute
    {
        private readonly IGeneradorEnlaces generadorEnlaces;
        private readonly ILogger<HATEOASAutorAttribute> logger;

        public HATEOASAutorAttribute(IGeneradorEnlaces generadorEnlaces, ILogger<HATEOASAutorAttribute> logger)
        {
            this.generadorEnlaces = generadorEnlaces;
            this.logger = logger;
        }

        public override async Task OnResultExecutionAsync (ResultExecutingContext context, ResultExecutionDelegate next)
        {
            //todo el código que este antes de next() se ejecutará antes de que el controlador que lo
            //utilice retorne un objeto o json, despues de retornar se ejecuta lo que este después del next
            
            logger.LogInformation("Inicio del filtro");
            
            var incluirHATEOAS = DebeIncluirHATEOAS(context);

            if (!incluirHATEOAS)
            {
                //continuar la ejecución 
                await next();
                logger.LogInformation("Fin del filtro");
                return;
            }

            var result = context.Result as ObjectResult;
            var modelo = result!.Value as AutorDTO ??
                throw new ArgumentNullException ("Se esperaba un AutorDTO");
            await generadorEnlaces.GenerarEnlaces(modelo);
            await next();
            logger.LogInformation("Fin del filtro");
        }
    }
}
