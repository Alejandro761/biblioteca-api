
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace BibliotecaAPI.Servicios
{
    public class AlmacenadorArchivosAzure : IAlmacenarArchivos
    {
        private readonly string connectionString;
        private readonly ILogger<AlmacenadorArchivosAzure> logger;

        public AlmacenadorArchivosAzure(IConfiguration configuration, ILogger<AlmacenadorArchivosAzure> logger)
        {
            //definir la llave para acceder al servicio en azure
            this.logger = logger;
            // this.logger.LogInformation(configuration.GetConnectionString("AzureStorageConnection"));
            connectionString = configuration.GetConnectionString("AzureStorageConnection")!;
        }
        
        public async Task<string> Almacenar(string contenedor, IFormFile archivo)
        {
            //cliente que nos permita conectarnos a azure storage account
            // System.Console.WriteLine(connectionString);
            var cliente = new BlobContainerClient(connectionString, contenedor);
            //crear el contenedor si no existe
            await cliente.CreateIfNotExistsAsync();
            //politica para permitir acceder a los archivos
            cliente.SetAccessPolicy(PublicAccessType.Blob);

            var extension = Path.GetExtension(archivo.FileName);
            var nombreArchivo = $"{Guid.NewGuid()}{extension}";
            //vamos a obtener un cliente que nos permita subir un archivo con ese nombre (nombreArchivo)
            var blob = cliente.GetBlobClient(nombreArchivo);
            //configuración para colocar el content type
            //para indicar que es una imagen y que permita ver la imagen en un navegador
            var blobHttpHeaders = new BlobHttpHeaders();
            blobHttpHeaders.ContentType = archivo.ContentType;
            //subir archivo
            await blob.UploadAsync(archivo.OpenReadStream(), blobHttpHeaders);
            //retornar la url
            return blob.Uri.ToString();
        }

        public async Task Borrar(string? ruta, string contenedor)
        {
            if (string.IsNullOrEmpty(ruta))
            {
                return;
            }

            var cliente = new BlobContainerClient(connectionString, contenedor);
            await cliente.CreateIfNotExistsAsync();
            var nombreArchivo = Path.GetFileName(ruta);
            var blob = cliente.GetBlobClient(nombreArchivo);
            await blob.DeleteIfExistsAsync();   
        }
    }
}
