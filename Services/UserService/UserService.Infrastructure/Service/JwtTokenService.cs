using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Service;

public interface IJwtTokenService
{
	string GenerateToken(ApplicationUser user, IList<string> roles);
	string GenerateRefreshToken();
}

public class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
	private readonly IConfiguration _configuration = configuration;

	public string GenerateToken(ApplicationUser user, IList<string> roles)
	{
		var claims = new List<Claim>
		{
			new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
			new Claim(JwtRegisteredClaimNames.Email, user.Email!),
			new Claim(JwtRegisteredClaimNames.Name, user.FullName! ?? user.Email!),
			new Claim("userId", user.Id.ToString())
		};

		// Add roles as claims
		foreach (var role in roles)
		{
			claims.Add(new Claim(ClaimTypes.Role, role));
		}

		var key = new SymmetricSecurityKey(
			Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!));
		var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var token = new JwtSecurityToken(
			issuer: _configuration["Jwt:Issuer"],
			audience: _configuration["Jwt:Audience"],
			claims: claims,
			expires: DateTime.UtcNow.AddMinutes(60),
			signingCredentials: creds
		);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}

	public string GenerateRefreshToken()
	{
		return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
	}
}