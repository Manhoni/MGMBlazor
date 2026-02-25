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
    public string DataEmissao { get; set; } = DateTime.Now.ToString("yyyy-MM-ddT00:00:00-03:00");

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
    public int TipoMulta { get; set; } = 2; //2 - percentual, 1 valor fixo

    [JsonPropertyName("dataMulta")]
    public string? DataMulta { get; set; }

    [JsonPropertyName("valorMulta")]
    public decimal ValorMulta { get; set; } = 2.00m;

    [JsonPropertyName("tipoJurosMora")]
    public int TipoJurosMora { get; set; } = 2; //2 taxa mensal, 1 valor por dia

    [JsonPropertyName("dataJurosMora")]
    public string? DataJurosMora { get; set; }

    [JsonPropertyName("valorJurosMora")]
    public decimal ValorJurosMora { get; set; } = 0.90m;

    [JsonPropertyName("numeroParcela")]
    public int NumeroParcela { get; set; } = 1;

    [JsonPropertyName("pagador")]
    public PagadorRequest Pagador { get; set; } = new();

    [JsonPropertyName("beneficiarioFinal")]
    public BeneficiarioRequest Beneficiario { get; set; } = new();

    [JsonPropertyName("mensagensInstrucao")]
    public List<string> MensagensInstrucao { get; set; } = new List<string>
    {
        "Após vencimento multa de 2%",
        "Após vencimento juros de 0,03%/dia",
        "Não conceder desconto."
    };

    [JsonPropertyName("gerarPdf")]
    public bool GerarPdf { get; set; } = true;

    [JsonPropertyName("codigoCadastrarPIX")]
    public int CodigoCadastrarPIX { get; set; } = 1; //2 - não cadastrar, 1 cadastrar

    [JsonPropertyName("numeroContratoCobranca")]
    public int? NumeroContratoCobranca { get; set; }
}

public class BeneficiarioRequest
{
    [JsonPropertyName("numeroCpfCnpj")]
    public string NumeroCpfCnpj { get; set; } = "02152507000196";

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = "MGM - ENGENHARIA DE SEGURANCA E MEDICINA DO TRABALHO LTDA";
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