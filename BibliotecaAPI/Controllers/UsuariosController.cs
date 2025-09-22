using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BibliotecaAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace BibliotecaAPI.Controllers
{
    [ApiController]
    [Route("api/usuarios")]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly IConfiguration configuration;
        private readonly SignInManager<IdentityUser> signInManager;

        public UsuariosController(UserManager<IdentityUser> userManager,
            IConfiguration configuration,
            SignInManager<IdentityUser> signInManager
        )
        {
            this.userManager = userManager;
            this.configuration = configuration;
            this.signInManager = signInManager;
        }

        [HttpPost("registro")]
        [AllowAnonymous]
        public async Task<ActionResult<RespuestaAutenticacionDTO>> Registrar(CredencialesUsuariosDTO credencialesUsuariosDTO)
        {
            var usuario = new IdentityUser
            {
                UserName = credencialesUsuariosDTO.Email,
                Email = credencialesUsuariosDTO.Email
            };

            var resultado = await userManager.CreateAsync(usuario, credencialesUsuariosDTO.Password!);

            if (resultado.Succeeded)
            {
                var respuetaAutenticacion = await ConstruirToken(credencialesUsuariosDTO);
                return respuetaAutenticacion;
            }
            else
            {
                foreach (var error in resultado.Errors)
                {
                    //para no asociar los errores a un campo especifico
                    //el primer parametro debe ser un string vacio
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return ValidationProblem();
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<RespuestaAutenticacionDTO>> Login(CredencialesUsuariosDTO credencialesUsuariosDTO)
        {
            var usuario = await userManager.FindByEmailAsync(credencialesUsuariosDTO.Email);

            if (usuario is null)
            {
                return RetornarLoginIncorrecto();
            }

            var resultado = await signInManager.CheckPasswordSignInAsync(
                usuario,
                credencialesUsuariosDTO.Password!,
                //lockoutOnFailure define si se bloquea la cuenta cuando
                //el usuario se equivoque varias veces con la constraseña
                lockoutOnFailure: false
                );

            if (resultado.Succeeded)
            {
                return await ConstruirToken(credencialesUsuariosDTO);
            }
            else
            {
                return RetornarLoginIncorrecto();
            }
        }

        private ActionResult RetornarLoginIncorrecto()
        {
            //hay que tratar de ser lo mas vagos posibles en el mensaje de error
            //para que no demos informacion importante de la base de datos
            ModelState.AddModelError(string.Empty, "Login incorrecto");
            return ValidationProblem();
        }

        //construccion del json web token
        private async Task<RespuestaAutenticacionDTO> ConstruirToken(CredencialesUsuariosDTO credencialesUsuariosDTO)
        {
            //los claims son info acerca de los usuarios
            var claims = new List<Claim>
            {
                //un claim basicamente es una llave y un valor
                new Claim("email", credencialesUsuariosDTO.Email),
                new Claim("lo que yo quiera", "cualquier valor")
            };

            //buscar al usuario por email
            var usuario = await userManager.FindByEmailAsync(
                credencialesUsuariosDTO.Email
            );

            //buscar los claims del usuario
            var claimsDB = await userManager.GetClaimsAsync(usuario!);

            //agregar los claims de la base de datos a la lista de claims
            claims.AddRange(claimsDB);

            //la llave está en un proveedor de configuraciones
            var llave = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["llavejwt"]!)
            );
            //el algoritmo HmacSha256 nos permite firmar el jwt para que 
            //nadie pueda editar sus claims
            var credenciales = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

            var expiracion = DateTime.UtcNow.AddYears(1);

            var tokenDeSeguridad = new JwtSecurityToken(issuer: null, audience: null,
                claims: claims, expires: expiracion, signingCredentials: credenciales
            );

            //generar el token
            var token = new JwtSecurityTokenHandler().WriteToken(tokenDeSeguridad);

            return new RespuestaAutenticacionDTO
            {
                Token = token,
                Expiracion = expiracion
            };
        }
    }
}
