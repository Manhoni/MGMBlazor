using MGMBlazor.Domain.Entities;

namespace MGMBlazor.Services.Clientes;

public interface IClienteService
{
      Task<Cliente?> BuscarPorCnpjAsync(string cnpj);
      Task SalvarAsync(Cliente cliente);
      Task AtualizarAsync(Cliente cliente);
}