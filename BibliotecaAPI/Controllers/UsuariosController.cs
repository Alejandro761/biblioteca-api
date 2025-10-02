using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace BibliotecaAPI.Controllers
{
    [ApiController]
    [Route("api/usuarios")]
    public class UsuariosController : ControllerBase
    {
        private readonly UserManager<Usuario> userManager;
        private readonly IConfiguration configuration;
        private readonly SignInManager<Usuario> signInManager;
        private readonly IServiciosUsuarios serviciosUsuarios;

        public UsuariosController(UserManager<Usuario> userManager,
            IConfiguration configuration,
            SignInManager<Usuario> signInManager,
            IServiciosUsuarios serviciosUsuarios
        )
        {
            this.userManager = userManager;
            this.configuration = configuration;
            this.signInManager = signInManager;
            this.serviciosUsuarios = serviciosUsuarios;
        }

        [HttpPost("registro")]
        public async Task<ActionResult<RespuestaAutenticacionDTO>> Registrar(CredencialesUsuariosDTO credencialesUsuariosDTO)
        {
            var usuario = new Usuario
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

        [HttpPut]
        [Authorize]
        public async Task<ActionResult> Put(ActualizarUsuarioDTO actualizarUsuarioDTO)
        {
            var usuario = await serviciosUsuarios.ObtenerUsuario();

            if (usuario is null)
            {
                return NotFound();
            }

            usuario.FechaNacimiento = actualizarUsuarioDTO.FechaNacimiento;

            await userManager.UpdateAsync(usuario);
            return NoContent();
        }

        [HttpGet("renovar-token")]
        [Authorize]
        public async Task<ActionResult<RespuestaAutenticacionDTO>> RenovarToken()
        {
            var usuario = await serviciosUsuarios.ObtenerUsuario();
            if (usuario is null)
            {
                return NotFound();
            }

            var credencialesUsuariosDTO = new CredencialesUsuariosDTO { Email = usuario.Email! };

            var respuetaAutenticacion = await ConstruirToken(credencialesUsuariosDTO);
            return respuetaAutenticacion;
        }

        [HttpPost("hacer-admin")]
        [Authorize(Policy = "esadmin")]
        public async Task<ActionResult> HacerAdmin(EditarClaimDTO editarClaimDTO)
        {
            var usuario = await userManager.FindByEmailAsync(editarClaimDTO.Email);

            if (usuario is null)
            {
                return NotFound();
            }

            //podemos añadirle cualquier valor al claim
            await userManager.AddClaimAsync(usuario, new Claim("esadmin", "true"));
            return NoContent();
        }

        [HttpPost("remover-admin")]
        [Authorize(Policy = "esadmin")]
        public async Task<ActionResult> RemoverAdmin(EditarClaimDTO editarClaimDTO)
        {
            var usuario = await userManager.FindByEmailAsync(editarClaimDTO.Email);

            if (usuario is null)
            {
                return NotFound();
            }

            //podemos añadirle cualquier valor al claim
            await userManager.RemoveClaimAsync(usuario, new Claim("esadmin", "true"));
            return NoContent();
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
