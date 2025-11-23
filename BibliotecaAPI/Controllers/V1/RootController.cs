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

            return datosHATEOAS;
        }
    }
}
