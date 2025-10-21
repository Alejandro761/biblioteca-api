using BibliotecaAPI.Entidades;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Datos
{
    public class ApplicationDbContext : IdentityDbContext<Usuario>
    {
        public ApplicationDbContext(DbContextOptions opciones) : base(opciones)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //filtro para mostrar aquellos comentarios que no han sido borrados
            modelBuilder.Entity<Comentario>().HasQueryFilter(b => !b.EstaBorrado);
        }
        //con DbSet indicamos que se cree una tabla en la bd a partir de la clase Autor
        //la tabla se llamara Autores
        public DbSet<Autor> Autores { get; set; }
        public DbSet<Libro> Libros { get; set; }
        public DbSet<Comentario> Comentarios { get; set; }
        public DbSet<AutorLibro> AutoresLibros { get; set; }
    }
}
