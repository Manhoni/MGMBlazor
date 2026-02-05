using Microsoft.AspNetCore.Identity;

namespace MGMBlazor.Domain.Entities;

public class ApplicationUser : IdentityUser
{
      // Adicionamos o que a MGM precisa especificamente
      public string NomeCompleto { get; set; } = string.Empty;
      public string Role { get; set; } = "Funcionario";
}