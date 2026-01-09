using System.Security.Cryptography.X509Certificates;
using MGMBlazor.Infrastructure.NFSe.Configuration;
using System.Text;
using Microsoft.Extensions.Options;

namespace MGMBlazor.Infrastructure.NFSe.Soap;

public class FintelSoapClient
{
    private readonly NfseOptions _options;

    public FintelSoapClient(IOptions<NfseOptions> options)
    {
        _options = options.Value;
    }

    public async Task<string> EnviarGerarNfseAsync(string xmlAssinado, X509Certificate2 certificado)
    {
        // 1. URL baseada no ambiente do JSON
        string url = _options.Ambiente == "Producao" 
            ? _options.Endpoints.Producao 
            : _options.Endpoints.Homologacao;

        // 2. SoapAction fixo para GerarNfse (focando no seu objetivo)
        string soapAction = $"{_options.Namespace}#GerarNfse";

        // 3. Montagem do Envelope SOAP 1.1 exato como o manual pede
        var soapEnvelope = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" 
               xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" 
               xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" 
               xmlns:tns=""{_options.Namespace}"">
  <soap:Body>
    <tns:GerarNfse>
      <xml><![CDATA[{xmlAssinado}]]></xml>
    </tns:GerarNfse>
  </soap:Body>
</soap:Envelope>";

        // 4. Preparação do envio com o Certificado Digital A1
        var handler = new HttpClientHandler();
        handler.ClientCertificates.Add(certificado);

        using var client = new HttpClient(handler);
        var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
        
        // Importante: SOAPAction 1.1 exige aspas duplas dentro da string
        content.Headers.Add("SOAPAction", $"\"{soapAction}\"");

        var response = await client.PostAsync(url, content);
        return await response.Content.ReadAsStringAsync();
    }
}

//     public FintelSoapClient(IOptions<NfseOptions> options)
//     {
//         var cfg = options.Value;

//         bool homologacao = cfg.Ambiente == "Homologacao";

//         _url = homologacao ? cfg.Urls.Homologacao : cfg.Urls.Producao;

//         _soapAction = homologacao
//             ? "https://nfse-ws.hom-ecity.maringa.pr.gov.br/v2.01#GerarNfse"
//             : "https://nfse-ws.ecity.maringa.pr.gov.br/v2.01#GerarNfse";
//     }

//     public async Task<string> EnviarGerarNfseAsync(string xmlAssinado, X509Certificate2 certificado)
//     {
//         var soapEnvelope = $@"<?xml version=""1.0"" encoding=""utf-8""?>
// <soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" 
//                xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" 
//                xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
//   <soap:Body>
//     <GerarNfse xmlns=""https://nfse-ws.ecity.maringa.pr.gov.br/v2.01"">
//       <xml><![CDATA[{xmlAssinado}]]></xml>
//     </GerarNfse>
//   </soap:Body>
// </soap:Envelope>";

//         var handler = new HttpClientHandler();
//         handler.ClientCertificates.Add(certificado);

//         using var client = new HttpClient(handler);

//         var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
//         content.Headers.Add("SOAPAction", _soapAction);

//         var response = await client.PostAsync(_url, content);

//         var responseBody = await response.Content.ReadAsStringAsync();

//         if (!response.IsSuccessStatusCode)
//         {
//             throw new Exception($"Erro SOAP: {response.StatusCode} - {responseBody}");
//         }

//         return responseBody;
//     }
//}
