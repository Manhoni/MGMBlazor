namespace MGMBlazor.Infrastructure.NFSe.Configuration;

public class NfseOptions
{
    public string Ambiente { get; set; } = "Homologacao";
    public string Namespace { get; set; } = string.Empty;
    public NfseEndpoints Endpoints { get; set; } = new();
}

public class NfseEndpoints
{
    public string Homologacao { get; set; } = string.Empty;
    public string Producao { get; set; } = string.Empty;
}
