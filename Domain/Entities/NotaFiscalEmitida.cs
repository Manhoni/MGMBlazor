namespace MGMBlazor.Domain.Entities;

public enum StatusNfse { Pendente, Faturada, Erro, Cancelada }

public class NotaFiscalEmitida
{
    public int Id { get; set; }
    public int VendaId { get; set; } // Link com sua tabela de vendas
    public DateTime DataEmissao { get; set; }
    
    // Dados que o seu Parser vai preencher:
    public string? NumeroNota { get; set; }
    public string? CodigoVerificacao { get; set; }
    
    // Onde guardaremos o documento legal
    public string XmlRetorno { get; set; } = string.Empty;
    
    public StatusNfse Status { get; set; } = StatusNfse.Pendente;
}