namespace MGMBlazor.Domain.Entities;

public class Cliente
{
    public int Id { get; set; } 
    public string RazaoSocial { get; set; } = default!;
    public string Cnpj { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string MunicipioCodigoIbge { get; set; } = default!;
}
