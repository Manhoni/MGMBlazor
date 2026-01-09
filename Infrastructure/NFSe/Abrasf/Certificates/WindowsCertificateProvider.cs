using System.Security.Cryptography.X509Certificates;

namespace MGMBlazor.Infrastructure.NFSe.Certificates;

public class WindowsCertificateProvider : ICertificateProvider
{
    public X509Certificate2 ObterCertificado()
    {
        Console.WriteLine("Iniciando certificado WINDOWS");

        var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadOnly);

        var cert = store.Certificates
            .Find(X509FindType.FindBySubjectName, "MGM", false)
            .FirstOrDefault();

        if (cert == null)
            throw new Exception("Certificado não encontrado.");

        return cert;
    }
}
