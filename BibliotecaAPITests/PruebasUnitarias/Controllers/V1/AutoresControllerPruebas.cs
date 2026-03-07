using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BibliotecaAPI.Controllers.V1;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Servicios;
using BibliotecaAPI.Servicios.V1;
using BibliotecaAPITests.Utilidades;
using BibliotecaAPITests.Utilidades.Dobles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace BibliotecaAPITests.PruebasUnitarias.Controllers.V1
{
    [TestClass]
    public class AutoresControllerPruebas: BasePruebas
    {
        IAlmacenarArchivos almacenarArchivos = null!;
        ILogger<AutoresController> logger = null!;
        IOutputCacheStore outputCacheStore = null!;
        IServicioAutores servicioAutores = null!;
        private string nombreBD = Guid.NewGuid().ToString();
        private AutoresController controller = null!;
        private const string cache = "autores-obtener";
        private const string contenedor = "autores";

        [TestInitialize]
        public void Setup()
        {
            //Preparación

            var context = ConstruirContext(nombreBD);
            var mapper = ConfigurarAutoMapper();
            // La libreria NSubstitute permite probar servicios, haciendo un doble de la interface que incluye funcionalidades como que si fue llamado una o contadas veces, si se le mando paginación DTO, etc
            almacenarArchivos = Substitute.For<IAlmacenarArchivos>();
            logger = Substitute.For<ILogger<AutoresController>>();
            outputCacheStore = Substitute.For<IOutputCacheStore>();
            servicioAutores = Substitute.For<IServicioAutores>();

            controller = new AutoresController(context, mapper, almacenarArchivos, logger, outputCacheStore, servicioAutores);

        }

        [TestMethod]
        public async Task Get_Retorna404_CuandoAutorConIdNoExiste()
        {
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
            var context = ConstruirContext(nombreBD);

            context.Autores.Add(new Autor {Nombres = "Ale", Apellidos = "Castañeda"});
            context.Autores.Add(new Autor {Nombres = "Elías", Apellidos = "Ibarra"});

            await context.SaveChangesAsync();

            //no es recomendable usar el mismo contexto con el que creamos los autores

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
            var context = ConstruirContext(nombreBD);

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
            //creamos un doble para suplantar el outputCacheStore
            // IOutputCacheStore outputCacheStore = new OutputCacheStoreFalso();

            var nuevoAutor = new AutorCreacionDTO {Nombres = "nuevo", Apellidos = "autor"};

            
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

        [TestMethod]
        public async Task Put_Retornar404_CuandoAutorNoExiste()
        {
            // Prueba
            var respuesta = await controller.Put(1, autorCreacionDTO: null!);
            
            // Verificación
            var resultado = respuesta as StatusCodeResult;

            Assert.AreEqual(expected: 404, actual: resultado!.StatusCode);
        }
        
        [TestMethod]
        public async Task Put_ActualizaAutor_CuandoEnviamosAutorSinFoto()
        {
            // Preparacion
            var context = ConstruirContext(nombreBD);
            context.Autores.Add(new Autor {
                Nombres = "Ale", 
                Apellidos = "Cast", 
                Identificacion = "ale53"
            });

            await context.SaveChangesAsync();

            var autorCreacionDTO = new AutorCreacionDTOConFoto
            {
                Nombres = "Ale2", 
                Apellidos = "Cast2", 
                Identificacion = "ale53453"
            };
            
            // Prueba
            var respuesta = await controller.Put(1, autorCreacionDTO);
            
            // Verificación
            var resultado = respuesta as StatusCodeResult;

            Assert.AreEqual(expected: 204, actual: resultado!.StatusCode);

            var context2 = ConstruirContext(nombreBD);
            var autorActualizado = await context2.Autores.SingleAsync();

            Assert.AreEqual(expected: "Ale2", actual: autorActualizado.Nombres);
            Assert.AreEqual(expected: "Cast2", actual: autorActualizado.Apellidos);
            Assert.AreEqual(expected: "ale53453", actual: autorActualizado.Identificacion);
            await outputCacheStore.Received(1).EvictByTagAsync(cache, default);
            //le pasamos default pq no importan sus parametros, solo queremos saber que no se ejecuto
            await almacenarArchivos.DidNotReceiveWithAnyArgs().Editar(default, default!, default!);
        }
        
        [TestMethod]
        public async Task Put_ActualizaAutor_CuandoEnviamosAutorConFoto()
        {
            // Preparacion
            var context = ConstruirContext(nombreBD);

            var urlAnterior = "url1";
            var urlNueva = "url2";
            // aseguramos que la función retorne la nueva url y que con esa se actualice la foto del autor
            almacenarArchivos.Editar(default, default!, default!).ReturnsForAnyArgs(urlNueva);
            
            context.Autores.Add(new Autor {
                Nombres = "Ale", 
                Apellidos = "Cast", 
                Identificacion = "ale53",
                Foto = urlAnterior
            });

            await context.SaveChangesAsync();

            var formFile = Substitute.For<IFormFile>();

            var autorCreacionDTO = new AutorCreacionDTOConFoto
            {
                Nombres = "Ale2", 
                Apellidos = "Cast2", 
                Identificacion = "ale53453",
                Foto = formFile
            };
            
            // Prueba
            var respuesta = await controller.Put(1, autorCreacionDTO);
            
            // Verificación
            var resultado = respuesta as StatusCodeResult;

            Assert.AreEqual(expected: 204, actual: resultado!.StatusCode);

            var context2 = ConstruirContext(nombreBD);
            var autorActualizado = await context2.Autores.SingleAsync();

            Assert.AreEqual(expected: "Ale2", actual: autorActualizado.Nombres);
            Assert.AreEqual(expected: "Cast2", actual: autorActualizado.Apellidos);
            Assert.AreEqual(expected: "ale53453", actual: autorActualizado.Identificacion);
            Assert.AreEqual(expected: urlNueva, actual: autorActualizado.Foto);
            await outputCacheStore.Received(1).EvictByTagAsync(cache, default);
            //le pasamos default pq no importan sus parametros, solo queremos saber que no se ejecuto
            await almacenarArchivos.Received(1).Editar(urlAnterior, contenedor, formFile);
        }

        [TestMethod]
        public async Task Patch_Retorna400_CuandoPatchDocEsNulo()
        {
            // prueba
            var respuesta = await controller.Patch(1, patchDocument: null!);

            // verificacion
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(expected: 400, actual: resultado!.StatusCode);
        }
        
        [TestMethod]
        public async Task Patch_Retorna404_CuandoAutorNoExiste()
        {
            // Preparacion
            var patchDoc = new JsonPatchDocument<AutorPatchDTO>();
            
            // prueba
            var respuesta = await controller.Patch(1, patchDoc);

            // verificacion
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(expected: 404, actual: resultado!.StatusCode);
        }
        
        [TestMethod]
        public async Task Patch_RetornaValidationProblem_CuandoHayErrorDeValidacion()
        {
            // Preparacion
            var context = ConstruirContext(nombreBD);
            context.Autores.Add(new Autor
            {
                Nombres = "Ale",
                Apellidos = "Cast",
                Identificacion = "139"
            });

            await context.SaveChangesAsync();
            
            var objectValidator = Substitute.For<IObjectModelValidator>();
            controller.ObjectValidator = objectValidator;

            var mensajeError = "mensaje de error";
            controller.ModelState.AddModelError("", mensajeError);
            
            var patchDoc = new JsonPatchDocument<AutorPatchDTO>();
            
            // prueba
            var respuesta = await controller.Patch(1, patchDoc);

            // verificacion
            var resultado = respuesta as ObjectResult;
            var problemsDetails = resultado!.Value as ValidationProblemDetails;

            Assert.IsNotNull(problemsDetails);
            Assert.AreEqual(expected: 1, actual: problemsDetails.Errors.Keys.Count);
            Assert.AreEqual(expected: mensajeError, actual: problemsDetails.Errors.Values.First().First());
        }
        
        [TestMethod]
        public async Task Patch_ActualizaCampo_CuandoSeLeEnviaUnaOperacion()
        {
            // Preparacion
            var context = ConstruirContext(nombreBD);
            context.Autores.Add(new Autor
            {
                Nombres = "Ale",
                Apellidos = "Cast",
                Identificacion = "139",
                Foto = "url1"
            });

            await context.SaveChangesAsync();
            
            var objectValidator = Substitute.For<IObjectModelValidator>();
            controller.ObjectValidator = objectValidator;
            
            var patchDoc = new JsonPatchDocument<AutorPatchDTO>();
            patchDoc.Operations.Add(new Operation<AutorPatchDTO>("replace", "/nombres", null, "Ale2"));
            
            // prueba
            var respuesta = await controller.Patch(1, patchDoc);

            // verificacion
            var resultado = respuesta as StatusCodeResult;
            Assert.AreEqual(expected: 204, actual: resultado!.StatusCode);
            await outputCacheStore.Received(1).EvictByTagAsync(cache, default);

            var context2 = ConstruirContext(nombreBD);
            var autorBD = await context2.Autores.SingleAsync();

            Assert.AreEqual(expected: "Ale2", autorBD.Nombres);
            Assert.AreEqual(expected: "Cast", autorBD.Apellidos);
            Assert.AreEqual(expected: "139", autorBD.Identificacion);
            Assert.AreEqual(expected: "url1", autorBD.Foto);
        }
    }
}
