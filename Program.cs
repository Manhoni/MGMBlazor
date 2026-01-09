using MGMBlazor.Domain.Entities;
using MGMBlazor.Infrastructure.NFSe.Certificates;
using MGMBlazor.Infrastructure.NFSe.Abrasf;
using MGMBlazor.Infrastructure.NFSe.Abrasf.Parsing;
using MGMBlazor.Infrastructure.NFSe.Soap;
using MGMBlazor.Services.Nfse;
using MGMBlazor.Infrastructure.NFSe.Configuration;
using System.Net;

//ServicePointManager.SecurityProtocol =
//    SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

var builder = WebApplication.CreateBuilder(args);

// ---------- CONFIG ----------
builder.Services.Configure<NfseOptions>(
    builder.Configuration.GetSection("Nfse"));

// ---------- NFSe ----------
builder.Services.AddScoped<AbrasfXmlBuilder>();
builder.Services.AddScoped<XmlValidator>();
builder.Services.AddScoped<XmlSigner>();
builder.Services.AddScoped<ICertificateProvider, LinuxCertificateProvider>();
builder.Services.AddScoped<INfseRetornoParser, AbrasfRetornoParser>();
builder.Services.AddScoped<FintelSoapClient>();
builder.Services.AddScoped<INfseService, NfseService>();

var app = builder.Build();

// ---------- TESTE ----------
using (var scope = app.Services.CreateScope())
{
    var nfseService = scope.ServiceProvider.GetRequiredService<INfseService>();

    var notaTeste = new NotaFiscal
    {
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

    Console.WriteLine($"Sucesso: {resposta.Sucesso}");
    Console.WriteLine($"Número NFSe: {resposta.NumeroNota}");
    Console.WriteLine(string.Join(Environment.NewLine, resposta.Erros));
}

app.Run();
