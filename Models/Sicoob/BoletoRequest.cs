using System.Text.Json.Serialization;

namespace MGMBlazor.Models.Sicoob;

public class BoletoRequest
{
    [JsonPropertyName("numeroCliente")]
    public long NumeroCliente { get; set; }

    [JsonPropertyName("codigoModalidade")]
    public int CodigoModalidade { get; set; }

    [JsonPropertyName("numeroContaCorrente")]
    public long NumeroContaCorrente { get; set; }

    [JsonPropertyName("codigoEspecieDocumento")]
    public string CodigoEspecieDocumento { get; set; } = string.Empty;

    [JsonPropertyName("dataEmissao")]
    public DateTimeOffset DataEmissao { get; set; }

    [JsonPropertyName("seuNumero")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long SeuNumero { get; set; }

    [JsonPropertyName("identificacaoEmissaoBoleto")]
    public int IdentificacaoEmissaoBoleto { get; set; }

    [JsonPropertyName("identificacaoDistribuicaoBoleto")]
    public int IdentificacaoDistribuicaoBoleto { get; set; }

    [JsonPropertyName("valor")]
    public decimal Valor { get; set; }

    [JsonPropertyName("dataVencimento")]
    public DateTimeOffset DataVencimento { get; set; }

    [JsonPropertyName("tipoDesconto")]
    public int TipoDesconto { get; set; }

    [JsonPropertyName("tipoMulta")]
    public int TipoMulta { get; set; }

    [JsonPropertyName("tipoJurosMora")]
    public int TipoJurosMora { get; set; }

    [JsonPropertyName("numeroParcela")]
    public int NumeroParcela { get; set; }

    [JsonPropertyName("pagador")]
    public PagadorRequest Pagador { get; set; } = new();

    [JsonPropertyName("numeroContratoCobranca")]
    public long NumeroContratoCobranca { get; set; }
}

public class PagadorRequest
{
    [JsonPropertyName("numeroCpfCnpj")]
    public string NumeroCpfCnpj { get; set; } = string.Empty;

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("endereco")]
    public string Endereco { get; set; } = string.Empty;

    [JsonPropertyName("bairro")]
    public string Bairro { get; set; } = string.Empty;

    [JsonPropertyName("cidade")]
    public string Cidade { get; set; } = string.Empty;

    [JsonPropertyName("cep")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long Cep { get; set; }

    [JsonPropertyName("uf")]
    public string Uf { get; set; } = string.Empty;
}