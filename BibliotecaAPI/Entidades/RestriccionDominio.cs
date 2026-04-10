namespace BibliotecaAPI.Entidades
{
    public class RestriccionDominio
    {
        public int Id { get; set; }
        public int LlaveId { get; set; }
        public required string Domininio { get; set; }
        public LlaveAPI? Llave { get; set; }
    }
}
