using System.Globalization;

namespace MGMBlazor.Infrastructure.Helpers;

public static class MGMFormatador
{
      public static string FormatarMoeda(decimal valor)
      {
            return valor.ToString("C2", new CultureInfo("pt-BR"));
      }

      // Formata CNPJ: 00.000.000/0000-00
      public static string FormatarCnpj(string? cnpj)
      {
            if (string.IsNullOrWhiteSpace(cnpj)) return "";
            var n = new string(cnpj.Where(char.IsDigit).ToArray());
            if (n.Length != 14) return cnpj;
            return string.Format("{0:00\\.000\\.000/0000-00}", ulong.Parse(n));
      }

      // Formata CEP: 00000-000
      public static string FormatarCep(string? cep)
      {
            if (string.IsNullOrWhiteSpace(cep)) return "";
            var n = new string(cep.Where(char.IsDigit).ToArray());
            if (n.Length != 8) return n;
            return n.Insert(5, "-");
      }

      // Ajusta a hora para o fuso de Brasília (UTC-3)
      public static string ParaHoraLocal(DateTime data)
      {
            // Como o Postgres salva em UTC, convertemos para Local ao exibir
            return data.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
      }

      public static decimal ConverterParaDecimal(string valor)
      {
            if (string.IsNullOrWhiteSpace(valor)) return 0;
            // Remove R$, espaços e pontos de milhar, troca vírgula por ponto
            string limpo = valor.Replace("R$", "").Replace(".", "").Replace(" ", "").Trim();
            if (decimal.TryParse(limpo, out decimal resultado)) return resultado;
            return 0;
      }

      // Converte decimal 2500.00 para "2.500,00" (sem o R$, pois o input-group já tem)
      public static string FormatarMoedaParaInput(decimal valor)
      {
            return valor.ToString("N2", new CultureInfo("pt-BR"));
      }
}