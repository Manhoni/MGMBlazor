namespace MGMBlazor.Infrastructure.Security;

public class UserSession
{
      public bool IsLoggedIn { get; private set; }
      public string? NomeUsuario { get; private set; }
      public bool IsAdmin { get; private set; }
      public string? Role { get; private set; }

      public void Login(string nome, bool isAdmin, string role)
      {
            IsLoggedIn = true;
            NomeUsuario = nome;
            IsAdmin = isAdmin;
            Role = role;
      }

      public void Logout()
      {
            IsLoggedIn = false;
            NomeUsuario = null;
            Role = "Funcionario";
            IsAdmin = false;
      }
}