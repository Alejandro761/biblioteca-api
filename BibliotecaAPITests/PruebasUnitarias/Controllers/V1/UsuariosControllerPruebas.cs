using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BibliotecaAPI.Controllers.V1;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Servicios;
using BibliotecaAPITests.Utilidades;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace BibliotecaAPITests.PruebasUnitarias.Controllers.V1
{
    [TestClass]
    public class UsuariosControllerPruebas: BasePruebas
    {
        private string nombreBD = Guid.NewGuid().ToString();
        private UserManager<Usuario> userManager = null!;
        private SignInManager<Usuario> signInManager = null!;
        private UsuariosController controller = null!;

        [TestInitialize]
        public void SetUp ()
        {
            var context = ConstruirContext(nombreBD);
            userManager = Substitute.For<UserManager<Usuario>>(
                Substitute.For<IUserStore<Usuario>>(), null, null, null, null, null, null, null, null
            );

            var miConfiguracion = new Dictionary<string, string>
            {
                {
                    "llavejwt", "ALKSDALKDJLASalejandroeliasjajaaj"
                }
            };

            //construir un proveedor de configuraciones y asi no utilizar un appsettings
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(miConfiguracion!).Build();

            var contextAccessor = Substitute.For<IHttpContextAccessor>();

            var userClaimsFactory = Substitute.For<IUserClaimsPrincipalFactory<Usuario>>();

            signInManager = Substitute.For<SignInManager<Usuario>>(userManager, contextAccessor, userClaimsFactory, null, null, null, null);

            var serviciosUsuarios = Substitute.For<IServiciosUsuarios>();

            var mapper = ConfigurarAutoMapper();

            controller = new UsuariosController(userManager, configuration, signInManager, serviciosUsuarios, context, mapper);
        }

        [TestMethod]
        public async Task Registrar_DevuelveValidationProblem_CuandoNoEsExitoso()
        {
            // Preparación
            var mensajeError = "prueba";
            var credenciales = new CredencialesUsuariosDTO
            {
                Email = "prueba@email.com",
                Password = "1223A!a"
            };

            userManager.CreateAsync(Arg.Any<Usuario>(), Arg.Any<string>()).Returns(IdentityResult.Failed(new IdentityError {Code = "prueba", Description = mensajeError}));

            // Prueba
            var respuesta = await controller.Registrar(credenciales);

            // Verificacion
            var resultado = respuesta.Result as ObjectResult;
            var problemDetails = resultado!.Value as ValidationProblemDetails;             

            Assert.IsNotNull(problemDetails);
            Assert.AreEqual(expected: 1, actual: problemDetails.Errors.Keys.Count);
            Assert.AreEqual(expected: mensajeError, actual: problemDetails.Errors.Values.First().First());
        }
        
        [TestMethod]
        public async Task Registrar_DevuelveToken_CuandoEsExitoso()
        {
            // Preparación
            var credenciales = new CredencialesUsuariosDTO
            {
                Email = "prueba@email.com",
                Password = "1223A!a"
            };

            userManager.CreateAsync(Arg.Any<Usuario>(), Arg.Any<string>()).Returns(IdentityResult.Success);

            // Prueba
            var respuesta = await controller.Registrar(credenciales);

            // Verificacion
            Assert.IsNotNull(respuesta.Value);
            Assert.IsNotNull(respuesta.Value.Token);
        }
        
        [TestMethod]
        public async Task Login_DevuelveValidationProblem_CuandoUsuarioNoExiste()
        {
            // Preparación
            var credenciales = new CredencialesUsuariosDTO
            {
                Email = "prueba@email.com",
                Password = "1223A!a"
            };

            // buscará por email y retornará nulo porque el user no existe
            userManager.FindByEmailAsync(credenciales.Email)!.Returns(Task.FromResult<Usuario>(null!));

            // Prueba
            var respuesta = await controller.Login(credenciales);

            // Verificacion
            var resultado = respuesta.Result as ObjectResult;
            var problemDetails = resultado!.Value as ValidationProblemDetails;             

            Assert.IsNotNull(problemDetails);
            Assert.AreEqual(expected: 1, actual: problemDetails.Errors.Keys.Count);
            Assert.AreEqual(expected: "Login incorrecto", actual: problemDetails.Errors.Values.First().First());
        }

        [TestMethod]
        public async Task Login_DevuelveValidationProblem_CuandoLoginEsIncorrecto()
        {
            // Preparación
            var credenciales = new CredencialesUsuariosDTO
            {
                Email = "prueba@email.com",
                Password = "1223A!a"
            };

            var usuario = new Usuario {Email = credenciales.Email};

            // buscará por email y retornará el usuario que creamos arriba
            userManager.FindByEmailAsync(credenciales.Email)!.Returns(Task.FromResult<Usuario>(usuario));

            // fallara checkPasswordSignInAsync
            signInManager.CheckPasswordSignInAsync(usuario, credenciales.Password, false).Returns(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            // Prueba
            var respuesta = await controller.Login(credenciales);

            // Verificacion
            var resultado = respuesta.Result as ObjectResult;
            var problemDetails = resultado!.Value as ValidationProblemDetails;             

            Assert.IsNotNull(problemDetails);
            Assert.AreEqual(expected: 1, actual: problemDetails.Errors.Keys.Count);
            Assert.AreEqual(expected: "Login incorrecto", actual: problemDetails.Errors.Values.First().First());
        }
        
        [TestMethod]
        public async Task Login_DevuelveToken_CuandoLoginEsCorrecto()
        {
            // Preparación
            var credenciales = new CredencialesUsuariosDTO
            {
                Email = "prueba@email.com",
                Password = "1223A!a"
            };

            var usuario = new Usuario {Email = credenciales.Email};

            // buscará por email y retornará el usuario que creamos arriba
            userManager.FindByEmailAsync(credenciales.Email)!.Returns(Task.FromResult<Usuario>(usuario));

            //  checkPasswordSignInAsync sera correcto
            signInManager.CheckPasswordSignInAsync(usuario, credenciales.Password, false).Returns(Microsoft.AspNetCore.Identity.SignInResult.Success);

            // Prueba
            var respuesta = await controller.Login(credenciales);

            // Verificacion
            Assert.IsNotNull(respuesta.Value);
            Assert.IsNotNull(respuesta.Value.Token);
        }
    }
}
