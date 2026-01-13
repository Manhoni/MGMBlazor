namespace MGMBlazor.Infrastructure.NFSe.Configuration;

public class NfseOptions
{
    public string Ambiente { get; set; } = "Homologacao";
    public string Namespace { get; set; } = string.Empty;
    public NfseEndpoints Endpoints { get; set; } = new();
    public NfsePrestador Prestador { get; set; } = new(); // Adicionado
}

public class NfsePrestador
{
    public string Cnpj { get; set; } = string.Empty;
    public string InscricaoMunicipal { get; set; } = string.Empty;
}

public class NfseEndpoints
{
    public string Homologacao { get; set; } = string.Empty;
    public string Producao { get; set; } = string.Empty;
}
