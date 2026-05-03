
using BibliotecaAPI.Datos;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Jobs
{
    public class FacturasBackgroundService : BackgroundService
    {
        // necesitamos el provider para poder obtener una instancia del application db context
        private readonly IServiceProvider services;

        public FacturasBackgroundService(IServiceProvider services)
        {
            this.services = services;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while(!stoppingToken.IsCancellationRequested)
                {
                    using (var scope = services.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        Console.WriteLine("Ejecutando proceso de emisión de facturas");
                        await EmitirFacturas(context);
                        await SetearUsuariosMalaPaga(context);
                        // retraso de 1 dia o que se detenga la ejecución
                        await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                    }
                }
            }
            catch(OperationCanceledException)
            {
                // podemos ejecutar código personalizado al detener la ejecución del job
            }
        }

        private async Task SetearUsuariosMalaPaga (ApplicationDbContext context)
        {
            await context.Database.ExecuteSqlAsync($"EXEC Usuario_SetearMalaPaga");
        }

        private async Task EmitirFacturas(ApplicationDbContext context)
        {
            
            var hoy = DateTime.Today;
            var fechaComparacion = hoy.AddMonths(-1);

            var facturasDelMesYaFueronEmitidas = await context.FacturasEmitidas
                .AnyAsync(x => x.Año == fechaComparacion.Year && x.Mes == fechaComparacion.Month);

            if (!facturasDelMesYaFueronEmitidas)
            {
                Console.WriteLine("Facturas del mes no fueron emitidas");
                var fechaInicio = new DateTime(fechaComparacion.Year, fechaComparacion.Month, 1);
                var fechaFin = fechaInicio.AddMonths(1);

                await context.Database.ExecuteSqlAsync(
                    $"EXEC Facturas_Crear {fechaInicio.ToString("yyyy-MM-dd")}, {fechaFin.ToString("yyyy-MM-dd")}");
            }
        }
    }
}
