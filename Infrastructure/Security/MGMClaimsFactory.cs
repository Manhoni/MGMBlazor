using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MGMBlazor.Domain.Entities;

namespace MGMBlazor.Infrastructure.Security;

public class MGMClaimsFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
      public MGMClaimsFactory(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IOptions<IdentityOptions> options)
          : base(userManager, roleManager, options) { }

      protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
      {
            var identity = await base.GenerateClaimsAsync(user);
            // Adiciona o NomeCompleto ao Cookie para o Layout ler sem ir ao banco
            identity.AddClaim(new Claim("NomeReal", user.NomeCompleto));
            return identity;
      }
}