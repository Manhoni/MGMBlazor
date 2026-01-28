using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using MGMBlazor.Infrastructure.NFSe.Configuration;
using Microsoft.Extensions.Options;

namespace MGMBlazor.Infrastructure.NFSe.Soap;

public class FintelSoapClient
{
    private readonly NfseOptions _options;

    public FintelSoapClient(IOptions<NfseOptions> options)
    {
        _options = options.Value;
    }

    // Métodos públicos específicos (Fachadas)
    public async Task<string> EnviarGerarNfseAsync(string xml, X509Certificate2 cert) =>
        await EnviarSoapAsync("GerarNfse", xml, cert);

    public async Task<string> ConsultarNfsePorRpsAsync(string xml, X509Certificate2 cert) =>
        await EnviarSoapAsync("ConsultarNfseRps", xml, cert);

    public async Task<string> CancelarNfseAsync(string xml, X509Certificate2 cert) =>
        await EnviarSoapAsync("CancelarNfse", xml, cert);

    public async Task<string> SubstituirNfseAsync(string xml, X509Certificate2 cert) =>
        await EnviarSoapAsync("SubstituirNfse", xml, cert);

    // O "Motor" Genérico de Envio
    private async Task<string> EnviarSoapAsync(string acao, string xmlInterno, X509Certificate2 certificado)
    {
        string url = _options.Ambiente == "Producao" ? _options.Endpoints.Producao : _options.Endpoints.Homologacao;
        string soapAction = $"{_options.Namespace}#{acao}";

        var soapEnvelope = $@"<?xml version=""1.0"" encoding=""utf-8""?>
            <soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" 
                        xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" 
                        xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" 
                        xmlns:tns=""{_options.Namespace}"">
            <soap:Body>
                <tns:{acao}>
                    <xml><![CDATA[{xmlInterno}]]></xml>
                </tns:{acao}>
            </soap:Body>
            </soap:Envelope>";

        var handler = new HttpClientHandler();
        handler.ClientCertificates.Add(certificado);

        using var client = new HttpClient(handler);
        var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
        content.Headers.Add("SOAPAction", $"\"{soapAction}\"");

        var response = await client.PostAsync(url, content);
        return await response.Content.ReadAsStringAsync();
    }
}