namespace MGMBlazor.Domain.Entities;

public class NotaFiscal
{
    public int Id { get; set; }
    public Cliente Tomador { get; set; } = default!;
    public Servico Servico { get; set; } = default!;
    public decimal Valor { get; set; }
    public DateTime DataEmissao { get; set; } = DateTime.UtcNow.AddHours(-3); // manobra para não dar problema de data no servidor que é UTC.
}
