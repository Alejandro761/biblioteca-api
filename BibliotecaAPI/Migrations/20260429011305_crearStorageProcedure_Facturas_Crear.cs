using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BibliotecaAPI.Migrations
{
    /// <inheritdoc />
    public partial class crearStorageProcedure_Facturas_Crear : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE PROCEDURE Facturas_Crear
				-- Add the parameters for the stored procedure here
				@fechaInicio datetime,
				@fechaFin datetime
			AS
			BEGIN
				-- SET NOCOUNT ON added to prevent extra result sets from
				-- interfering with SELECT statements.
				SET NOCOUNT ON;

				-- Insert statements for procedure here

			--1 dolar por cada dos peticiones
			declare @montoPorCadaPeticion decimal(4,4) = 1.0/2

			--insert into Facturas(UsuarioId, Monto, FechaEmision, FechaLimiteDePago, Pagada)
			select 
				UsuarioId,
				count(*) * @montoPorCadaPeticion as Monto,
				GETDATE() as FechaEmision,
				DATEADD(d, 60, GETDATE()) as FechaLimitePago,
				0 as Pagada
			from Peticiones
			inner join LlaveAPIs
			on LlaveAPIs.Id = Peticiones.LlaveId
			where LlaveAPIs.TipoLlave != 1 and FechaPeticion >= @fechaInicio and
				FechaPeticion < @fechaFin
			group by UsuarioId

			insert into FacturasEmitidas(Mes,Año)
			select
				case MONTH(GETDATE())
				when 1 then 12
				else MONTH(GETDATE()) - 1 end as MES,

				case MONTH(GETDATE())
				when 1 then year(GETDATE()) - 1
				else YEAR(GETDATE()) end as Año
			END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql("DROP PROCEDURE Facturas_Crear");
        }
    }
}
