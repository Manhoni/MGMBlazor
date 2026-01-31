namespace MGMBlazor.Domain.Entities;

public class Servico
{
    public int Id { get; set; }
    public string CodigoMunicipal { get; set; } = default!;
    public string Descricao { get; set; } = default!;
    //public decimal Valor { get; set; }
}
