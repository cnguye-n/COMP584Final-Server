using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using worldmodel;
namespace COMP584Server
{
    public class JwtHandler(UserManager<WorldModelUser> userManager , IConfiguration configuration)
    {
        public async Task<JwtSecurityToken> GenerateTokenAsync(WorldModelUser user)
        {
            return new JwtSecurityToken(
                issuer: configuration["JwtSettings:Issuer"],
                audience: configuration["JwtSettings:Audience"],
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(configuration["JwtSettings:ExpiryInMinutes"])),
                signingCredentials: GetSigningCredentials(),
                claims: await GetClaimAsync(user)
            );
        }
        private SigningCredentials GetSigningCredentials()
        {
            byte[] key = Encoding.UTF8.GetBytes(configuration["JwtSettings:SecretKey"]!);
            SymmetricSecurityKey signingKey = new(key);
            return new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        }
        private async Task<List<Claim>> GetClaimAsync(WorldModelUser user)
        {
            List<Claim> claims = [new Claim(ClaimTypes.Name, user.UserName!)];
            // Claim.AddRange((await userManager.GetRolesAsync(user).Select(RoleManager => new Claim(ClaimTypes.Role, role)));
            foreach (var role in await userManager.GetRolesAsync(user))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            return claims;
        }
    }
}