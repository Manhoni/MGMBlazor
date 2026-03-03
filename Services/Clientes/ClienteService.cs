using MGMBlazor.Domain.Entities;
using MGMBlazor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MGMBlazor.Services.Clientes;

public class ClienteService : IClienteService
{
      private readonly IDbContextFactory<AppDbContext> _factory;

      public ClienteService(IDbContextFactory<AppDbContext> factory) => _factory = factory;

      public async Task<Cliente?> BuscarPorCnpjAsync(string cnpj)
      {
            using var context = await _factory.CreateDbContextAsync();
            return await context.Clientes.FirstOrDefaultAsync(c => c.Cnpj == cnpj);
      }

      public async Task SalvarAsync(Cliente cliente)
      {
            using var context = await _factory.CreateDbContextAsync();
            context.Clientes.Add(cliente);
            await context.SaveChangesAsync();
      }

      public async Task AtualizarAsync(Cliente cliente)
      {
            using var context = await _factory.CreateDbContextAsync();
            context.Clientes.Update(cliente);
            await context.SaveChangesAsync();
      }

      public async Task<List<Cliente>> PesquisarClientesAsync(string termo)
      {
            using var context = await _factory.CreateDbContextAsync();
            return await context.Clientes
                .Where(c => EF.Functions.ILike(c.RazaoSocial, $"%{termo}%") || c.Cnpj.Contains(termo))
                  .OrderBy(c => c.RazaoSocial)
                .Take(10) // Traz apenas os 10 melhores resultados
                .ToListAsync();
      }
}