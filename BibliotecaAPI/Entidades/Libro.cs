using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.Entidades
{
    public class Libro
    {
        public int Id { get; set; }
        [Required]
        public required string Titulo { get; set; }
        public List<AutorLibro> Autores { get; set; } = [];
        //public int AutorId { get; set; }
        //public Autor? Autor { get; set; }
        public List<Comentario> Comentarios { get; set; } = [];
    }
}
