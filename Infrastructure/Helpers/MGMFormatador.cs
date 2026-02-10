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
}