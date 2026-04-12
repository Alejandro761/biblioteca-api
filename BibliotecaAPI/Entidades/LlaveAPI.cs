namespace BibliotecaAPI.Entidades
{
    public class LlaveAPI
    {
        public int Id { get; set; }
        public required string Llave { get; set; }
        public TipoLlave TipoLlave {get; set;}
        public bool Activa { get; set; }
        //relacion 1:1 con usuario
        public required string UsuarioId { get; set; }
        public Usuario? Usuario { get; set; } 
        public List<RestriccionIP> RestriccionesIP {get; set;} = [];
        public List<RestriccionDominio> RestriccionesDominio {get; set;} = [];
    }

    public enum TipoLlave //enum nos permite tener una enumeración
    {
        Gratuita = 1,
        Profesional = 2
    }
}
