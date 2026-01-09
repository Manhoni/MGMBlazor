namespace MGMBlazor.Domain.Entities;

public class Cliente
{
    public string RazaoSocial { get; set; } = default!;
    public string Cnpj { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string MunicipioCodigoIbge { get; set; } = default!;
}
