using System.Net;
using System.Text.Json;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using BibliotecaAPITests.Utilidades;

namespace BibliotecaAPITests.PruebasDeIntegracion.Controllers.V1
{
    [TestClass]
    public class AutoresControllerPruebas: BasePruebas
    {
        private static readonly string url = "/api/v1/autores";
        private string nombreBD = Guid.NewGuid().ToString();
        
        [TestMethod]
        public async Task Get_Devuelve404_CuandoAutorNoExiste ()
        {
            // Preparación
            var factory = ConstruirWebApplicationFactory(nombreBD);
            var cliente = factory.CreateClient();

            // Prueba
            var respuesta = await cliente.GetAsync($"{url}/1");

            // verificación
            var statusCode = respuesta.StatusCode;
            Assert.AreEqual(expected: HttpStatusCode.NotFound, actual: statusCode);
        }
        
        [TestMethod]
        public async Task Get_DevuelveAutor_CuandoAutorExiste ()
        {
            // Preparación
            var context = ConstruirContext(nombreBD);
            context.Autores.Add(new Autor() {Nombres = "Ale", Apellidos = "Castañeda"});
            context.Autores.Add(new Autor() {Nombres = "Elías", Apellidos = "Ibarra"});
            await context.SaveChangesAsync();

            var factory = ConstruirWebApplicationFactory(nombreBD);
            var cliente = factory.CreateClient();

            // Prueba
            var respuesta = await cliente.GetAsync($"{url}/1");

            // verificación
            
            // verifica que tuvimos un código de status tipo 200
            respuesta.EnsureSuccessStatusCode();

            var autor = JsonSerializer.Deserialize<AutorConLibrosDTO>(
                // ReadAsStringAsync permite obtener el cuerpo de la respueta http en string
                await respuesta.Content.ReadAsStringAsync(), jsonSerializerOptions
            )!;

            Assert.AreEqual(expected: 1, autor.Id);            
        }

        [TestMethod]
        public async Task Post_Devuelve401_CuandoUsuarioNoEstaAutenticado()
        {
            // Preparación
            // false para no ignorar la seguridad
            var factory = ConstruirWebApplicationFactory(nombreBD, ignorarSeguridad: false);
            
            var cliente = factory.CreateClient();
            var autorCreacionDTO = new AutorCreacionDTO {Nombres = "Alex", Apellidos = "Castañeda", Identificacion = "1299"};
            
            // Prueba
            var respuesta = await cliente.PostAsJsonAsync(url, autorCreacionDTO);

            // Verificación
            Assert.AreEqual(expected: HttpStatusCode.Unauthorized, actual: respuesta.StatusCode);
        }
    }
}
