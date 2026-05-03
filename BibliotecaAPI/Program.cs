using BibliotecaAPI;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Jobs;
using BibliotecaAPI.Servicios;
using BibliotecaAPI.Swagger;
using BibliotecaAPI.Utilidades;
using BibliotecaAPI.Utilidades.V1;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Area de servicios

builder.Services.AddRateLimiter(opciones =>
{
//    opciones.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
//         RateLimitPartition.GetFixedWindowLimiter(
//             // agrupamos a los usuarios por ip o por "desconocido" si no tenemos la ip
//             partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "desconocido",
//             // 5 peticiones cada 10 segundos
//             factory: _ => new FixedWindowRateLimiterOptions
//             {
//                 PermitLimit = 5,
//                 Window = TimeSpan.FromSeconds(10)
//             }
//         )
//     );

    opciones.AddPolicy("general", context =>
    {
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "desconocido",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromSeconds(10)
            }
        );
    });
    
    opciones.AddPolicy("estricta", context =>
    {
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "desconocido",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 2,
                Window = TimeSpan.FromSeconds(5)
            }
        );
    });

    // se divide el periodo en 2 segmentos, cada 5 seg se reciclan las peticiones usadas
    opciones.AddPolicy("movil", context =>
    {
        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "desconocido",
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromSeconds(10),
                SegmentsPerWindow = 2,
                // cuando se acaben las peticiones se podra tener una peticion extra en espera hasta que se pase a la siguiente ventana
                QueueLimit = 1,
                // especificamos a quien se la prioridad en la cola, a las mas nuevas o las mas viejas
                QueueProcessingOrder = QueueProcessingOrder.NewestFirst
            }
        );
    });
    
    // periodos de 10 seg con 5 tokens, cada 10 seg se habilitan 2 tokens de los cuales cada token se habilitará cada 5 seg
    opciones.AddPolicy("cubeta", context =>
    {
        return RateLimitPartition.GetTokenBucketLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "desconocido",
            factory: _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 5,
                TokensPerPeriod = 2,
                ReplenishmentPeriod = TimeSpan.FromSeconds(10)
            }
        );
    });
    
    // solo se le permite al usuario hacer 1 peticion al mismo endpoint, hasta que termine la accion podra volver a hacer alguna peticion a ese endpoint
    opciones.AddPolicy("concurrencia", context =>
    {
        return RateLimitPartition.GetConcurrencyLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "desconocido",
            factory: _ => new ConcurrencyLimiterOptions
            {
                PermitLimit = 1,
            }
        );
    });

    opciones.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    opciones.OnRejected = async (context, cancellationToken) =>
    {
        //  retryafeter es una variable que guarda el tiempo que falta para volver a intentar una peticion
        // por el momento retryafter solo se encuentra disponible en ventanas
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers["Retry-After"] = retryAfter.TotalSeconds.ToString();
        }

        await context.HttpContext.Response.WriteAsync("Limite excedido. Intente más tarde.", 
            cancellationToken);
    };
});

builder.Services.AddOutputCache(opciones =>
{
    opciones.DefaultExpirationTimeSpan = TimeSpan.FromSeconds(60);
});

// builder.Services.AddStackExchangeRedisOutputCache(opciones =>
// {
//     opciones.Configuration = builder.Configuration.GetConnectionString("redis");
// });

//configuración minima necesaria para realizar encriptación
builder.Services.AddDataProtection();

var origenesPermitidos = builder.Configuration.GetSection("origenesPermitidos").Get<string[]>()!;

builder.Services.AddCors(opciones =>
{
    opciones.AddDefaultPolicy(opcionesCors =>
    {
        //AllowAnyOrigin permite que cualquier origen pueda comunicarse
        // opcionesCors.WithOrigins(origenesPermitidos).AllowAnyMethod().AllowAnyHeader().WithExposedHeaders("mi-cabecera");
        opcionesCors.WithOrigins(origenesPermitidos).AllowAnyMethod().AllowAnyHeader().WithExposedHeaders("cantidad-total-registro");
    });
});

builder.Services.AddAutoMapper(typeof(Program));

//modificamos el serealizador de json para ignorar los ciclos en las consultas
// builder.Services.AddControllers().AddJsonOptions(opciones => opciones.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
//al usar dtos ya no es necesario el serializados de json para los ciclos en consultas

builder.Services.AddControllers(opciones =>
{
    //agregar un filtro global
    opciones.Filters.Add<FiltroTiempoEjecucion>();
    // agregar la conveción para agrupar los controladores por versión
    opciones.Conventions.Add(new ConvencionAgrupaPorVersion());
}).AddNewtonsoftJson();

//configuramos ApplicationDbContext como un servicio
builder.Services.AddDbContext<ApplicationDbContext>(opciones => 
    opciones.UseSqlServer("name=DefaultConnection"));

//identityUser es la clase que representa a un usuario
//configuramos identity para que use ApplicationDbContext para
//coenctarse con las tablas de usuarios en la bd
builder.Services.AddIdentityCore<Usuario>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

//UserManager es el manejador de usuarios que nos va a permitir
//registrar usuarios validar contraseñas, etc
builder.Services.AddScoped<UserManager<Usuario>>();
//SignInManager nos permite autenticar usuarios
builder.Services.AddScoped<SignInManager<Usuario>>();
//addtransient si no se necesita manejar estados
builder.Services.AddTransient<IServiciosUsuarios, ServiciosUsuarios>();
builder.Services.AddTransient<IAlmacenarArchivos, AlmacenadorArchivosAzure>();
// addScoped porque se recomienda que como dependemos de dbContext utilicemos scoped
builder.Services.AddScoped<MiFIltroDeAccion>();
builder.Services.AddScoped<FiltroValidaciónLibro>();
builder.Services.AddScoped<BibliotecaAPI.Servicios.V1.IServicioAutores, BibliotecaAPI.Servicios.V1.ServicioAutores>();

builder.Services.AddScoped<BibliotecaAPI.Servicios.V1.IGeneradorEnlaces, BibliotecaAPI.Servicios.V1.GeneradorEnlaces>();

builder.Services.AddScoped<HATEOASAutorAttribute>();
builder.Services.AddScoped<HATEOASAutoresAttribute>();

builder.Services.AddHostedService<FacturasBackgroundService>();

builder.Services.AddScoped<IServicioLlaves, ServicioLlaves>();


//nos permite acceder al contexto http desde cualquier clase
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication().AddJwtBearer(opciones =>
{
    //para que asp.netcore no cambie el numero de un claim por otro
    //de manera automatica
    opciones.MapInboundClaims = false;
    opciones.TokenValidationParameters =
    new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        //validar el tiempo de expiracion del token
        ValidateLifetime = true,
        //validar la llave secreta que se uso para firmar el token
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                builder.Configuration["llavejwt"]!
            )),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(opciones =>
{
    opciones.AddPolicy("esadmin", politica => politica.RequireClaim("esadmin"));
});

// builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(opciones =>
{
    opciones.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Version = "v1",
        Title = "Biblioteca API",
        Description = "Esta es una API para trabajar con datos de autores y libros",
        Contact = new OpenApiContact
        {
            Email = "alejandroxd62@gmail.com",
            Name = "Alejandro Castañeda",
            Url = new Uri("https://github.com/Alejandro761")
        },
        License = new OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/license/mit/")
        }
    });
    
    opciones.SwaggerDoc("v2", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Version = "v2",
        Title = "Biblioteca API",
        Description = "Esta es una API para trabajar con datos de autores y libros",
        Contact = new OpenApiContact
        {
            Email = "alejandroxd62@gmail.com",
            Name = "Alejandro Castañeda",
            Url = new Uri("https://github.com/Alejandro761")
        },
        License = new OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/license/mit/")
        }
    });

    //configuraciones para poder autenticarnos en swagger

    opciones.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    opciones.OperationFilter<FiltroAutorizacion>();
    // opciones.AddSecurityRequirement(new OpenApiSecurityRequirement
    // {
    //     {
    //         new OpenApiSecurityScheme {
    //             Reference = new OpenApiReference {
    //                 Type = ReferenceType.SecurityScheme,
    //                 Id = "Bearer"
    //             }
    //         },
    //         new String[]{}
    //     }
    // });
});

builder.Services.AddOptions<LimitarPeticionesDTO>()
    .Bind(builder.Configuration.GetSection(LimitarPeticionesDTO.Seccion))
    .ValidateDataAnnotations()
    .ValidateOnStart();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (dbContext.Database.IsRelational())
    {
        // cuando se corra la app se ejecutarán las migraciones
        dbContext.Database.Migrate();
    }
}

// area de middlewares

// manejo de errores
app.UseExceptionHandler(exceptionHandlerApp => exceptionHandlerApp.Run(async context =>
{
    var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
    var excepcion = exceptionHandlerFeature?.Error!;

    var error = new Error()
    {
        MensajeError = excepcion.Message,
        StrackTrace = excepcion.StackTrace,
        Fecha = DateTime.UtcNow
    };

    var dbContext = context.RequestServices.GetRequiredService<ApplicationDbContext>();
    dbContext.Add(error);
    await dbContext.SaveChangesAsync();
    await Results.InternalServerError(new
    {
        tipo = "error",
        mensaje = "Ha ocurrido un error inesperado",
        estatus = 500
    }).ExecuteAsync(context);
}));

app.UseSwagger();
app.UseSwaggerUI(opciones =>
{
    //para que swagger divida los endpoints por versiones
    opciones.SwaggerEndpoint("/swagger/v1/swagger.json", "Biblioteca API V1");
    opciones.SwaggerEndpoint("/swagger/v2/swagger.json", "Biblioteca API V2");
});

app.UseStaticFiles();

app.UseRateLimiter();

app.UseCors();

app.UseLimitarPeticiones();

app.UseOutputCache();

app.MapControllers();

app.Run();

public partial class Program { }