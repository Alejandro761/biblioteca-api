using Microsoft.AspNetCore.Identity;

namespace BibliotecaAPI.Servicios
{
    public interface IServiciosUsuarios
    {
        Task<IdentityUser?> ObtenerUsuario();
    }
}