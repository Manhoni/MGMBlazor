using System.Text.Json.Serialization;

namespace MGMBlazor.Models.Sicoob;

public class BoletoResponse
{
    [JsonPropertyName("resultado")]
    public Resultado? Resultado { get; set; }
}

public class Resultado
{
    [JsonPropertyName("numeroCliente")]
    public long NumeroCliente { get; set; }

    [JsonPropertyName("codigoModalidade")]
    public int CodigoModalidade { get; set; }

    [JsonPropertyName("numeroContaCorrente")]
    public long NumeroContaCorrente { get; set; }

    [JsonPropertyName("codigoEspecieDocumento")]
    public string? CodigoEspecieDocumento { get; set; }

    [JsonPropertyName("dataEmissao")]
    public string? DataEmissao { get; set; }

    [JsonPropertyName("nossoNumero")]
    public long NossoNumero { get; set; }

    [JsonPropertyName("seuNumero")]
    public string? SeuNumero { get; set; }

    [JsonPropertyName("codigoBarras")]
    public string? CodigoBarras { get; set; }

    [JsonPropertyName("linhaDigitavel")]
    public string? LinhaDigitavel { get; set; }

    [JsonPropertyName("valor")]
    public decimal Valor { get; set; }

    [JsonPropertyName("dataVencimento")]
    public string? DataVencimento { get; set; }

    [JsonPropertyName("pagador")]
    public PagadorResponse? Pagador { get; set; }

    [JsonPropertyName("mensagensInstrucao")]
    public List<string>? MensagensInstrucao { get; set; }

    [JsonPropertyName("pdfBoleto")]
    public string? PdfBoleto { get; set; }

    [JsonPropertyName("qrCode")]
    public string? QrCode { get; set; }

    [JsonPropertyName("situacaoBoleto")]
    public string? SituacaoBoleto { get; set; }

    // Campos extras que o seu JSON gigante trouxe:
    [JsonPropertyName("identificacaoBoletoEmpresa")]
    public string? IdentificacaoBoletoEmpresa { get; set; }

    [JsonPropertyName("numeroContratoCobranca")]
    public long NumeroContratoCobranca { get; set; }
}

public class PagadorResponse
{
    [JsonPropertyName("numeroCpfCnpj")]
    public string? NumeroCpfCnpj { get; set; }

    [JsonPropertyName("nome")]
    public string? Nome { get; set; }

    [JsonPropertyName("endereco")]
    public string? Endereco { get; set; }
}