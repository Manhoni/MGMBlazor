using MGMBlazor.Domain.Entities;
using MGMBlazor.Infrastructure.NFSe.Abrasf;
using System.Xml.Linq;
using MGMBlazor.Infrastructure.NFSe.Certificates;
using MGMBlazor.Infrastructure.NFSe.Soap;
using MGMBlazor.Infrastructure.NFSe.Abrasf.Parsing;

namespace MGMBlazor.Services.Nfse;

public class NfseService : INfseService
{
    private readonly AbrasfXmlBuilder _builder;
    private readonly XmlValidator _validator;
    private readonly XmlSigner _signer;
    private readonly ICertificateProvider _certificateProvider;
    private readonly FintelSoapClient _soapClient; // Injetado ou criado aqui
    private readonly string _pastaSchemas;
    private readonly INfseRetornoParser _retornoParser;

    public NfseService(
        AbrasfXmlBuilder builder,
        XmlValidator validator,
        XmlSigner signer,
        ICertificateProvider certificateProvider,
        FintelSoapClient soapClient,
        INfseRetornoParser retornoParser)
    {
        _builder = builder;
        _validator = validator;
        _signer = signer;
        _certificateProvider = certificateProvider;
        _soapClient = soapClient;
        _retornoParser = retornoParser;

        // Define a pasta uma única vez no construtor
        _pastaSchemas = Path.Combine(AppContext.BaseDirectory, "Infrastructure", "NFSe", "Abrasf", "Schemas");
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

            // 5. Envio SOAP (Aqui está a função que estava "faltando")
            // Certifique-se que o nome na classe FintelSoapClient seja igual a este
            string xmlRetorno = await _soapClient.EnviarGerarNfseAsync(xmlAssinado, certificado);
            resposta.XmlRetorno = xmlRetorno;

            // 6. Próximo passo será criar o Parser para ler o xmlRetorno
            _retornoParser.Processar(xmlRetorno, resposta);

            //resposta.Sucesso = true; 
        }
        catch (Exception ex)
        {
            resposta.Sucesso = false;
            resposta.Erros.Add($"Falha na emissão: {ex.Message}");
        }

        return resposta;
    }
}
