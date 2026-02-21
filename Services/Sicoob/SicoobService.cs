using System.Net.Http.Headers;
using System.Text.Json;
using MGMBlazor.Models.Sicoob;
using MGMBlazor.Domain.Entities;
using MGMBlazor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;

namespace MGMBlazor.Services.Sicoob;

public class SicoobService : ISicoobService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly IDbContextFactory<AppDbContext> _factory;
    private string? _accessToken;
    private DateTime _tokenExpiration;

    public SicoobService(HttpClient httpClient, IConfiguration config, IDbContextFactory<AppDbContext> factory)
    {
        _httpClient = httpClient;
        _config = config;
        _factory = factory;
    }
    //public enum PeriodoParcelamento { Mensal, Quinzenal, Semanal }

    // --- MÉTODOS AUXILIARES DE AMBIENTE ---

    private bool IsSandbox() => _config.GetValue<bool>("SicoobConfig:UsarSandbox");

    private string GetBaseUrl()
    {
        return IsSandbox()
            ? _config["SicoobConfig:UrlSandbox"]!
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
            Console.WriteLine("Produção: Reutilizando Token ainda válido.");
            return;
        }

        Console.WriteLine("Produção Sicoob: Iniciando requisição de Token via mTLS (Certificado)...");

        var requestBody = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", GetClientId()),
            new KeyValuePair<string, string>("scope", _config["SicoobConfig:Escopo"]!)
        });

        var response = await _httpClient.PostAsync(_config["SicoobConfig:AuthUrl"], requestBody);

        if (!response.IsSuccessStatusCode)
        {
            var erro = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"ERRO ao obter token: {erro}");
            response.EnsureSuccessStatusCode();
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        _accessToken = doc.RootElement.GetProperty("access_token").GetString();
        _tokenExpiration = DateTime.Now.AddSeconds(doc.RootElement.GetProperty("expires_in").GetInt32());

        Console.WriteLine("Produção Sicoob: Token obtido com sucesso!");
    }

    private void SetDefaultHeaders()
    {
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        _httpClient.DefaultRequestHeaders.Add("client_id", GetClientId());
    }

    public async Task<List<Cobranca>> ListarCobrancasAsync(DateTime inicio, DateTime fim)
    {
        using var _dbContext = await _factory.CreateDbContextAsync();
        return await _dbContext.Cobrancas
            .Include(c => c.NotaFiscalEmitida) // Traz os dados da nota vinculada
            .Where(c => c.DataCadastro >= inicio && c.DataCadastro <= fim.AddDays(1))
            .OrderByDescending(c => c.Id)
            .ToListAsync();
    }

    // --- MÉTODOS DE AÇÃO (INCLUIR, CONSULTAR, BAIXAR) ---

    public async Task<List<BoletoResponse>> GerarLoteBoletosAsync(
    int? notaId,
    BoletoRequest requestBase,
    int totalParcelas,
    PeriodoParcelamento periodo)
    {
        var resultados = new List<BoletoResponse>();
        decimal valorTotalOriginal = requestBase.Valor;

        // 1. Cálculo matemático das parcelas (Tratando centavos)
        decimal valorParcelaBase = Math.Floor((valorTotalOriginal / totalParcelas) * 100) / 100;
        decimal sobraCentavos = valorTotalOriginal - (valorParcelaBase * totalParcelas);

        // Pegamos a data do primeiro vencimento que o usuário escolheu na tela
        DateTime dataVencimentoReferencia = DateTime.Parse(requestBase.DataVencimento);

        for (int i = 1; i <= totalParcelas; i++)
        {
            DateTime vencimentoParcela = dataVencimentoReferencia.AddMonths(i - 1);
            string dataMultaParcela = vencimentoParcela.AddDays(1).ToString("yyyy-MM-dd");

            var usarSandbox = _config.GetValue<bool>("SicoobConfig:UsarSandbox");

            // 2. Criamos uma cópia da requisição para cada parcela
            var requestParcela = new BoletoRequest
            {
                NumeroCliente = requestBase.NumeroCliente,
                NumeroContaCorrente = requestBase.NumeroContaCorrente,
                Pagador = requestBase.Pagador,
                GerarPdf = true,

                // Atribui o número da parcela (1, 2, 3...)
                NumeroParcela = i,

                // Valor da parcela + sobra dos centavos apenas na primeira
                Valor = (i == 1) ? (valorParcelaBase + sobraCentavos) : valorParcelaBase,

                // Lógica de Data baseada no Período
                DataVencimento = usarSandbox ? "2018-09-20" : CalcularVencimento(dataVencimentoReferencia, i, periodo),
                DataMulta = usarSandbox ? "2018-09-20" : dataMultaParcela,
                DataJurosMora = usarSandbox ? "2018-09-20" : dataMultaParcela,

                DataEmissao = usarSandbox ? "2018-09-20" : requestBase.DataEmissao,

                // O "SeuNumero" precisa ser único para o Sicoob não rejeitar duplicidade
                SeuNumero = $"{requestBase.SeuNumero}/{i:D2}"
            };

            // 3. Chama a função que já existe para registrar no banco e no Postgres
            var response = await IncluirBoletoAsync(notaId, requestParcela);

            if (response != null) resultados.Add(response);
        }

        return resultados;
    }

    // Função auxiliar para as datas
    private string CalcularVencimento(DateTime baseDate, int parcela, PeriodoParcelamento periodo)
    {
        DateTime novaData = periodo switch
        {
            PeriodoParcelamento.Mensal => baseDate.AddMonths(parcela - 1),
            PeriodoParcelamento.Quinzenal => baseDate.AddDays((parcela - 1) * 15),
            PeriodoParcelamento.Semanal => baseDate.AddDays((parcela - 1) * 7),
            _ => baseDate
        };
        return novaData.ToString("yyyy-MM-dd");
    }

    public async Task<BoletoResponse?> IncluirBoletoAsync(int? notaFiscalEmitidaDbId, BoletoRequest request)
    {
        if (request.Valor <= 0)
        {
            throw new Exception("Não é possível gerar um boleto com valor zero ou negativo.");
        }

        await EnsureTokenAsync();
        SetDefaultHeaders();

        // Configuração para ignorar nulos e seguir a risca o que o Sicoob quer
        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true,
            WriteIndented = false,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        // --- json para DEBUG ---
        // string jsonParaEnviar = JsonSerializer.Serialize(request, options);
        // Console.WriteLine("\n[DEBUG-SICOOB] JSON QUE O C# ESTÁ GERANDO:");
        // Console.WriteLine(jsonParaEnviar);
        // Console.WriteLine("-------------------------------------------\n");
        // ----------------------------------------

        string url = $"{GetBaseUrl()}/boletos";
        //Console.WriteLine($"[DEBUG-SICOOB] Enviando inclusão de boleto para: {url}");

        // O Sicoob V3 exige envio em Array []
        var response = await _httpClient.PostAsJsonAsync(url, request, options);

        if (!response.IsSuccessStatusCode)
        {
            var erro = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"ERRO na inclusão ({response.StatusCode}): {erro}");
            throw new Exception($"Erro Sicoob: {erro}");
        }

        var boletoGerado = await response.Content.ReadFromJsonAsync<BoletoResponse>();

        if (boletoGerado?.Resultado != null)
        {
            using var _dbContext = await _factory.CreateDbContextAsync();

            Console.WriteLine($"Sucesso! Nosso Número: {boletoGerado.Resultado.NossoNumero}");

            // Tratamento de Data para o Postgres (Tryparse para não quebrar com string vazia)
            DateTime dataVenc = DateTime.Now;
            if (DateTime.TryParse(boletoGerado.Resultado.DataVencimento, out var dtParsed))
            {
                dataVenc = dtParsed.ToUniversalTime(); // Postgres prefere UTC
            }

            var pdfFinal = boletoGerado.Resultado.PdfBoleto; // somente para salvar no banco uma vez para mandar no suporte API Sicoob
            /*
            string? pdfFinal = null;
            if (!string.IsNullOrEmpty(boletoGerado.Resultado.PdfBoleto))
            {
                // Limpeza preventiva para produção
                pdfFinal = boletoGerado.Resultado.PdfBoleto.Trim().Replace("\n", "").Replace("\r", "");

                // Verificação de integridade mínima para Produção
                if (pdfFinal.Length % 4 != 0) pdfFinal = null;
            }
            */

            // SALVANDO NO BANCO DE DADOS
            var novaCobranca = new Cobranca
            {
                NotaFiscalEmitidaId = notaFiscalEmitidaDbId,
                NossoNumero = boletoGerado.Resultado.NossoNumero,
                LinhaDigitavel = boletoGerado.Resultado.LinhaDigitavel ?? "",
                CodigoBarras = boletoGerado.Resultado.CodigoBarras,
                QrCodePix = boletoGerado.Resultado.QrCode,
                Valor = boletoGerado.Resultado.Valor,
                NumeroParcela = request.NumeroParcela,
                DataVencimento = dataVenc,
                DataCadastro = DateTime.UtcNow,
                PdfBase64 = pdfFinal,
                JsonRetorno = JsonSerializer.Serialize(boletoGerado),
                Status = "Pendente"
            };

            _dbContext.Cobrancas.Add(novaCobranca);
            await _dbContext.SaveChangesAsync();
            Console.WriteLine($"Cobrança salva no banco vinculada à Nota ID {notaFiscalEmitidaDbId}.");
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

        // 1. Faz a chamada e pega a resposta bruta
        var response = await _httpClient.GetAsync(url);
        var jsonRaw = await response.Content.ReadAsStringAsync();

        // 2. IMPRIME O JSON NO CONSOLE (O seu "dotnet watch" do servidor)
        Console.WriteLine("-------------------------------------------");
        Console.WriteLine($"[DEBUG-SICOOB] JSON RECEBIDO DO BANCO:");
        Console.WriteLine(jsonRaw);
        Console.WriteLine("-------------------------------------------");

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"[ERRO SICOOB] Falha na consulta: {response.StatusCode}");
            return null;
        }

        // 3. Converte o JSON para o objeto C# usando as opções de camelCase
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<BoletoResponse>(jsonRaw, options);

        // Minha função que funcionava
        // var numeroCliente = IsSandbox() ? 25546454 : _config.GetValue<long>("SicoobConfig:NumeroCliente");
        // var url = $"{GetBaseUrl()}/boletos?numeroCliente={numeroCliente}&codigoModalidade=1&nossoNumero={nossoNumero}";

        // Console.WriteLine($"Produção Sicoob: Consultando boleto: {nossoNumero}");
        // return await _httpClient.GetFromJsonAsync<BoletoResponse>(url);
    }

    public async Task<bool> BaixarBoletoAsync(long nossoNumero)
    {
        await EnsureTokenAsync();
        SetDefaultHeaders();

        long numeroCliente = IsSandbox() ? 25546454 : _config.GetValue<long>("SicoobConfig:NumeroCliente");
        var body = new { numeroCliente = numeroCliente, codigoModalidade = 1 };

        Console.WriteLine($"Produção Sicoob: Solicitando baixa do boleto: {nossoNumero}");
        var response = await _httpClient.PostAsJsonAsync($"{GetBaseUrl()}/boletos/{nossoNumero}/baixar", body);

        if (response.IsSuccessStatusCode)
        {
            using var _dbContext = await _factory.CreateDbContextAsync();
            var cobranca = await _dbContext.Cobrancas.FirstOrDefaultAsync(c => c.NossoNumero == nossoNumero);
            if (cobranca != null)
            {
                cobranca.Status = "Baixado";
                await _dbContext.SaveChangesAsync();
                Console.WriteLine("Baixa efetuada com sucesso no Sicoob e no Postgres.");
            }
        }
        return response.IsSuccessStatusCode;
    }
}