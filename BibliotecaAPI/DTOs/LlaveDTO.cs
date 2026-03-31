using BibliotecaAPI.Entidades;

namespace BibliotecaAPI.DTOs
{
    public class LlaveDTO
    {
        public int Id { get; set; }
        public required string LLave { get; set; }
        public bool Activa { get; set; }
        public required string TipoLlave { get; set; }
    }
}
