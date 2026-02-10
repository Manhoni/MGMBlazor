using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;

namespace MGMBlazor.Infrastructure.NFSe.Certificates;

public class WindowsCertificateProvider : ICertificateProvider
{
    private readonly IConfiguration _config;

    public WindowsCertificateProvider(IConfiguration config)
    {
        _config = config;
    }

    public X509Certificate2 ObterCertificado()
    {
        Console.WriteLine("Iniciando busca de certificado no repositório WINDOWS...");

        var subjectName = _config["CertificateConfig:SubjectName"];

        using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadOnly);

        var cert = store.Certificates
            .Find(X509FindType.FindBySubjectName, subjectName!, false)
            .FirstOrDefault();

        if (cert == null)
        {
            throw new Exception($"Certificado com o nome '{subjectName}' não encontrado no repositório pessoal do Windows.");
        }

        return cert;
    }
}