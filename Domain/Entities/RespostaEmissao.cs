namespace MGMBlazor.Domain.Entities;

public class RespostaEmissao
{
    public bool Sucesso { get; set; }
    public string XmlEnviado { get; set; } = string.Empty;
    public string XmlRetorno { get; set; } = string.Empty;
    public string? NumeroNota { get; set; }
    public string? CnpjTomadorRecuperado { get; set; }
    public decimal ValorRecuperado { get; set; }
    public string? CodigoVerificacao { get; set; }
    public List<string> Erros { get; set; } = new();
    public int IdInternoNoBanco { get; set; }
    public StatusNfse StatusRecuperado { get; set; } = StatusNfse.Pendente;
}
