using System.ComponentModel;
using System.IO.Compression;
using AutoMapper;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Servicios;
using BibliotecaAPI.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Controllers
{
    [ApiController]
    [Route("api/autores")]
    [Authorize(Policy = "esadmin")]
    public class AutoresController : ControllerBase
    {
        //instancia de ApplicationDbContext para interactuar con la bd
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IAlmacenarArchivos almacenarArchivos;
        private const string contenedor = "autores";

        public AutoresController(ApplicationDbContext context, IMapper mapper, IAlmacenarArchivos almacenarArchivos)
        {
            this.context = context;
            this.mapper = mapper;
            this.almacenarArchivos = almacenarArchivos;
        }

        [HttpGet]
        //AllowAnonymous indica que este endpoint se puede usar
        // sin estar autenticado
        [AllowAnonymous]
        public async Task<IEnumerable<AutorDTO>> Get([FromQuery] PaginacionDTO paginacionDTO)
        {
            var queryable = context.Autores.AsQueryable();
            await HttpContext.InsertarParametrosPaginacionEnCabecera(queryable);
            var autores = await queryable.OrderBy(x => x.Nombres).Paginar(paginacionDTO).ToListAsync();
            var autoresDTO = mapper.Map<IEnumerable<AutorDTO>>(autores);
            return autoresDTO;
        }

        [HttpGet("{id:int}", Name = "ObtenerAutor")] // api/autores/id?incluirLibros=true|false
        [AllowAnonymous]
        [EndpointSummary("Obtiene autor por Id")]
        [EndpointDescription("Obtiene autor por su Id. Incluye sus libros. Si el autor no existe, se retorna 404.")]
        [ProducesResponseType<AutorConLibrosDTO>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AutorConLibrosDTO>> Get([Description("El id del autor")] int id, [FromQuery] bool incluirLibros)
        {
            var autor = await context.Autores
                .Include(x => x.Libros)
                .ThenInclude(x => x.Libro)
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
        public async Task<ActionResult> Post(AutorCreacionDTO autorCreacionDTO)
        {
            var autor = mapper.Map<Autor>(autorCreacionDTO);
            context.Add(autor);
            await context.SaveChangesAsync();
            var autorDTO = mapper.Map<AutorDTO>(autor);
            return CreatedAtRoute("ObtenerAutor", new { id = autor.Id }, autorDTO);
        }

        [HttpPost("con-foto")]
        //fromform es cuando obtenemos data del formulario, es util cuando recibimos archivos
        public async Task<ActionResult> PostConFoto([FromForm] AutorCreacionDTOConFoto autorCreacionDTO)
        {
            var autor = mapper.Map<Autor>(autorCreacionDTO);

            if (autorCreacionDTO.Foto is not null)
            {
                var url = await almacenarArchivos.Almacenar(contenedor, autorCreacionDTO.Foto);
                autor.Foto = url;
            }
            
            context.Add(autor);
            await context.SaveChangesAsync();
            var autorDTO = mapper.Map<AutorDTO>(autor);
            return CreatedAtRoute("ObtenerAutor", new { id = autor.Id }, autorDTO);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, [FromForm] AutorCreacionDTOConFoto autorCreacionDTO)
        {
            var existeAutor = await context.Autores.AnyAsync(x => x.Id == id);

            if (!existeAutor)
            {
                return NotFound();
            }

            var autor = mapper.Map<Autor>(autorCreacionDTO);
            autor.Id = id;

            if (autorCreacionDTO.Foto is not null)
            {
                var fotoActual = await context.Autores.Where(x => x.Id == id).Select(x => x.Foto).FirstAsync();
                var url = await almacenarArchivos.Editar(fotoActual, contenedor, autorCreacionDTO.Foto);
                autor.Foto = url;
            }
            
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
            // var registrosBorrados = await context.Autores.Where(x => x.Id == id).ExecuteDeleteAsync();

            // if (registrosBorrados == 0)
            // {
            //     return NotFound();
            // }

            var autor = await context.Autores.FirstOrDefaultAsync(x => x.Id == id);

            if (autor is null)
            {
                return NotFound();
            }

            context.Remove(autor);
            await context.SaveChangesAsync();
            await almacenarArchivos.Borrar(autor.Foto, contenedor);

            return NoContent();
        }
    }
}
