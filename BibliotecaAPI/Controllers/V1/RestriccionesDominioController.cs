using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Servicios;
using BibliotecaAPI.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Controllers.V1
{
    [ApiController]
    [Route("api/v1/restriccionesdominio")]
    [Authorize]
    [DeshabilitarLimitarPeticiones]
    public class RestriccionesDominioController: ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IServiciosUsuarios serviciosUsuarios;
        private readonly ILogger<RestriccionesDominioController> logger;

        public RestriccionesDominioController(ApplicationDbContext context,
            IServiciosUsuarios serviciosUsuarios, ILogger<RestriccionesDominioController> logger)
        {
            this.context = context;
            this.serviciosUsuarios = serviciosUsuarios;
            this.logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult> Post(RestriccionDominioCreacionDTO restriccionDominioCreacionDTO)
        {
            var llaveDB = await context.LlaveAPIs.FirstOrDefaultAsync(x => x.Id == 
                restriccionDominioCreacionDTO.LlaveId);

            logger.LogInformation("llaveDB is null ??");
            
            if (llaveDB is null)
            {
            logger.LogInformation("yes");
                return NotFound();
            }
            logger.LogInformation("no");

            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            logger.LogInformation("obtenerUsuarioId");

            if (llaveDB.UsuarioId != usuarioId)
            {
                return Forbid();
            }

            var restriccionDominio = new RestriccionDominio
            {
                LlaveId = restriccionDominioCreacionDTO.LlaveId,
                Domininio = restriccionDominioCreacionDTO.Dominio
            };

            context.Add(restriccionDominio);
            await context.SaveChangesAsync();

            return NoContent();
        }
        
        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, RestriccionDominioActualizacionDTO 
            restriccionDominioActualizacionDTO)
        {
            var restriccionDB = await context.RestriccionesDominio.Include(x => x.Llave).FirstOrDefaultAsync(x => x.Id == id); 

            if (restriccionDB is null)
            {
                return NotFound();
            }

            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();

            if (restriccionDB.Llave!.UsuarioId != usuarioId)
            {
                return Forbid();
            }

            restriccionDB.Domininio = restriccionDominioActualizacionDTO.Dominio;

            await context.SaveChangesAsync();

            return NoContent();
        }
        
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var restriccionDB = await context.RestriccionesDominio.Include(x => x.Llave).FirstOrDefaultAsync(x => x.Id == id); 

            if (restriccionDB is null)
            {
                return NotFound();
            }

            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();

            if (restriccionDB.Llave!.UsuarioId != usuarioId)
            {
                return Forbid();
            }

            context.Remove(restriccionDB);
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}
