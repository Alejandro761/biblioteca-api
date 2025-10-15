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
            CreateMap<AutorCreacionDTOConFoto, Autor>()
                .ForMember(ent => ent.Foto, config => config.Ignore());
            CreateMap<Autor, AutorPatchDTO>().ReverseMap();

            CreateMap<AutorLibro, LibroDTO>()
                .ForMember(dto => dto.Id, config => config.MapFrom(ent => ent.LibroId))
                .ForMember(dto => dto.Titulo, config => config.MapFrom(ent => ent.Libro!.Titulo));

            CreateMap<Libro, LibroDTO>();
            CreateMap<LibroCreacionDTO, Libro>()
                .ForMember(ent => ent.Autores, config => config.MapFrom(dto => dto.AutoresIds.Select(id => new AutorLibro { AutorId = id })));

            CreateMap<Libro, LibroConAutoresDTO>();

            CreateMap<AutorLibro, AutorDTO>()
                .ForMember(dto => dto.Id, config => config.MapFrom(ent => ent.AutorId))
                .ForMember(dto => dto.NombreCompleto, config => config.MapFrom(ent => MappearNombreYApellidoAutor(ent.Autor!)));

            //CreateMap<Libro, LibroConAutorDTO>()
            //    .ForMember(dto => dto.AutorNombre, config => config.MapFrom(
            //        ent => MappearNombreYApellidoAutor(ent.Autor!)
            //        // con ! le decimos que el autor no sera nulo
            //    ));

            CreateMap<LibroCreacionDTO, AutorLibro>()
                .ForMember(ent => ent.Libro,
                config => config.MapFrom(dto => new Libro { Titulo = dto.Titulo })
                );

            CreateMap<ComentarioCreacionDTO, Comentario>();
            CreateMap<ComentarioPatchDTO, Comentario>().ReverseMap();
            CreateMap<Comentario, ComentarioDTO>()
                .ForMember(dto => dto.UsuarioEmail,
                    config => config.MapFrom(ent => ent.Usuario!.Email)
                );

            CreateMap<Usuario, UsuarioDTO>();
        }

        private string MappearNombreYApellidoAutor(Autor autor) => $"{autor.Nombres} {autor.Apellidos}";
    }
}
