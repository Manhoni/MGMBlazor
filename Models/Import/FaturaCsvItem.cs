using CsvHelper.Configuration.Attributes;

namespace MGMBlazor.Models.Import;

public class FaturaCsvItem
{
    [Name("Cliente")]
    public string Cliente { get; set; } = string.Empty;

    [Name("Documento")]
    public string Documento { get; set; } = string.Empty; // CNPJ

    [Name("Produtos/Serviços")]
    public string TipoServico { get; set; } = string.Empty;

    [Name("Descrição")]
    public string Descricao { get; set; } = string.Empty;

    [Name("Funcionário")]
    public string Funcionario { get; set; } = string.Empty;

    [Name("Exame")]
    public string Exame { get; set; } = string.Empty;

    [Name("Valor")]
    public decimal Valor { get; set; }
}