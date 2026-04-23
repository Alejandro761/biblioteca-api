using BibliotecaAPI.Entidades;

namespace BibliotecaAPI.DTOs
{
    public class LlaveDTO
    {
        public int Id { get; set; }
        public required string Llave { get; set; }
        public bool Activa { get; set; }
        public required string TipoLlave { get; set; }
        public List<RestriccionIPDTO> RestriccionesIP { get; set; } = [];
        public List<RestriccionDominioDTO> RestriccionesDominio { get; set; } = [];
    }
}
