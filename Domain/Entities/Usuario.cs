using System.ComponentModel.DataAnnotations;

namespace MGMBlazor.Domain.Entities;

public class Usuario
{
      public int Id { get; set; }

      [Required]
      public string Nome { get; set; } = string.Empty;

      [Required]
      public string Login { get; set; } = string.Empty;

      [Required]
      public string SenhaHash { get; set; } = string.Empty; // Nunca salve senha pura!

      public bool IsAdmin { get; set; } = false;
      public string Role { get; set; } = "Funcionario"; // Admin, Fiscal, Funcionario
}