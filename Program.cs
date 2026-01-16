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
builder.Services.AddHttpClient<SicoobService>()
    .ConfigurePrimaryHttpMessageHandler(sp => CriarHandlerComCertificado(sp));


var app = builder.Build();

// ---------- TESTE DE HOMOLOGAÇÃO (MANTIDO) ----------
using (var scope = app.Services.CreateScope())
{
    var nfseService = scope.ServiceProvider.GetRequiredService<INfseService>();

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

        var resposta = await nfseService.EmitirNotaAsync(notaTeste);

        Console.WriteLine($"Sucesso NFSe: {resposta.Sucesso}");
        Console.WriteLine($"Número NFSe: {resposta.NumeroNota}");
        if (resposta.Erros.Any())
            Console.WriteLine("Erros: " + string.Join(Environment.NewLine, resposta.Erros));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro no teste de inicialização: {ex.Message}");
    }
}

app.Run();