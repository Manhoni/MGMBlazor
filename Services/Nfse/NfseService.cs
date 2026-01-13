using MGMBlazor.Domain.Entities;
using MGMBlazor.Infrastructure.NFSe.Abrasf;
using System.Xml.Linq;
using MGMBlazor.Infrastructure.NFSe.Certificates;
using MGMBlazor.Infrastructure.NFSe.Soap;
using MGMBlazor.Infrastructure.NFSe.Abrasf.Parsing;
using MGMBlazor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MGMBlazor.Services.Nfse;

public class NfseService : INfseService
{
    private readonly AppDbContext _context;
    private readonly AbrasfXmlBuilder _builder;
    private readonly XmlValidator _validator;
    private readonly XmlSigner _signer;
    private readonly ICertificateProvider _certificateProvider;
    private readonly FintelSoapClient _soapClient; // Injetado ou criado aqui
    private readonly string _pastaSchemas;
    private readonly INfseRetornoParser _retornoParser;

    public NfseService(
        AppDbContext context,
        AbrasfXmlBuilder builder,
        XmlValidator validator,
        XmlSigner signer,
        ICertificateProvider certificateProvider,
        FintelSoapClient soapClient,
        INfseRetornoParser retornoParser)
    {
        _context = context;
        _builder = builder;
        _validator = validator;
        _signer = signer;
        _certificateProvider = certificateProvider;
        _soapClient = soapClient;
        _retornoParser = retornoParser;

        // Define a pasta uma única vez no construtor
        _pastaSchemas = Path.Combine(AppContext.BaseDirectory, "Infrastructure", "NFSe", "Abrasf", "Schemas");
    }

    public async Task<int> ObterProximoNumeroRpsAsync()
    {
        // Busca o maior VendaId já salvo no banco. Se não houver nenhum, começa do 1 (ou do número que você definir)
        var ultimoRPS = await _context.NotasFiscaisEmitidas
            .MaxAsync(n => (int?)n.VendaId) ?? 4; // Se quiser começar de um número específico, mude o 0 para 100, por exemplo.

        return ultimoRPS + 1;
    }

    public async Task<RespostaEmissao> EmitirNotaAsync(NotaFiscal nota)
    {
        var resposta = new RespostaEmissao();

        try
        {
            // 1. Geração do XML
            var xmlDoc = _builder.GerarXml(nota);

            // 2. Validação XSD (Usando a variável do construtor)
            _validator.Validar(xmlDoc, _pastaSchemas);

            // 3. Certificado
            var certificado = _certificateProvider.ObterCertificado();

            // 4. Assinatura (Retorna String)
            string xmlAssinado = _signer.AssinarLoteRps(xmlDoc, certificado);
            resposta.XmlEnviado = xmlAssinado;

            // GRAVA O ARQUIVO PARA COPIAR PARA O POSTMAN
            File.WriteAllText("nota_assinada.xml", xmlAssinado);

            // Limpa a declaração XML interna que Maringá não gosta
            string xmlSemDeclaracao = xmlAssinado.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", "");

            // Escapa o XML programaticamente (transforma < em &lt;)
            string xmlParaPostman = System.Security.SecurityElement.Escape(xmlSemDeclaracao);

            File.WriteAllText("CONTEUDO_PARA_POSTMAN.txt", xmlParaPostman);

            string xmlRetorno = await _soapClient.EnviarGerarNfseAsync(xmlAssinado, certificado);
            resposta.XmlRetorno = xmlRetorno;

            _retornoParser.Processar(xmlRetorno, resposta);

            if (resposta.Sucesso)
            {
                await SalvarNoBanco(nota, resposta);
            }
        }
        catch (Exception ex)
        {
            resposta.Sucesso = false;
            resposta.Erros.Add($"Falha na emissão: {ex.Message}");
        }

        return resposta;
    }

    private async Task SalvarNoBanco(NotaFiscal nota, RespostaEmissao resposta)
{
    var notaEmitida = new NotaFiscalEmitida
    {
        VendaId = nota.Id, // Grava o número do RPS que acabamos de usar
        DataEmissao = DateTime.UtcNow,
        NumeroNota = resposta.NumeroNota,
        CodigoVerificacao = resposta.CodigoVerificacao,
        XmlRetorno = resposta.XmlRetorno,
        Status = StatusNfse.Faturada
    };

    _context.NotasFiscaisEmitidas.Add(notaEmitida);
    await _context.SaveChangesAsync();
    Console.WriteLine($"[BANCO] Nota {resposta.NumeroNota} salva com sucesso!");
}
}
