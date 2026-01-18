using MGMBlazor.Domain.Entities;
using MGMBlazor.Infrastructure.NFSe.Certificates;
using MGMBlazor.Infrastructure.NFSe.Abrasf;
using MGMBlazor.Infrastructure.NFSe.Abrasf.Parsing;
using MGMBlazor.Infrastructure.NFSe.Soap;
using MGMBlazor.Services.Nfse;
using MGMBlazor.Services.Sicoob;
using MGMBlazor.Infrastructure.NFSe.Configuration;
using System.Net;
using MGMBlazor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using MGMBlazor.Models.Sicoob;

var builder = WebApplication.CreateBuilder(args);

// ---------- CONFIGURAÇÃO DE CERTIFICADO DINÂMICO ----------
// Removemos a duplicação. O .NET decide aqui qual provider usar.
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    builder.Services.AddSingleton<ICertificateProvider, WindowsCertificateProvider>();
}
else
{
    builder.Services.AddSingleton<ICertificateProvider, LinuxCertificateProvider>();
}

// Função para configurar o HttpClient com o Certificado Digital MGM
HttpClientHandler CriarHandlerComCertificado(IServiceProvider sp)
{
    var handler = new HttpClientHandler();
    var certProvider = sp.GetRequiredService<ICertificateProvider>();
    var cert = certProvider.ObterCertificado();
    
    if (cert != null)
    {
        handler.ClientCertificates.Add(cert);
    }
    return handler;
}

// ---------- BANCO DE DADOS E CONFIGS ----------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.Configure<NfseOptions>(
    builder.Configuration.GetSection("Nfse"));

// ---------- INFRAESTRUTURA NFSe ----------
builder.Services.AddScoped<AbrasfXmlBuilder>();
builder.Services.AddScoped<XmlValidator>();
builder.Services.AddScoped<XmlSigner>();
builder.Services.AddScoped<INfseRetornoParser, AbrasfRetornoParser>();
builder.Services.AddScoped<FintelSoapClient>();

// ---------- REGISTRO DOS SERVIÇOS COM HTTPCLIENT "BATIZADO" ----------

// Registra NfseService injetando o HttpClient já com certificado
builder.Services.AddHttpClient<INfseService, NfseService>()
    .ConfigurePrimaryHttpMessageHandler(sp => CriarHandlerComCertificado(sp));

// Prepara o SicoobService (ele usará o mesmo certificado automaticamente)
builder.Services.AddHttpClient<ISicoobService, SicoobService>()
    .ConfigurePrimaryHttpMessageHandler(sp => CriarHandlerComCertificado(sp));


var app = builder.Build();

// ---------- TESTE DE HOMOLOGAÇÃO (MANTIDO) ----------
using (var scope = app.Services.CreateScope())
{
    var nfseService = scope.ServiceProvider.GetRequiredService<INfseService>();
    var sicoobService = scope.ServiceProvider.GetRequiredService<ISicoobService>();

    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    bool isSandbox = config.GetValue<bool>("SicoobConfig:UsarSandbox");

    try 
    {
        var proximoRPS = await nfseService.ObterProximoNumeroRpsAsync();
        Console.WriteLine($"Gerando nota com RPS: {proximoRPS}");

        var notaTeste = new NotaFiscal
        {
            Id = proximoRPS,
            Tomador = new Cliente
            {
                RazaoSocial = "Cliente Teste LTDA",
                Cnpj = "12345678000199",
                Email = "teste@cliente.com",
                MunicipioCodigoIbge = "4115200"
            },
            Servico = new Servico
            {
                CodigoMunicipal = "0701",
                Descricao = "Exame ocupacional admissional",
                Valor = 150.00m
            }
        };

        var respostaNfse = await nfseService.EmitirNotaAsync(notaTeste);

        Console.WriteLine($"Sucesso NFSe: {respostaNfse.Sucesso}");
        Console.WriteLine($"Número NFSe: {respostaNfse.NumeroNota}");
        if (respostaNfse.Erros.Any())
            Console.WriteLine("Erros: " + string.Join(Environment.NewLine, respostaNfse.Erros));

        if (respostaNfse.Sucesso)
        {
            Console.WriteLine("\n--- VERIFICAÇÃO DE SEGURANÇA SICOOB ---");
            
            // Verificamos o que o sistema está lendo do appsettings
            string urlUsada = isSandbox ? config["SicoobConfig:UrlSandbox"]! : config["SicoobConfig:Api-cobranca-bancaria-Url"]!;
            
            if (isSandbox) {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("AMBIENTE: [SANDBOX] - NENHUM BOLETO REAL SERÁ GERADO.");
            } else {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ATENÇÃO: AMBIENTE [PRODUÇÃO]! BOLETO REAL COM CUSTO BANCÁRIO.");
            }
            Console.ResetColor();
            Console.WriteLine($"URL Alvo: {urlUsada}");
            Console.WriteLine("---------------------------------------\n");

            // Se estiver escrito SANDBOX acima, pode descomentar a linha abaixo sem medo:
            
            var boletoReq = new BoletoRequest {
                NumeroCliente = isSandbox ? 25546454 : config.GetValue<long>("SicoobConfig:NumeroCliente"),
                Valor = notaTeste.Servico.Valor,
                SeuNumero = "001JAN", //o usuario coloca o que quiser e pode contar letras
                DataVencimento = DateTime.Now.AddDays(7).ToString("yyyy-MM-ddT00:00:00-03:00"),
                Pagador = new PagadorRequest {
                    Nome = notaTeste.Tomador.RazaoSocial,
                    NumeroCpfCnpj = notaTeste.Tomador.Cnpj,
                    Endereco = "Rua Teste, 123",
                    Bairro = "Centro",
                    Cidade = "Maringa",
                    Cep = "87000000",
                    Uf = "PR"
                }
            };

            // PODE DESCOMENTAR AQUI SE O LOG ACIMA DISSER SANDBOX:
            var respBoleto = await sicoobService.IncluirBoletoAsync(respostaNfse.IdInternoNoBanco, boletoReq);
            
            if (respBoleto?.Resultado != null) {
                Console.WriteLine($"BOLETO SANDBOX GERADO! Nosso Número: {respBoleto.Resultado.NossoNumero}");
                Console.WriteLine($"   Linha Digitável: {respBoleto.Resultado.LinhaDigitavel}");
                Console.WriteLine($"   ID Vinculado no Banco: {respostaNfse.IdInternoNoBanco}");
            }
        }
        else
        {
            Console.WriteLine("Falha na NFSe. O fluxo do boleto foi interrompido para sua segurança.");
            if (respostaNfse.Erros.Any())
                Console.WriteLine("Erros: " + string.Join(" | ", respostaNfse.Erros));
        }

    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro no teste de inicialização: {ex.Message}");
    }
}

app.Run();