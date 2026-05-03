using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BibliotecaAPI.Migrations
{
    /// <inheritdoc />
    public partial class Crea_SP_Usuarios_SetearMalaPaga : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE PROCEDURE Usuario_SetearMalaPaga
	                -- Add the parameters for the stored procedure here
                AS
                BEGIN
	                -- SET NOCOUNT ON added to prevent extra result sets from
	                -- interfering with SELECT statements.
	                SET NOCOUNT ON;

                    -- Insert statements for procedure here
	                update AspNetUsers
	                set
	                MalaPaga = 'True'
	                from Facturas
	                inner join AspNetUsers
	                on AspNetUsers.Id = Facturas.UsuarioId
	                where Pagada = 'False' and FechaLimiteDePago < GETDATE()
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE Usuario_SetearMalaPaga");
        }
    }
}
