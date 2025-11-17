namespace BibliotecaAPI.Entidades
{
    public class Error
    {
        public Guid Id { get; set; }
        public required string MensajeError { get; set; }
        public string? StrackTrace { get; set; } // es la secuencia de código que llevo al error
        public DateTime Fecha { get; set; }
    }
}
