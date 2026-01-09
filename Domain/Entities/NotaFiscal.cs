namespace MGMBlazor.Domain.Entities;

public class NotaFiscal
{
    public Cliente Tomador { get; set; } = default!;
    public Servico Servico { get; set; } = default!;
    public DateTime DataEmissao { get; set; } = DateTime.Now;
}
