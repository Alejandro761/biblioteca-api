using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BibliotecaAPI.Swagger
{
    public class FiltroAutorizacion : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            //verificar si el controller o un endpoint no tiene Authorize
            if (!context.ApiDescription.ActionDescriptor.EndpointMetadata
            .OfType<AuthorizeAttribute>().Any())
            {
                return;
            }

            //verificar si el controller o un endpoint tiene AllowAnonymous
            if (context.ApiDescription.ActionDescriptor.EndpointMetadata
            .OfType<AllowAnonymousAttribute>().Any())
            {
                return;
            }
            //poner el candado al endpoint en swagger
            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme {
                            Reference = new OpenApiReference {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new String[]{}
                    }
                }
            };
        }
    }
}
