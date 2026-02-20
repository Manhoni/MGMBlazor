using MGMBlazor.Domain.Entities;
using MGMBlazor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MGMBlazor.Services.Shared;

public class AuditLogService
{
      private readonly IDbContextFactory<AppDbContext> _factory;

      public AuditLogService(IDbContextFactory<AppDbContext> factory)
      {
            _factory = factory;
      }

      public async Task RegistrarLogAsync(string usuario, string operacao, string detalhes, string tela)
      {
            try
            {
                  using var context = await _factory.CreateDbContextAsync();

                  var log = new LogAuditoria
                  {
                        DataHora = DateTime.UtcNow,
                        Usuario = usuario,
                        Operacao = operacao,
                        Detalhes = detalhes,
                        Tela = tela
                  };

                  context.LogsAuditoria.Add(log);
                  await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                  // Log de falha no console para não derrubar o sistema se o log falhar
                  Console.WriteLine($"[ERRO GRAVACAO LOG] {ex.Message}");
            }
      }
}