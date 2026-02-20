namespace MGMBlazor.Domain.Entities;

public class LogAuditoria
{
      public int Id { get; set; }
      public DateTime DataHora { get; set; } = DateTime.UtcNow;
      public string Usuario { get; set; } = string.Empty; // E-mail de quem fez
      public string Operacao { get; set; } = string.Empty; // Ex: "Emissão de Nota", "Baixa de Boleto"
      public string Detalhes { get; set; } = string.Empty; // Ex: "Nota: 1942 | Valor: 2.77"
      public string Tela { get; set; } = string.Empty; // Ex: "Faturamento", "Boleto Avulso"
}