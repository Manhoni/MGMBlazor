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

            //Console.WriteLine("\n[DEBUG-XML-SOAP no Parser] " + xmlSoap);

            // Busca o conteúdo de QUALQUER tag que termine em "Result" ou seja "return"
            var resultadoRaw = soapDoc.Descendants().FirstOrDefault(x =>
                x.Name.LocalName.EndsWith("Result") ||
                x.Name.LocalName == "return")?.Value;

            if (string.IsNullOrEmpty(resultadoRaw))
            {
                resposta.Sucesso = false;
                resposta.Erros.Add("O servidor não retornou dados no campo de resultado.");
                return;
            }

            // --- DEBUG ---
            //Console.WriteLine("DEBUG XML INTERNO: " + resultadoRaw);

            var xmlInterno = XDocument.Parse(resultadoRaw);

            // Definimos o Namespace ABRASF para facilitar buscas
            XNamespace nsAbrasf = "http://www.abrasf.org.br/nfse.xsd";

            // 2. BUSCA ERROS (Usando o Namespace e o LocalName juntos para não falhar)
            var mensagens = xmlInterno.Descendants().Where(x => x.Name.LocalName == "MensagemRetorno").ToList();

            if (mensagens.Any())
            {
                resposta.Sucesso = false;
                foreach (var m in mensagens)
                {
                    var codigo = m.Descendants().FirstOrDefault(x => x.Name.LocalName == "Codigo")?.Value;
                    var msg = m.Descendants().FirstOrDefault(x => x.Name.LocalName == "Mensagem")?.Value;
                    resposta.Erros.Add($"{codigo} - {msg}");
                }
                return;
            }

            // 3. BUSCA SUCESSO (ComplNfse)
            // No sucesso, a estrutura é: ListaNfse -> CompNfse -> Nfse -> InfNfse

            // 1. Tenta achar o bloco da nota que SUBSTITUIU a antiga
            var nfseSubstituidora = xmlInterno.Descendants().FirstOrDefault(x => x.Name.LocalName == "NfseSubstituidora");

            // Busca confirmação de cancelamento
            var cancelamento = xmlInterno.Descendants().FirstOrDefault(x => x.Name.LocalName == "RetCancelamento");

            // 2. Define qual nó de informação usar
            XElement? infNfse;

            if (cancelamento != null)
            {
                resposta.Sucesso = true;
                Console.WriteLine("Cancelamento confirmado pela prefeitura.");
                return;
            }

            if (nfseSubstituidora != null)
            {
                // Se for substituição, pegamos a nota nova que está dentro de NfseSubstituidora
                infNfse = nfseSubstituidora.Descendants().FirstOrDefault(x => x.Name.LocalName == "InfNfse");
                Console.WriteLine("Detectada Substituição. Pegando dados da nota NOVA.");
            }
            else
            {
                // Se for emissão normal, pegamos a primeira que aparecer
                infNfse = xmlInterno.Descendants().FirstOrDefault(x => x.Name.LocalName == "InfNfse");
            }

            if (infNfse != null)
            {
                resposta.Sucesso = true;
                resposta.NumeroNota = infNfse.Descendants().FirstOrDefault(x => x.Name.LocalName == "Numero")?.Value;
                resposta.CodigoVerificacao = infNfse.Descendants().FirstOrDefault(x => x.Name.LocalName == "CodigoVerificacao")?.Value;
                resposta.XmlRetorno = xmlInterno.ToString(); // Armazena o XML recuperado

                var temCancelamento = xmlInterno.Descendants().Any(x => x.Name.LocalName == "NfseCancelamento");

                // 2. Verifica o campo Status dentro do RPS
                //var statusPrefeitura = infNfse.Descendants().FirstOrDefault(x => x.Name.LocalName == "Status")?.Value;

                if (temCancelamento /*|| statusPrefeitura == "2"*/)
                {
                    Console.WriteLine("Esta nota consta como CANCELADA na prefeitura.");
                    resposta.StatusRecuperado = StatusNfse.Cancelada;
                }
                else
                {
                    resposta.StatusRecuperado = StatusNfse.Faturada;
                }
            }
            else
            {
                resposta.Sucesso = false;
                resposta.Erros.Add("Nota não encontrada e nenhum erro foi reportado.");
            }
        }
        catch (Exception ex)
        {
            resposta.Sucesso = false;
            resposta.Erros.Add($"Erro crítico no Parser: {ex.Message}");
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

