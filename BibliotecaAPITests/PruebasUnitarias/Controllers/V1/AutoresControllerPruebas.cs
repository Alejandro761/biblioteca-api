using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BibliotecaAPI.Controllers.V1;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Servicios;
using BibliotecaAPI.Servicios.V1;
using BibliotecaAPITests.Utilidades;
using BibliotecaAPITests.Utilidades.Dobles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

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
        
        [TestMethod]
        public async Task Get_RetornaAutorConLibros_CuandoAutorTieneLibros()
        {
            //Preparación
            var nombreBD = Guid.NewGuid().ToString();
            var context = ConstruirContext(nombreBD);
            var mapper = ConfigurarAutoMapper();
            IAlmacenarArchivos almacenarArchivos = null!;
            ILogger<AutoresController> logger = null!;
            IOutputCacheStore outputCacheStore = null!;
            IServicioAutores servicioAutores = null!;

            var libro1 = new Libro {Titulo = "Libro1"};
            var libro2 = new Libro {Titulo = "Libro2"};
            
            var autor = new Autor {
                Nombres = "Ale",
                Apellidos = "Cast",
                Libros = new List<AutorLibro>
                {
                    new AutorLibro {Libro = libro1},
                    new AutorLibro {Libro = libro2}
                }
            };


            context.Add(autor);

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
            Assert.AreEqual(expected: 2, actual: resultado!.Libros.Count);
        }

        [TestMethod]
        public async Task Get_DebeLlamarGetDelServicioAutores()
        {
            // Prepación
            var nombreBD = Guid.NewGuid().ToString();
            var context = ConstruirContext(nombreBD);
            var mapper = ConfigurarAutoMapper();
            IAlmacenarArchivos almacenarArchivos = null!;
            ILogger<AutoresController> logger = null!;
            //creamos un doble para suplantar el outputCacheStore
            IOutputCacheStore outputCacheStore = null!;
            // La libreria NSubstitute permite probar servicios, haciendo un doble de la interface que incluye funcionalidades como que si fue llamado una o contadas veces, si se le mando paginación DTO, etc
            IServicioAutores servicioAutores = Substitute.For<IServicioAutores>();

            var controller = new AutoresController(context, mapper, almacenarArchivos, logger, outputCacheStore, servicioAutores);

            var paginacionDTO = new PaginacionDTO(2, 3);
            
            //Prueba
            await controller.Get(paginacionDTO);

            // Verificación
            // se espera que el metodo Get de ServiciosAutores sea llamado 1 vez mandando paginacionDTO
            await servicioAutores.Received(1).Get(paginacionDTO);

        }

        [TestMethod]
        public async Task Post_DebeCrearAutor_CuandoEnviamosAutor()
        {
            // Preparación
            var nombreBD = Guid.NewGuid().ToString();
            var context = ConstruirContext(nombreBD);
            var mapper = ConfigurarAutoMapper();
            IAlmacenarArchivos almacenarArchivos = null!;
            ILogger<AutoresController> logger = null!;
            //creamos un doble para suplantar el outputCacheStore
            IOutputCacheStore outputCacheStore = new OutputCacheStoreFalso();
            IServicioAutores servicioAutores = null!;

            var nuevoAutor = new AutorCreacionDTO {Nombres = "nuevo", Apellidos = "autor"};

            var controller = new AutoresController(context, mapper, almacenarArchivos, logger, outputCacheStore, servicioAutores);
            
            // Prueba
            var respuesta = await controller.Post(nuevoAutor);

            // Verificación

            //casteamos el action result a un CreateAtRouteResult porque es lo que retorna (un enalce) el Post
            var resultado = respuesta as CreatedAtRouteResult;
            //verificar que no sea nulo
            Assert.IsNotNull(resultado);

            //verificar en un contexto diferente si la bd tiene el autor que acabamos de crear
            var context2 = ConstruirContext(nombreBD);
            var cantidad = await context2.Autores.CountAsync();
            Assert.AreEqual(expected: 1, actual: cantidad);
        }
    }
}
