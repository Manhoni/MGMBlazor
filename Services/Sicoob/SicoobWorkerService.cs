using MGMBlazor.Infrastructure.Data;
using MGMBlazor.Services.Sicoob;
using Microsoft.EntityFrameworkCore;

public class SicoobWorkerService : BackgroundService
{
      private readonly IServiceProvider _serviceProvider;
      public SicoobWorkerService(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

      protected override async Task ExecuteAsync(CancellationToken stoppingToken)
      {
            while (!stoppingToken.IsCancellationRequested)
            {
                  // Aguarda até as 06:00 da manhã
                  var now = DateTime.Now;
                  var nextRun = now.Date.AddDays(1).AddHours(6);
                  var delay = nextRun - now;

                  await Task.Delay(delay, stoppingToken);

                  using var scope = _serviceProvider.CreateScope();
                  var sicoob = scope.ServiceProvider.GetRequiredService<ISicoobService>();
                  var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                  // Busca boletos pendentes dos últimos 30 dias
                  var pendentes = await db.Cobrancas
                      .Where(c => c.Status == "Pendente" && c.DataCadastro >= DateTime.Today.AddDays(-30))
                      .ToListAsync();

                  foreach (var cob in pendentes)
                  {
                        var result = await sicoob.ConsultarBoletoAsync(cob.NossoNumero);
                        if (result?.Resultado != null)
                        {
                              cob.Status = result.Resultado.SituacaoBoleto ?? "Pendente";
                              db.Cobrancas.Update(cob);
                        }
                        await Task.Delay(1000); // Pausa de 1s entre consultas para não estressar a API
                  }
                  await db.SaveChangesAsync();
            }
      }
}