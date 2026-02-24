using BibliotecaAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers.V1
{
    [ApiController]
    [Route("api/v1/")]
    [Authorize]
    public class RootController: ControllerBase
    {
        private readonly IAuthorizationService authorizationService;

        public RootController(IAuthorizationService authorizationService)
        {
            this.authorizationService = authorizationService;
        }

        [HttpGet(Name = "ObtenerRootV1")]
        [AllowAnonymous]
        public async Task<IEnumerable<DatosHATEOASDTO>> Get()
        {
            var datosHATEOAS = new List<DatosHATEOASDTO>();

            var esAdmin = await authorizationService.AuthorizeAsync(User, "esadmin");

            // no se pueden poner rutas con parametros en el enlace
            datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("ObtenerRootV1", new {})!, 
                Descripcion: "self", Metodo: "GET"));

            datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("ObtenerAutoresV1", new {})!, 
                Descripcion: "autores-obtener", Metodo: "GET"));

            //datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("PatchAutorV1", new {})!, 
            //    Descripcion: "autor-patch", Metodo: "PATCH"));

            datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("FiltrarAutoresV1", new {})!, 
                Descripcion: "autores-filtrar", Metodo: "GET"));

            datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("ObtenerLibrosV1", new {})!, 
                Descripcion: "libros-obtener", Metodo: "GET"));

            //datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("ObtenerComentariosV1", new {})!, 
            //    Descripcion: "comentarios-obtener", Metodo: "GET"));

            //Acciones que puede realizar usuario logueado
            if (User.Identity!.IsAuthenticated)
            {
                datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("ActualizarUsuarioV1", new {})!, 
                    Descripcion: "actualizar-usuario", Metodo: "PUT"));
                
                datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("RenovarTokenV1", new {})!, 
                    Descripcion: "renovar-token", Metodo: "GET"));
            }

            //Acciones que solo puede realizar un admin
            if (esAdmin.Succeeded)
            {
                datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("CrearAutorV1", new {})!, 
                    Descripcion: "autor-crear", Metodo: "POST"));

                datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("CrearAutorConFotoV1", new {})!, 
                    Descripcion: "autor-crear-con-foto", Metodo: "POST"));
            }

            return datosHATEOAS;
        }
    }
}
