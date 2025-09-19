using BibliotecaAPI;
using BibliotecaAPI.Datos;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var diccionarioConfiguraciones = new Dictionary<string, string>
{
    {"quien_soy", "un diccionario en memoria" }
};

builder.Configuration.AddInMemoryCollection(diccionarioConfiguraciones!);

// Area de servicios

builder.Services.AddOptions<PersonaOpciones>().
    Bind(builder.Configuration.GetSection(PersonaOpciones.Seccion))
    //las siguientes funciones validan las restricciones
    //puestas en la clase PersonaOpciones
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddAutoMapper(typeof(Program));

//modificamos el serealizador de json para ignorar los ciclos en las consultas
// builder.Services.AddControllers().AddJsonOptions(opciones => opciones.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
//al usar dtos ya no es necesario el serializados de json para los ciclos en consultas

builder.Services.AddControllers().AddNewtonsoftJson();

//configuramos ApplicationDbContext como un servicio
builder.Services.AddDbContext<ApplicationDbContext>(opciones => 
    opciones.UseSqlServer("name=DefaultConnection"));

var app = builder.Build();


app.MapControllers();



app.Run();
