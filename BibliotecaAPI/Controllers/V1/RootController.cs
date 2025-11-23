using BibliotecaAPI.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers.V1
{
    [ApiController]
    [Route("api/v1/")]
    public class RootController: ControllerBase
    {
        [HttpGet(Name = "ObtenerRootV1")]
        public IEnumerable<DatosHATEOASDTO> Get()
        {
            var datosHATEOAS = new List<DatosHATEOASDTO>();

            datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("ObtenerRootV1", new {})!, 
                Descripcion: "self", Metodo: "GET"));

            datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("ObtenerAutoresV1", new {})!, 
                Descripcion: "autores-obtener", Metodo: "GET"));

            datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("PatchAutorV1", new {})!, 
                Descripcion: "autor-patch", Metodo: "PATCH"));

            datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("FiltrarAutoresV1", new {})!, 
                Descripcion: "autores-filtrar", Metodo: "GET"));

            datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("CrearAutorConFotoV1", new {})!, 
                Descripcion: "autor-crear-con-foto", Metodo: "POST"));

            datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("ObtenerLibrosV1", new {})!, 
                Descripcion: "libros-obtener", Metodo: "GET"));

            datosHATEOAS.Add(new DatosHATEOASDTO(Enlace: Url.Link("ObtenerComentariosV1", new {})!, 
                Descripcion: "comentarios-obtener", Metodo: "GET"));

            return datosHATEOAS;
        }
    }
}
