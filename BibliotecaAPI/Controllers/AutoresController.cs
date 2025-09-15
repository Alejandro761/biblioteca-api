using AutoMapper;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Controllers
{
    [ApiController]
    [Route("api/autores")]
    public class AutoresController : ControllerBase
    {
        //instancia de ApplicationDbContext para interactuar con la bd
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public AutoresController(ApplicationDbContext context, IMapper mapper) {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IEnumerable<AutorDTO>> Get()
        {
            var autores = await context.Autores.ToListAsync();
            var autoresDTO = mapper.Map<IEnumerable<AutorDTO>>(autores);
            return autoresDTO;
        }

        [HttpGet("{id:int}", Name = "ObtenerAutor")] // api/autores/id?incluirLibros=true|false
        public async Task<ActionResult<AutorConLibrosDTO>> Get(int id, [FromQuery] bool incluirLibros)
        {
            var autor = await context.Autores
                .Include(x => x.libros)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (autor is null)
            {
                return NotFound();
            }

            var autorDTO = mapper.Map<AutorConLibrosDTO>(autor);
            return autorDTO;
        }

        //para indicar que un parametro sea string pero que solo se puedan usar letras (no numeros ni simbolos) entonces usamos alpha, en caso contrario no indicamos el tipo
        //[HttpGet("{nombre:alpha}")]
        //public async Task<IEnumerable<Autor>> Get(string nombre)
        //{
        //    return await context.Autores.Where(x => x.Nombre.Contains(nombre))
        //        //.Include(x => x.libros)
        //        .ToListAsync();
        //}

        //podemos poner parametro opcionales indicandolos con ?
        //[HttpGet("{param1}/{param2?}/{param3?}")]
        //public ActionResult Get(string param1, string? param2, string param3 = "valor por defecto")
        //{
        //    return Ok(new { param1, param2, param3 });
        //}

        [HttpPost]
        public async Task<ActionResult> Post(AutorCreacionDTO autorCracionDTO)
        {
            var autor = mapper.Map<Autor>(autorCracionDTO);
            context.Add(autor);
            await context.SaveChangesAsync();
            var autorDTO = mapper.Map<AutorDTO>(autor);
            return CreatedAtRoute("ObtenerAutor", new {id = autor.Id}, autorDTO);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, AutorCreacionDTO autorCreacionDTO)
        {
            var autor = mapper.Map<Autor>(autorCreacionDTO);
            autor.Id = id;
            context.Update(autor);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id:int}")]
        public async Task<ActionResult> Patch(int id, JsonPatchDocument<AutorPatchDTO> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }

            var autorDB = context.Autores.FirstOrDefault(x => x.Id == id);
            if (autorDB == null)
            {
                return NotFound();
            }

            var autorPatchDTO = mapper.Map<AutorPatchDTO>(autorDB);

            // se aplican los cambios al autor
            patchDocument.ApplyTo(autorPatchDTO, ModelState);

            var esValido = TryValidateModel(autorPatchDTO);

            if (!esValido)
            {
                return ValidationProblem();
            }

            mapper.Map(autorPatchDTO, autorDB);

            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var registrosBorrados = await context.Autores.Where(x => x.Id == id).ExecuteDeleteAsync();

            if (registrosBorrados == 0)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
