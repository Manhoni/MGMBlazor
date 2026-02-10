using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.DataProtection.KeyManagement.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace MGMBlazor.Infrastructure.NFSe.Certificates;

public class LinuxCertificateProvider : ICertificateProvider
{
    private readonly IConfiguration _config;
    private readonly IHostEnvironment _env;
    private X509Certificate2? _cache;

    public LinuxCertificateProvider(IConfiguration config, IHostEnvironment env)
    {
        _config = config;
        _env = env;
    }

    public X509Certificate2 ObterCertificado()
    {
        if (_cache != null) return _cache;

        Console.WriteLine("Iniciando carregamento de certificado em ambiente LINUX...");

        var certNome = _config["CertificateConfig:NomeArquivo"];
        var certSenha = _config["CertificateConfig:Senha"];

        var caminho = _config["CertificateConfig:LinuxPath"];

        if (!File.Exists(caminho))
        {
            throw new FileNotFoundException($"Certificado não encontrado no caminho Linux: {caminho}");
        }

        _cache = new X509Certificate2(
            caminho,
            certSenha,
            X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet
        );

        return _cache;
    }
}