using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BibliotecaAPI.Controllers.V1;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Servicios;
using BibliotecaAPI.Servicios.V1;
using BibliotecaAPITests.Utilidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Logging;

namespace BibliotecaAPITests.PruebasUnitarias.Controllers.V1
{
    [TestClass]
    public class AutoresControllerPruebas: BasePruebas
    {
        [TestMethod]
        public async Task Get_Retorna404_CuandoAutorConIdNoExiste()
        {
            //Preparación
            var nombreBD = Guid.NewGuid().ToString();
            var context = ConstruirContext(nombreBD);
            var mapper = ConfigurarAutoMapper();
            IAlmacenarArchivos almacenarArchivos = null!;
            ILogger<AutoresController> logger = null!;
            IOutputCacheStore outputCacheStore = null!;
            IServicioAutores servicioAutores = null!;

            var controller = new AutoresController(context, mapper, almacenarArchivos, logger, outputCacheStore, servicioAutores);

            //Prueba
            var respuesta = await controller.Get(1);

            //verificacion
            //como es un action result podemos castearlo a un statuscoderesult para obtener el codigo de estado
            var resultado = respuesta.Result as StatusCodeResult;
            Assert.AreEqual(expected: 404, actual: resultado!.StatusCode);
        }
        
        [TestMethod]
        public async Task Get_RetornaAutor_CuandoAutorConIdExiste()
        {
            //Preparación
            var nombreBD = Guid.NewGuid().ToString();
            var context = ConstruirContext(nombreBD);
            var mapper = ConfigurarAutoMapper();
            IAlmacenarArchivos almacenarArchivos = null!;
            ILogger<AutoresController> logger = null!;
            IOutputCacheStore outputCacheStore = null!;
            IServicioAutores servicioAutores = null!;

            context.Autores.Add(new Autor {Nombres = "Ale", Apellidos = "Castañeda"});
            context.Autores.Add(new Autor {Nombres = "Elías", Apellidos = "Ibarra"});

            await context.SaveChangesAsync();

            //no es recomendable usar el mismo contexto con el que creamos los autores

            var context2 = ConstruirContext(nombreBD);

            var controller = new AutoresController(context2, mapper, almacenarArchivos, logger, outputCacheStore, servicioAutores);

            //Prueba
            var respuesta = await controller.Get(1);

            //verificacion
            //como es un action result podemos castearlo a un statuscoderesult para obtener el codigo de estado
            var resultado = respuesta.Value;
            Assert.AreEqual(expected: 1, actual: resultado!.Id);
        }
    }
}
