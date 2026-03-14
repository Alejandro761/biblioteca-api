using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BibliotecaAPI.Datos;
using BibliotecaAPI.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace BibliotecaAPITests.Utilidades
{
    public class BasePruebas
    {
        protected ApplicationDbContext ConstruirContext (string nombreBD)
        {
            var opciones = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(nombreBD).Options;

            var dbContext = new ApplicationDbContext(opciones);
            return dbContext;
        }

        protected IMapper ConfigurarAutoMapper()
        {
            var config = new MapperConfiguration(opciones =>
            {
                opciones.AddProfile(new AutoMapperProfiles());
            });

            return config.CreateMapper();
        }
        
        // metodo que permite en cualquier clase de prueba poder obtener una representación en memoria del webapi para realizar pruebas de integración
        protected WebApplicationFactory<Program> ContruirWebApplicationFactory(string nombreBD, bool ignorarSeguridad = true)
        {
            var factory = new WebApplicationFactory<Program>();
            factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    // remover el proveedor actualmente configurado que es el de sql server ya que no podemos tener dos proveedores al mismo tiempo
                    ServiceDescriptor descriptorDBContext = services.SingleOrDefault(d => d.ServiceType == typeof(IDbContextOptionsConfiguration<ApplicationDbContext>))!;

                    if (descriptorDBContext is not null)
                    {
                        services.Remove(descriptorDBContext);
                    }

                    services.AddDbContext<ApplicationDbContext>(opciones => opciones.UseInMemoryDatabase(nombreBD));

                    if (ignorarSeguridad)
                    {
                        services.AddSingleton<IAuthorizationHandler, AllowAnonymousHandler>();

                        services.AddControllers(opciones =>
                        {
                            opciones.Filters.Add(new UsuarioFalsoFiltro());
                        });
                    }
                });
            });

            return factory;
        }
    }
}
