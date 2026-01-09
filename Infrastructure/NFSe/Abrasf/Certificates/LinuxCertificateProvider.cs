using System.Security.Cryptography.X509Certificates;

namespace MGMBlazor.Infrastructure.NFSe.Certificates;

public class LinuxCertificateProvider : ICertificateProvider
{
    public X509Certificate2 ObterCertificado()
    {
        Console.WriteLine("Iniciando certificado LINUX");
        
        return new X509Certificate2(
            "/home/manhones/Documentos/MGM/certificado.pfx",
            "senha",
            X509KeyStorageFlags.Exportable
        );
    }
}
