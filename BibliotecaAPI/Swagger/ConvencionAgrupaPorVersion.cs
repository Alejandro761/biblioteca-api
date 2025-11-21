using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace BibliotecaAPI.Swagger
{
    public class ConvencionAgrupaPorVersion : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            // Ejemplo: "Controllers.V1"
            var namespaceDelControlador = controller.ControllerType.Namespace;
            // obtener el ultimo string del controlador que hace referencia a la versión (V1, V2, etc)
            var version = namespaceDelControlador!.Split(".").Last().ToLower();
            // agrupar por version
            controller.ApiExplorer.GroupName = version;
        }
    }
}
