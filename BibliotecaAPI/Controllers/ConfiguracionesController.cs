using Microsoft.AspNetCore.Mvc;

namespace BibliotecaAPI.Controllers
{
    [ApiController]
    [Route("api/configuraciones")]
    public class ConfiguracionesController: ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly IConfigurationSection seccion_01;
        private readonly IConfigurationSection seccion_02;

        public ConfiguracionesController(IConfiguration configuration)
        {
            this.configuration = configuration;
            seccion_01 = configuration.GetSection("seccion_1");
            seccion_02 = configuration.GetSection("seccion_2");
        }

        [HttpGet("obtenertodos")]
        public ActionResult GetObtenerTodos()
        {
            var hijos = configuration.GetChildren().Select(x => $"{x.Key}: {x.Value}");
            return Ok(new { hijos });
        }

        [HttpGet("seccion_01")]
        public ActionResult GetSeccion01()
        {
            var nombre = seccion_01.GetValue<string>("nombre");
            var edad = seccion_01.GetValue<int>("edad");

            return Ok(new { nombre, edad });
        }

        [HttpGet("seccion_02")]
        public ActionResult GetSeccion02()
        {
            var nombre = seccion_02.GetValue<string>("nombre");
            var edad = seccion_02.GetValue<int>("edad");

            return Ok(new { nombre, edad });
        }


        [HttpGet]
        public ActionResult<string> Get()
        {
            var opcion1 = configuration["apellido"];
            var opcion2 = configuration.GetValue<string>("apellido")!;
            return opcion2;
        }

        [HttpGet("secciones")]
        public ActionResult<string> GetSeccion()
        {
            var opcion1 = configuration["ConnectionStrings:DefaultConnection"];
            var opcion2 = configuration.GetValue<string>("ConnectionStrings:DefaultConnection")!;
            var seccion = configuration.GetSection("ConnectionStrings");

            var opcion3 = seccion["DefaultConnection"];

            return opcion3!;
        }
    }
}
