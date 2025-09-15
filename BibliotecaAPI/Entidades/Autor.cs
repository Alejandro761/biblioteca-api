using BibliotecaAPI.Validaciones;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata.Ecma335;

namespace BibliotecaAPI.Entidades
{
    public class Autor
    {
        public int Id { get; set; }
        //required indica que la propiedad no será nulo
        [Required (ErrorMessage = "El campo {0} es requerido")]
        [StringLength (150, ErrorMessage ="El campo {0} debe tener {1} caracteres o menos")]
        [PrimeraLetraMayuscula]
        public required string Nombres { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(150, ErrorMessage = "El campo {0} debe tener {1} caracteres o menos")]
        [PrimeraLetraMayuscula]
        public required string Apellidos { get; set; }
        [StringLength(20, ErrorMessage = "El campo {0} debe tener {1} caracteres o menos")]
        public string? Identificacion { get; set; }
        public List<Libro> libros { get; set; } = new List<Libro>();


    }
}
