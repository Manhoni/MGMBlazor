using MGMBlazor.Models.Shared;

namespace MGMBlazor.Services.Shared;

public class CepService
{
      private readonly HttpClient _httpClient;
      public CepService(HttpClient httpClient) => _httpClient = httpClient;

      public async Task<CepResponse?> BuscarEnderecoAsync(string cep)
      {
            var cepLimpo = new string(cep.Where(char.IsDigit).ToArray());
            if (cepLimpo.Length != 8) return null;

            try
            {
                  return await _httpClient.GetFromJsonAsync<CepResponse>($"https://viacep.com.br/ws/{cepLimpo}/json/");
            }
            catch { return null; }
      }
}