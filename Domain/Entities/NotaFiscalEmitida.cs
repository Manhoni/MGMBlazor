namespace MGMBlazor.Domain.Entities;

public enum StatusNfse { Pendente, Faturada, Cancelada, Erro }

public class NotaFiscalEmitida
{
    public int Id { get; set; }
    public int RpsNumero { get; set; }
    public int VendaId { get; set; } // Link com sua tabela de vendas
    public DateTime DataEmissao { get; set; }

    public decimal Valor { get; set; }
    public string CnpjTomador { get; set; } = string.Empty;

    // Dados que o seu Parser vai preencher:
    public string? NumeroNota { get; set; }
    public string? CodigoVerificacao { get; set; }

    // Onde guardaremos o documento legal
    public string XmlRetorno { get; set; } = string.Empty;
    public string? LinkPdf { get; set; }

    public StatusNfse Status { get; set; } = StatusNfse.Pendente;
}