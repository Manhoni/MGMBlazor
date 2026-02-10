namespace MGMBlazor.Domain.Entities;

public class Cliente
{
    public int Id { get; set; }
    public string RazaoSocial { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Uf { get; set; } = string.Empty; // PR, SP, etc.
    public string Cep { get; set; } = string.Empty;
    public string MunicipioCodigoIbge { get; set; } = "4115200"; // Código IBGE para Maringá, PR
}
