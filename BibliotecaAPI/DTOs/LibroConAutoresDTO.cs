using BibliotecaAPI.Entidades;

namespace BibliotecaAPI.DTOs
{
    public class LibroConAutoresDTO: LibroDTO
    {
        public List<AutorDTO> Autores { get; set; } = new List<AutorDTO>();
    }
}
