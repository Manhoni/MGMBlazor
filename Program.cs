using MGMBlazor.Domain.Entities;
using MGMBlazor.Infrastructure.NFSe.Certificates;
using MGMBlazor.Infrastructure.NFSe.Abrasf;
using MGMBlazor.Infrastructure.NFSe.Abrasf.Parsing;
using MGMBlazor.Infrastructure.NFSe.Soap;
using MGMBlazor.Services.Nfse;
using MGMBlazor.Infrastructure.NFSe.Configuration;
using System.Net;
using MGMBlazor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

/*
 * Sistema de Automação Fiscal MGM
 * Copyright (c) 2026 João Gabriel Manhoni
 * 
 * Este arquivo faz parte de um projeto licenciado sob CC BY-NC-SA 4.0
 * Uso comercial requer permissão. Veja LICENSE para detalhes.
 * 
 * Contato: seuemail@exemplo.com
 */

//ServicePointManager.SecurityProtocol =
//    SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

var builder = WebApplication.CreateBuilder(args);

// ---------- CONFIG ----------
builder.Services.Configure<NfseOptions>(
    builder.Configuration.GetSection("Nfse"));

// 1. Pega a string de conexão do appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Registra o DbContext (É aqui que a ferramenta olha)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

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
