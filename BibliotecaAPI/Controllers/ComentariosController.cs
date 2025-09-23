using AutoMapper;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Controllers
{
    [ApiController]
    [Route("api/libros/{libroId:int}/comentarios")]
    [Authorize]
    public class ComentariosController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public ComentariosController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<ComentarioDTO>>> Get(int libroId)
        {
            var existeLibro = await context.Libros.AnyAsync(x => x.Id == libroId);

            if (!existeLibro)
            {
                return NotFound();
            }

            var comentarios = await context.Comentarios
                .Include(x => x.Usuario) //para obtener los datos del usuario
                .Where(x => x.LibroId == libroId)
                .OrderBy(x => x.FechaPublicacion)
                .ToListAsync();

            return mapper.Map<List<ComentarioDTO>>(comentarios);
        }

        [HttpGet("{id}", Name = "ObtenerComentario")]
        public async Task<ActionResult<ComentarioDTO>> Get(Guid id)
        {
            var comentario = await context.Comentarios
                                    .Include(x => x.Usuario)
                                    .FirstOrDefaultAsync(x => x.Id == id);

            if (comentario is null)
            {
                return NotFound();
            }

            return mapper.Map<ComentarioDTO>(comentario);

        }

        [HttpPost]
        public async Task<ActionResult> Post(int libroId, ComentarioCreacionDTO comentarioCreacionDTO)
        {
            var existeLibro = await context.Libros.AnyAsync(x => x.Id == libroId);

            if (!existeLibro)
            {
                return NotFound();
            }

            var comentario = mapper.Map<Comentario>(comentarioCreacionDTO);
            comentario.LibroId = libroId;
            comentario.FechaPublicacion = DateTime.UtcNow;
            context.Add(comentario);
            await context.SaveChangesAsync();

            var comentarioDTO = mapper.Map<ComentarioDTO>(comentario);

            return CreatedAtRoute("ObtenerComentario", new { id = comentarioDTO.Id, libroId }, comentarioDTO);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> Patch(Guid id, int libroId, JsonPatchDocument<ComentarioPatchDTO> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }

            var existeLibro = await context.Libros.AnyAsync(x => x.Id == libroId);

            if (!existeLibro)
            {
                return NotFound();
            }

            var comentarioDB = context.Comentarios.FirstOrDefault(x => x.Id == id);
            if (comentarioDB == null)
            {
                return NotFound();
            }

            var comentarioPatchDTO = mapper.Map<ComentarioPatchDTO>(comentarioDB);

            // se aplican los cambios al comentario
            patchDocument.ApplyTo(comentarioPatchDTO, ModelState);

            var esValido = TryValidateModel(comentarioPatchDTO);

            if (!esValido)
            {
                return ValidationProblem();
            }

            mapper.Map(comentarioPatchDTO, comentarioDB);

            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id, int libroId)
        {
            var existeLibro = await context.Libros.AnyAsync(x => x.Id == libroId);

            if (!existeLibro)
            {
                return NotFound();
            }

            var registrosBorrados = await context.Comentarios.Where(x => x.Id == id).ExecuteDeleteAsync();

            if (registrosBorrados == 0)
            {
                NotFound();
            }

            return NoContent();
        }
    }
}
