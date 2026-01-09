using System.Xml.Linq;
using MGMBlazor.Domain.Entities;
using MGMBlazor.Infrastructure.NFSe.Configuration;
using Microsoft.Extensions.Options;

namespace MGMBlazor.Infrastructure.NFSe.Abrasf.Parsing;

public class AbrasfRetornoParser : INfseRetornoParser
{
    private readonly NfseOptions _options;

    public AbrasfRetornoParser(IOptions<NfseOptions> options)
    {
        _options = options.Value;
    }

    public void Processar(string xmlSoap, RespostaEmissao resposta)
    {
        try
        {
            var soapDoc = XDocument.Parse(xmlSoap);
            XNamespace fintelNs = _options.Namespace; // Puxa do appsettings.json
            XNamespace abrasfNs = "http://www.abrasf.org.br/nfse.xsd";

            // 1. Extrai o conteúdo da tag de resultado
            var resultadoRaw = soapDoc.Descendants(fintelNs + "GerarNfseResult").FirstOrDefault()?.Value;

            if (string.IsNullOrEmpty(resultadoRaw))
            {
                resposta.Sucesso = false;
                resposta.Erros.Add("Resposta da prefeitura veio vazia.");
                return;
            }

            var xmlInterno = XDocument.Parse(resultadoRaw);

            // 2. Verifica se há erros
            var mensagens = xmlInterno.Descendants(abrasfNs + "MensagemRetorno");
            if (mensagens.Any())
            {
                resposta.Sucesso = false;
                foreach (var m in mensagens)
                {
                    resposta.Erros.Add($"{m.Element(abrasfNs + "Codigo")?.Value} - {m.Element(abrasfNs + "Mensagem")?.Value}");
                }
                return;
            }

            // 3. Se deu certo, pega os dados
            var nfse = xmlInterno.Descendants(abrasfNs + "ComplNfse").FirstOrDefault();
            if (nfse != null)
            {
                resposta.Sucesso = true;
                resposta.NumeroNota = nfse.Descendants(abrasfNs + "Numero").FirstOrDefault()?.Value;
                resposta.CodigoVerificacao = nfse.Descendants(abrasfNs + "CodigoVerificacao").FirstOrDefault()?.Value;
            }
        }
        catch (Exception ex)
        {
            resposta.Sucesso = false;
            resposta.Erros.Add($"Erro no processamento: {ex.Message}");
        }
    }
}
    // public void Processar(string xmlSoap, RespostaEmissao resposta)
    // {
    //     try
    //     {
    //         var soapDoc = XDocument.Parse(xmlSoap);

    //         XNamespace soapNs = "http://schemas.xmlsoap.org/soap/envelope/";
    //         XNamespace fintelNs = "https://nfse-ws.ecity.maringa.pr.gov.br/v2.01";
    //         XNamespace abrasfNs = "http://www.abrasf.org.br/nfse.xsd";

    //         // 1. SOAP Fault
    //         var fault = soapDoc.Descendants(soapNs + "Fault").FirstOrDefault();
    //         if (fault != null)
    //         {
    //             resposta.Sucesso = false;
    //             resposta.Erros.Add("SOAP Fault: " + fault.Value);
    //             return;
    //         }

    //         // 2. Extrair XML interno do GerarNfseResult
    //         var resultadoRaw = soapDoc
    //             .Descendants(fintelNs + "GerarNfseResult")
    //             .FirstOrDefault()
    //             ?.Value;

    //         if (string.IsNullOrWhiteSpace(resultadoRaw))
    //         {
    //             resposta.Sucesso = false;
    //             resposta.Erros.Add("Retorno vazio do WebService.");
    //             return;
    //         }

    //         var xmlInterno = XDocument.Parse(resultadoRaw);

    //         // 3. Mensagens de erro ABRASF
    //         var mensagensErro = xmlInterno.Descendants(abrasfNs + "MensagemRetorno");
    //         if (mensagensErro.Any())
    //         {
    //             resposta.Sucesso = false;

    //             foreach (var erro in mensagensErro)
    //             {
    //                 var codigo = erro.Element(abrasfNs + "Codigo")?.Value;
    //                 var mensagem = erro.Element(abrasfNs + "Mensagem")?.Value;
    //                 var correcao = erro.Element(abrasfNs + "Correcao")?.Value;

    //                 resposta.Erros.Add($"{codigo} - {mensagem} {(string.IsNullOrEmpty(correcao) ? "" : $"(Correção: {correcao})")}");
    //             }

    //             return;
    //         }

    //         // 4. Dados da NFSe emitida
    //         var infNfse = xmlInterno
    //             .Descendants(abrasfNs + "InfNfse")
    //             .FirstOrDefault();

    //         if (infNfse == null)
    //         {
    //             resposta.Sucesso = false;
    //             resposta.Erros.Add("NFSe não encontrada no retorno.");
    //             return;
    //         }

    //         resposta.NumeroNota = infNfse.Element(abrasfNs + "Numero")?.Value;
    //         resposta.CodigoVerificacao = infNfse.Element(abrasfNs + "CodigoVerificacao")?.Value;
    //         resposta.Sucesso = true;
    //     }
    //     catch (Exception ex)
    //     {
    //         resposta.Sucesso = false;
    //         resposta.Erros.Add($"Erro ao processar retorno NFSe: {ex.Message}");
    //     }
    // }

