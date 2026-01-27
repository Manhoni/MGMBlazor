using System.Text.Json.Serialization;

namespace MGMBlazor.Models.Sicoob;

public class BoletoRequest
{
    [JsonPropertyName("numeroCliente")]
    public long NumeroCliente { get; set; }

    [JsonPropertyName("codigoModalidade")]
    public int CodigoModalidade { get; set; } = 1;

    [JsonPropertyName("numeroContaCorrente")]
    public long NumeroContaCorrente { get; set; }

    [JsonPropertyName("codigoEspecieDocumento")]
    public string CodigoEspecieDocumento { get; set; } = "DM";

    [JsonPropertyName("dataEmissao")]
    public string DataEmissao { get; set; } = "2018-09-20"; // DateTime.Now.ToString("yyyy-MM-ddT00:00:00-03:00")

    [JsonPropertyName("seuNumero")]
    public string SeuNumero { get; set; } = string.Empty;

    [JsonPropertyName("identificacaoEmissaoBoleto")]
    public int IdentificacaoEmissaoBoleto { get; set; } = 2; //2

    [JsonPropertyName("identificacaoDistribuicaoBoleto")]
    public int IdentificacaoDistribuicaoBoleto { get; set; } = 2; //2

    [JsonPropertyName("valor")]
    public decimal Valor { get; set; }

    [JsonPropertyName("dataVencimento")]
    public string DataVencimento { get; set; } = string.Empty;

    [JsonPropertyName("tipoDesconto")]
    public int TipoDesconto { get; set; } = 0;

    [JsonPropertyName("tipoMulta")]
    public int TipoMulta { get; set; } = 0;

    [JsonPropertyName("tipoJurosMora")]
    public int TipoJurosMora { get; set; } = 3; //3

    [JsonPropertyName("numeroParcela")]
    public int NumeroParcela { get; set; } = 1;

    [JsonPropertyName("pagador")]
    public PagadorRequest Pagador { get; set; } = new();

    [JsonPropertyName("numeroContratoCobranca")]
    public long NumeroContratoCobranca { get; set; } = 1;

    [JsonPropertyName("gerarPdf")]
    public bool GerarPdf { get; set; } = true;
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
    public string Cep { get; set; } = string.Empty;

    [JsonPropertyName("uf")]
    public string Uf { get; set; } = string.Empty;
}