using System.Net.Http.Headers;
using System.Text.Json;
using MGMBlazor.Models.Sicoob;
using MGMBlazor.Domain;
using MGMBlazor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using MGMBlazor.Domain.Entities;

namespace MGMBlazor.Services.Sicoob;

public class SicoobService : ISicoobService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly AppDbContext _dbContext;
    private string? _accessToken;
    private DateTime _tokenExpiration;

    public SicoobService(HttpClient httpClient, IConfiguration config, AppDbContext dbContext)
    {
        _httpClient = httpClient;
        _config = config;
        _dbContext = dbContext;
    }

    private async Task EnsureTokenAsync()
    {
        if (!string.IsNullOrEmpty(_accessToken) && DateTime.Now < _tokenExpiration.AddSeconds(-30))
            return;

        var requestBody = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", _config["SicoobConfig:ClientId"]!),
            new KeyValuePair<string, string>("scope", _config["SicoobConfig:Scope"]!)
        });

        var response = await _httpClient.PostAsync(_config["SicoobConfig:AuthUrl"], requestBody);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        _accessToken = doc.RootElement.GetProperty("access_token").GetString();
        _tokenExpiration = DateTime.Now.AddSeconds(doc.RootElement.GetProperty("expires_in").GetInt32());
    }

    public async Task<BoletoResponse?> IncluirBoletoAsync(int notaFiscalId, BoletoRequest request)
    {
        await EnsureTokenAsync();

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        _httpClient.DefaultRequestHeaders.Add("client_id", _config["SicoobConfig:ClientId"]);

        // O Sicoob V3 exige envio em Array []
        var response = await _httpClient.PostAsJsonAsync($"{_config["SicoobConfig:Api-cobranca-bancaria-Url"]}/boletos", new[] { request });

        if (!response.IsSuccessStatusCode)
        {
            var erro = await response.Content.ReadAsStringAsync();
            throw new Exception($"Erro Sicoob: {erro}");
        }

        var boletoGerado = await response.Content.ReadFromJsonAsync<BoletoResponse>();

        if (boletoGerado?.Resultado != null)
        {
            // --- SALVANDO NO POSTGRES ---
            var novaCobranca = new Cobranca
            {
                NotaFiscalEmitidaId = notaFiscalId,
                NossoNumero = boletoGerado.Resultado.NossoNumero,
                LinhaDigitavel = boletoGerado.Resultado.LinhaDigitavel ?? "",
                CodigoBarras = boletoGerado.Resultado.CodigoBarras,
                QrCodePix = boletoGerado.Resultado.QrCode,
                Valor = boletoGerado.Resultado.Valor,
                DataVencimento = boletoGerado.Resultado.DataVencimento.DateTime,
                Status = "Pendente",
                DataCadastro = DateTime.Now
            };

            _dbContext.Cobrancas.Add(novaCobranca);
            await _dbContext.SaveChangesAsync();
        }

        return boletoGerado;
    }

    public async Task<BoletoResponse?> ConsultarBoletoAsync(long nossoNumero)
    {
        await EnsureTokenAsync();
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        _httpClient.DefaultRequestHeaders.Add("client_id", _config["SicoobConfig:ClientId"]);

        var url = $"{_config["SicoobConfig:Api-cobranca-bancaria-Url"]}/boletos?numeroCliente={_config["SicoobConfig:NumeroCliente"]}&codigoModalidade=1&nossoNumero={nossoNumero}";
        return await _httpClient.GetFromJsonAsync<BoletoResponse>(url);
    }

    public async Task<bool> BaixarBoletoAsync(long nossoNumero)
    {
        await EnsureTokenAsync();
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        _httpClient.DefaultRequestHeaders.Add("client_id", _config["SicoobConfig:ClientId"]);

        var body = new { numeroCliente = int.Parse(_config["SicoobConfig:NumeroCliente"]!), codigoModalidade = 1 };
        var response = await _httpClient.PatchAsJsonAsync($"{_config["SicoobConfig:ApiUrl"]}/boletos/baixa/{nossoNumero}", body);
        
        if (response.IsSuccessStatusCode)
        {
            var cobranca = await _dbContext.Cobrancas.FirstOrDefaultAsync(c => c.NossoNumero == nossoNumero);
            if (cobranca != null)
            {
                cobranca.Status = "Baixado";
                await _dbContext.SaveChangesAsync();
            }
        }
        return response.IsSuccessStatusCode;
    }
}