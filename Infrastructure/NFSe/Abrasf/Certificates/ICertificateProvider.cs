using System.Security.Cryptography.X509Certificates;

namespace MGMBlazor.Infrastructure.NFSe.Certificates;

public interface ICertificateProvider
{
    X509Certificate2 ObterCertificado();
}
