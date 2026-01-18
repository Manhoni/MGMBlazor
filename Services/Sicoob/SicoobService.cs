using System.Net.Http.Headers;
using System.Text.Json;
using MGMBlazor.Models.Sicoob;
using MGMBlazor.Domain.Entities;
using MGMBlazor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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

    // --- MÉTODOS AUXILIARES DE AMBIENTE ---

    private bool IsSandbox() => _config.GetValue<bool>("SicoobConfig:UsarSandbox");

    private string GetBaseUrl()
    {
        return IsSandbox() 
            ? "https://sandbox.sicoob.com.br/sicoob/sandbox/cobranca-bancaria/v3" 
            : _config["SicoobConfig:Api-cobranca-bancaria-Url"]!;
    }

    private string GetClientId()
    {
        return IsSandbox() 
            ? _config["SicoobConfig:ClientIdSandbox"]! 
            : _config["SicoobConfig:ClientId"]!;
    }

    // --- GESTÃO DE TOKEN ---

    private async Task EnsureTokenAsync()
    {
        if (IsSandbox())
        {
            Console.WriteLine("[DEBUG-SICOOB] Ambiente: SANDBOX. Usando Token estático do appsettings.");
            _accessToken = _config["SicoobConfig:TokenSandbox"];
            return; 
        }

        if (!string.IsNullOrEmpty(_accessToken) && DateTime.Now < _tokenExpiration.AddSeconds(-30))
        {
            Console.WriteLine("[DEBUG-SICOOB] Produção: Reutilizando Token ainda válido.");
            return;
        }

        Console.WriteLine("[DEBUG-SICOOB] Produção: Iniciando requisição de Token via mTLS (Certificado)...");

        var requestBody = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", GetClientId()),
            new KeyValuePair<string, string>("scope", _config["SicoobConfig:Scopo"]!)
        });

        var response = await _httpClient.PostAsync(_config["SicoobConfig:AuthUrl"], requestBody);
        
        if (!response.IsSuccessStatusCode)
        {
            var erro = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[DEBUG-SICOOB] ERRO ao obter token: {erro}");
            response.EnsureSuccessStatusCode();
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        _accessToken = doc.RootElement.GetProperty("access_token").GetString();
        _tokenExpiration = DateTime.Now.AddSeconds(doc.RootElement.GetProperty("expires_in").GetInt32());
        
        Console.WriteLine("[DEBUG-SICOOB] Produção: Token obtido com sucesso!");
    }

    private void SetDefaultHeaders()
    {
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        _httpClient.DefaultRequestHeaders.Add("client_id", GetClientId());
    }

    // --- MÉTODOS DE AÇÃO (INCLUIR, CONSULTAR, BAIXAR) ---

    public async Task<BoletoResponse?> IncluirBoletoAsync(int notaFiscalEmitidaDbId, BoletoRequest request)
    {
        await EnsureTokenAsync();
        SetDefaultHeaders();

        string url = $"{GetBaseUrl()}/boletos";
        Console.WriteLine($"[DEBUG-SICOOB] Enviando inclusão de boleto para: {url}");

        // O Sicoob V3 exige envio em Array []
        var response = await _httpClient.PostAsJsonAsync(url, new[] { request });

        if (!response.IsSuccessStatusCode)
        {
            var erro = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[DEBUG-SICOOB] ERRO na inclusão ({response.StatusCode}): {erro}");
            throw new Exception($"Erro Sicoob: {erro}");
        }

        var boletoGerado = await response.Content.ReadFromJsonAsync<BoletoResponse>();

        if (boletoGerado?.Resultado != null)
        {
            Console.WriteLine($"[DEBUG-SICOOB] Sucesso! Nosso Número: {boletoGerado.Resultado.NossoNumero}");

            // Tratamento de Data para o Postgres (Tryparse para não quebrar com string vazia)
            DateTime dataVenc = DateTime.Now;
            if (DateTime.TryParse(boletoGerado.Resultado.DataVencimento, out var dtParsed)) {
                dataVenc = dtParsed.ToUniversalTime(); // Postgres prefere UTC
            }

            // SALVANDO NO BANCO DE DADOS
            var novaCobranca = new Cobranca
            {
                NotaFiscalEmitidaId = notaFiscalEmitidaDbId,
                NossoNumero = boletoGerado.Resultado.NossoNumero,
                LinhaDigitavel = boletoGerado.Resultado.LinhaDigitavel ?? "",
                CodigoBarras = boletoGerado.Resultado.CodigoBarras,
                QrCodePix = boletoGerado.Resultado.QrCode,
                Valor = boletoGerado.Resultado.Valor,
                DataVencimento = dataVenc,
                Status = "Pendente",
                DataCadastro = DateTime.UtcNow
            };

            _dbContext.Cobrancas.Add(novaCobranca);
            await _dbContext.SaveChangesAsync();
            Console.WriteLine($"[DEBUG-SICOOB] Cobrança salva no banco vinculada à Nota ID {notaFiscalEmitidaDbId}.");
        }

        return boletoGerado;
    }

    public async Task<BoletoResponse?> ConsultarBoletoAsync(long nossoNumero)
    {
        await EnsureTokenAsync();
        SetDefaultHeaders();

        var numeroCliente = IsSandbox() ? 25546454 : _config.GetValue<long>("SicoobConfig:NumeroCliente");
        var url = $"{GetBaseUrl()}/boletos?numeroCliente={numeroCliente}&codigoModalidade=1&nossoNumero={nossoNumero}";
        
        Console.WriteLine($"[DEBUG-SICOOB] Consultando boleto: {nossoNumero}");
        return await _httpClient.GetFromJsonAsync<BoletoResponse>(url);
    }

    public async Task<bool> BaixarBoletoAsync(long nossoNumero)
    {
        await EnsureTokenAsync();
        SetDefaultHeaders();

        var numeroCliente = IsSandbox() ? 25546454 : _config.GetValue<long>("SicoobConfig:NumeroCliente");
        var body = new { numeroCliente = numeroCliente, codigoModalidade = 1 };
        
        Console.WriteLine($"[DEBUG-SICOOB] Solicitando baixa do boleto: {nossoNumero}");
        var response = await _httpClient.PatchAsJsonAsync($"{GetBaseUrl()}/boletos/baixa/{nossoNumero}", body);
        
        if (response.IsSuccessStatusCode)
        {
            var cobranca = await _dbContext.Cobrancas.FirstOrDefaultAsync(c => c.NossoNumero == nossoNumero);
            if (cobranca != null)
            {
                cobranca.Status = "Baixado";
                await _dbContext.SaveChangesAsync();
                Console.WriteLine("[DEBUG-SICOOB] Baixa efetuada com sucesso no Sicoob e no Postgres.");
            }
        }
        return response.IsSuccessStatusCode;
    }
}