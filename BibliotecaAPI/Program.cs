using BibliotecaAPI;
using BibliotecaAPI.Datos;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Servicios;
using BibliotecaAPI.Swagger;
using BibliotecaAPI.Utilidades;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Area de servicios

// builder.Services.AddOutputCache(opciones =>
// {
//     opciones.DefaultExpirationTimeSpan = TimeSpan.FromSeconds(60);
// });

builder.Services.AddStackExchangeRedisOutputCache(opciones =>
{
    opciones.Configuration = builder.Configuration.GetConnectionString("redis");
});

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
builder.Services.AddScoped<MiFIltroDeAccion>();


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

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(opciones =>
{
    opciones.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
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

var app = builder.Build();

// area de middlewares


app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();

app.UseOutputCache();

app.MapControllers();

app.Run();