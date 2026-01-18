using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using MGMBlazor.Models.Import;

namespace MGMBlazor.Services.Import;

public class FaturaImportService
{
    public FaturaResumo ProcessarCsv(Stream fileStream)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ",",
        };

        using var reader = new StreamReader(fileStream);
        using var csv = new CsvReader(reader, config);
        
        var registros = csv.GetRecords<FaturaCsvItem>().ToList();

        // 1. Pegamos os dados do cliente (da primeira linha que tiver documento)
        var primeiraLinhaValida = registros.FirstOrDefault(r => r.Documento != "-");
        
        if (primeiraLinhaValida == null) throw new Exception("CSV Inválido");

        // 2. Limpamos o CNPJ para o formato que a NFSe e o Sicoob gostam (só números)
        string cnpjLimpo = new string(primeiraLinhaValida.Documento.Where(char.IsDigit).ToArray());

        // 3. Pegamos o valor Total (geralmente está na última linha do seu CSV)
        decimal valorTotal = registros.FirstOrDefault(r => r.Exame == "Total")?.Valor 
                            ?? registros.Where(r => r.Exame != "Total").Sum(r => r.Valor);

        return new FaturaResumo
        {
            NomeCliente = primeiraLinhaValida.Cliente,
            CnpjCpf = cnpjLimpo,
            ValorTotal = valorTotal,
            QuantidadeItens = registros.Count - 2 // Descontando as linhas de Total/Desconto
        };
    }
}

public class FaturaResumo
{
    public string NomeCliente { get; set; } = string.Empty;
    public string CnpjCpf { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }
    public int QuantidadeItens { get; set; }
}