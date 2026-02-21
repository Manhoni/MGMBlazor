using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace MGMBlazor.Domain.Entities;

[Index(nameof(Cnpj), IsUnique = true)]
public class Cliente
{
    public int Id { get; set; }

    [Required(ErrorMessage = "A Razão Social é obrigatória.")]
    public string RazaoSocial { get; set; } = string.Empty;

    [Required(ErrorMessage = "O CNPJ é obrigatório.")]
    public string Cnpj { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    public string Email { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Uf { get; set; } = string.Empty;

    [Required(ErrorMessage = "O CEP é obrigatório.")]
    public string Cep { get; set; } = string.Empty;
    public string MunicipioCodigoIbge { get; set; } = string.Empty; // 4115200 Código IBGE para Maringá, PR
}
