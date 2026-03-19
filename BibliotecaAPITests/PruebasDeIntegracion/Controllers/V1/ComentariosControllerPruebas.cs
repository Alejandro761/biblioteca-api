using System.Net;
using System.Net.Http.Headers;
using BibliotecaAPI.Entidades;
using BibliotecaAPITests.Utilidades;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPITests.PruebasDeIntegracion.Controllers.V1
{
    [TestClass]
    public class ComentariosControllerPruebas: BasePruebas
    {
        private readonly string url = "/api/v1/libros/1/comentarios";
        private string nombreBD = Guid.NewGuid().ToString();
        
        private async Task CrearDataDePrueba ()
        {
            var context = ConstruirContext(nombreBD);
            var autor = new Autor {Nombres = "Alejandro Elías", Apellidos = "Castañeda Ibarra"};
            context.Add(autor);
            await context.SaveChangesAsync();

            var libro = new Libro {Titulo = "Alejandro's Things"};
            libro.Autores.Add(new AutorLibro {Autor = autor});
            context.Add(libro);
            await context.SaveChangesAsync();
        }

        [TestMethod]
        public async Task Delete_Devuelve204_CuandoUsuarioBorraSuPropioComentario()
        {
            // Preparación
            await CrearDataDePrueba();

            var factory = ConstruirWebApplicationFactory(nombreBD, ignorarSeguridad: false);

            var token = await CrearUsuario(nombreBD, factory);

            var context = ConstruirContext(nombreBD);
            var usuario = await context.Users.FirstAsync();

            var comentario = new Comentario {Cuerpo = "contenido", UsuarioId = usuario.Id, LibroId = 1};
            
            context.Add(comentario);
            await context.SaveChangesAsync();

            var cliente = factory.CreateClient();
            cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Prueba
            var respuesta = await cliente.DeleteAsync($"{url}/{comentario.Id}");
            
            // Verificación
            Assert.AreEqual(expected: HttpStatusCode.NoContent, actual: respuesta.StatusCode);
        }
        
        [TestMethod]
        public async Task Delete_Devuelve403_CuandoUsuarioIntentaBorrarElComentarioDeOtro()
        {
            // Preparación
            await CrearDataDePrueba();

            var factory = ConstruirWebApplicationFactory(nombreBD, ignorarSeguridad: false);

            var emailCreadorComentario = "ale@email.com";
            await CrearUsuario(nombreBD, factory, email: emailCreadorComentario, claims: []);

            var context = ConstruirContext(nombreBD);
            var usuarioCreadorComentario = await context.Users.FirstAsync();

            var comentario = new Comentario {
                Cuerpo = "contenido", 
                UsuarioId = usuarioCreadorComentario.Id, 
                LibroId = 1
            };
            
            context.Add(comentario);
            await context.SaveChangesAsync();

            // creando otro usuario
            var tokenUsuarioDistinto = await CrearUsuario(nombreBD, factory, claims: [],
                email: "usuario-disinto@email.com");

            var cliente = factory.CreateClient();
            cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", 
                tokenUsuarioDistinto);

            // Prueba
            var respuesta = await cliente.DeleteAsync($"{url}/{comentario.Id}");
            
            // Verificación
            Assert.AreEqual(expected: HttpStatusCode.Forbidden, actual: respuesta.StatusCode);
        }
    }
}
