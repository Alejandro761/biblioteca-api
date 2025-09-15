using System.IO.Compression;
using AutoMapper;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;

namespace BibliotecaAPI.Utilidades
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Autor, AutorDTO>()
                .ForMember(dto => dto.NombreCompleto, config => config.MapFrom(
                    autor => MappearNombreYApellidoAutor(autor)));

            CreateMap<Autor, AutorConLibrosDTO>()
                // .IncludeBase<Autor, AutorDTO>(); //otra forma para reutilizar el primer mapeo
                .ForMember(dto => dto.NombreCompleto, config => config.MapFrom(
                autor => MappearNombreYApellidoAutor(autor)));

            CreateMap<AutorCreacionDTO, Autor>();
            CreateMap<Autor, AutorPatchDTO>().ReverseMap();

            CreateMap<Libro, LibroDTO>();
            CreateMap<LibroCreacionDTO, Libro>()
                .ForMember(ent => ent.Autores, config => config.MapFrom(dto => dto.AutoresIds.Select(id => new AutorLibro { AutorId = id})));

            //CreateMap<Libro, LibroConAutorDTO>()
            //    .ForMember(dto => dto.AutorNombre, config => config.MapFrom(
            //        ent => MappearNombreYApellidoAutor(ent.Autor!)
            //        // con ! le decimos que el autor no sera nulo
            //    ));

            CreateMap<ComentarioCreacionDTO, Comentario>();
            CreateMap<ComentarioPatchDTO, Comentario>().ReverseMap();
            CreateMap<Comentario, ComentarioDTO>();
        }

        private string MappearNombreYApellidoAutor(Autor autor) => $"{autor.Nombres} {autor.Apellidos}";
    }
}
