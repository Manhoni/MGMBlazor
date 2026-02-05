using Microsoft.AspNetCore.Identity;
using MGMBlazor.Domain.Entities;

namespace MGMBlazor.Infrastructure.Data;

public static class DbSeeder
{
      public static async Task SeedAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
      {
            //if (userManager.Users.Any()) return; // Banco de dados já foi populado

            // 1. Cria as Roles Oficiais
            string[] roles = { "Admin", "Fiscal", "Funcionario" };
            foreach (var roleName in roles)
            {
                  if (!await roleManager.RoleExistsAsync(roleName))
                        await roleManager.CreateAsync(new IdentityRole(roleName));
            }

            // 2. Cria o Administrador inicial
            var adminEmail = "admin@mgm.com.br";
            var user = await userManager.FindByEmailAsync(adminEmail);

            if (user == null)
            {
                  user = new ApplicationUser
                  {
                        UserName = adminEmail,
                        Email = adminEmail,
                        NomeCompleto = "Administrador MGM",
                        Role = "Admin",
                        EmailConfirmed = true
                  };

                  var result = await userManager.CreateAsync(user, "mgm123");

                  if (result.Succeeded)
                  {
                        // Vincula o usuário à Role Admin na tabela AspNetUserRoles
                        await userManager.AddToRoleAsync(user, "Admin");
                  }
            }
      }
}