using System.Text.Json;
using MGMBlazor.Infrastructure.Data;
using MGMBlazor.Services.Sicoob;
using Microsoft.EntityFrameworkCore;

public class SicoobWorkerService : BackgroundService
{
      private readonly IServiceProvider _serviceProvider;
      private readonly ILogger<SicoobWorkerService> _logger;
      public SicoobWorkerService(
            IServiceProvider serviceProvider,
            ILogger<SicoobWorkerService> logger)
      {
            _serviceProvider = serviceProvider;
            _logger = logger;
      }

      protected override async Task ExecuteAsync(CancellationToken stoppingToken)
      {
            _logger.LogInformation("✅ Robô Sicoob iniciado e aguardando as 06:00...");

            while (!stoppingToken.IsCancellationRequested)
            {
                  // Aguarda até as 06:00 da manhã
                  var now = DateTime.Now;
                  var nextRun = now.Date.AddDays(1).AddHours(6);
                  var delay = nextRun - now;

                  await Task.Delay(delay, stoppingToken);

                  _logger.LogInformation("🚀 Robô acordou! Iniciando sincronização diária...");

                  using var scope = _serviceProvider.CreateScope();
                  var sicoob = scope.ServiceProvider.GetRequiredService<ISicoobService>();
                  var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                  // Busca boletos que não foram pagos (Pendente ou Em Aberto) dos últimos 30 dias
                  var pendentes = await db.Cobrancas
                      .Where(c => (c.Status == "Pendente" || c.Status == "Em Aberto")
                             && c.DataCadastro >= DateTime.Today.AddDays(-30))
                      .ToListAsync();

                  foreach (var cob in pendentes)
                  {
                        // 1. Desempacota a tupla (objeto C# e a string JSON original)
                        var (response, jsonBruto) = await sicoob.ConsultarBoletoAsync(cob.NossoNumero);

                        if (response?.Resultado != null)
                        {
                              var novoStatus = response.Resultado.SituacaoBoleto ?? cob.Status;

                              // 2. Se mudou para Liquidado e não temos a data, vamos "caçar" no JSON Bruto
                              if (novoStatus == "Liquidado" && !cob.DataPagamento.HasValue)
                              {
                                    try
                                    {
                                          using JsonDocument doc = JsonDocument.Parse(jsonBruto);
                                          var root = doc.RootElement;

                                          // Navegação segura no JSON para pegar a data real da LIQUIDAÇÃO
                                          if (root.TryGetProperty("resultado", out var resNode) &&
                                              resNode.TryGetProperty("listaHistorico", out var histNode))
                                          {
                                                foreach (var item in histNode.EnumerateArray())
                                                {
                                                      if (item.TryGetProperty("descricaoHistorico", out var desc) &&
                                                          desc.GetString()?.Contains("LIQUIDAÇÃO") == true)
                                                      {
                                                            if (item.TryGetProperty("dataHistorico", out var data) &&
                                                                DateTime.TryParse(data.GetString(), out var dataReal))
                                                            {
                                                                  cob.DataPagamento = DateTime.SpecifyKind(dataReal, DateTimeKind.Utc);
                                                                  _logger.LogInformation($"[ROBÔ] Data real capturada para boleto {cob.NossoNumero}: {dataReal:dd/MM/yyyy}");
                                                                  break;
                                                            }
                                                      }
                                                }
                                          }
                                    }
                                    catch (Exception ex)
                                    {
                                          _logger.LogWarning($"[ROBÔ] Falha ao ler histórico do boleto {cob.NossoNumero}: {ex.Message}");
                                    }

                                    // Fallback: Se não achou no histórico, usa a data atual como plano B
                                    if (!cob.DataPagamento.HasValue)
                                          cob.DataPagamento = DateTime.UtcNow.AddHours(-3);
                              }

                              cob.Status = novoStatus;
                              db.Cobrancas.Update(cob);
                        }

                        await Task.Delay(1000); // Pausa de 1s para respeitar o limite da API
                  }

                  await db.SaveChangesAsync();
            }
      }
}