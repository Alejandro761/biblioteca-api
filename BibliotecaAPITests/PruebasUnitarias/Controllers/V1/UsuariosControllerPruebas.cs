using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BibliotecaAPI.Controllers.V1;
using BibliotecaAPI.Datos;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Servicios;
using BibliotecaAPITests.Utilidades;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace BibliotecaAPITests.PruebasUnitarias.Controllers.V1
{
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
                    "llavejwt", "alejanadksdjakd"
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
    }
}
