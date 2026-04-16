using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BibliotecaAPI.Utilidades
{
    public static class LimitarPeticionesMiddlewareExtensions
    {
        public static IApplicationBuilder UseLimitarPeticiones(this IApplicationBuilder app)
        {
            return app.UseMiddleware<LimitarPeticionesMiddleware>();
        }
    }
    public class LimitarPeticionesMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IOptionsMonitor<LimitarPeticionesDTO> optionsLimitarPeticiones;

        public LimitarPeticionesMiddleware(RequestDelegate next, IOptionsMonitor<LimitarPeticionesDTO> optionsLimitarPeticiones)
        {
            this.next = next;
            this.optionsLimitarPeticiones = optionsLimitarPeticiones;
        }

        public async Task InvokeAsync(HttpContext httpContext, ApplicationDbContext context)
        {
            var endpoint = httpContext.GetEndpoint();

            if (endpoint is null)
            {
                await next(httpContext);
                return;
            }

            var actionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
            
            if (actionDescriptor is not null)
            {
                // inherit true sirve para que tome en cuenta si el atribute está en una clase base
                var accionTieneAtributoIgnorarLimitarPeticiones = actionDescriptor.MethodInfo
                    .GetCustomAttributes(typeof(DeshabilitarLimitarPeticionesAttribute), inherit: true)
                    .Any();

                var controladorTieneAtributoIgnorarLimitarPeticiones = actionDescriptor.ControllerTypeInfo
                    .GetCustomAttributes(typeof(DeshabilitarLimitarPeticionesAttribute), inherit: true)
                    .Any();

                if (accionTieneAtributoIgnorarLimitarPeticiones || controladorTieneAtributoIgnorarLimitarPeticiones)
                {
                    await next(httpContext);
                    return;
                }
            }
            
            var limitarPeticionesDTO = optionsLimitarPeticiones.CurrentValue;

            var llaveStringValues = httpContext.Request.Headers["X-Api-Key"];

            if (llaveStringValues.Count == 0)
            {
                httpContext.Response.StatusCode = 400;
                await httpContext.Response.WriteAsync("Debe proveer la llave en la cabecera X-Api-Key");
                return;
            }

            if (llaveStringValues.Count > 1)
            {
                httpContext.Response.StatusCode = 400;
                await httpContext.Response.WriteAsync("Solo una llave debe de estar presente");
                return;
            }

            var llave = llaveStringValues[0];

            var llaveDB = await context.LlaveAPIs
                .Include(x => x.RestriccionesDominio)
                .Include(x => x.RestriccionesIP)
                .FirstOrDefaultAsync(x => x.Llave == llave);

            if (llaveDB is null)
            {
                httpContext.Response.StatusCode = 400;
                await httpContext.Response.WriteAsync("La llave no existe");
                return;
            }

            if (!llaveDB.Activa) 
            {
                httpContext.Response.StatusCode = 400;
                await httpContext.Response.WriteAsync("La llave se encuentra inactiva");
                return;
            }

            var restriccionesSuperadas = PeticionSuperaAlgunaDeLasRestricciones(llaveDB, httpContext);

            if (!restriccionesSuperadas)
            {
                httpContext.Response.StatusCode = 403;
                return;
            }

            if (llaveDB.TipoLlave == TipoLlave.Gratuita)
            {
                var hoy = DateTime.UtcNow.Date;
                var cantidadPeticionesRealizadasHoy = await context.Peticiones.CountAsync(
                    x => x.LlaveId == llaveDB.Id && x.FechaPeticion >= hoy
                );

                if (limitarPeticionesDTO.PeticionesPorDiaGratuito <= cantidadPeticionesRealizadasHoy)
                {
                    httpContext.Response.StatusCode = 429; // too many requests (demasiadas peticiones)
                    await httpContext.Response.WriteAsync("Ha excedido el limite de peticiones por día.");
                    return;
                }
            }

            var peticion = new Peticion() {LlaveId = llaveDB.Id, FechaPeticion = DateTime.UtcNow};
            context.Add(peticion);
            await context.SaveChangesAsync();

            await next(httpContext);
        }

        private bool PeticionSuperaAlgunaDeLasRestricciones(LlaveAPI llaveApi, HttpContext httpContext)
        {
            var hayRestricciones = llaveApi.RestriccionesDominio.Any() || llaveApi.RestriccionesIP.Any();
            
            if (!hayRestricciones)
            {
                return true;
            }

            var peticionSuperaLasRestriccionesDeDominio = 
                PeticionSuperaLasRestriccionesDeDominio(llaveApi.RestriccionesDominio, httpContext);

            var peticionSuperaLasRestriccionesDeIP = 
                PeticionSuperaLasRestriccionesDeIP(llaveApi.RestriccionesIP, httpContext);

            return peticionSuperaLasRestriccionesDeDominio || peticionSuperaLasRestriccionesDeIP;
        }

        private bool PeticionSuperaLasRestriccionesDeDominio(List<RestriccionDominio> restricciones, 
            HttpContext httpContext)
        {
            if (restricciones is null || restricciones.Count == 0)
            {
                return false;
            }

            // referer se refiere a desde donde viene la peticion (url)
            var referer = httpContext.Request.Headers["referer"].ToString();

            if (referer == string.Empty)
            {
                return false;
            }

            var miURI = new Uri(referer);
            var dominio = miURI.Host;

            var superaRestriccion = restricciones.Any(x => x.Domininio == dominio);

            return superaRestriccion;
        }

        private bool PeticionSuperaLasRestriccionesDeIP (List<RestriccionIP> restricciones, 
            HttpContext httpContext)
        {
            if (restricciones is null || restricciones.Count == 0)
            {
                return false;
            }

            var remoteIpAddress = httpContext.Connection.RemoteIpAddress;

            if (remoteIpAddress is null)
            {
                return false;
            }

            var IP = remoteIpAddress.ToString();

            if (IP == string.Empty)
            {
                return false;
            }

            var superaRestriccion = restricciones.Any(x => x.IP == IP);
            return superaRestriccion;
        }
    }
}
