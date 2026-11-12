using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Data;
using AuthService.DTOs;
using AuthService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Services;

public class AuthService : IAuthService
{
    private readonly AuthDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(
        AuthDbContext context,
        IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<LoginResponse> RegisterAsync(
        RegisterRequest request)
    {
        var existingUser =
            await _context.Users
                .FirstOrDefaultAsync(
                    u => u.Email == request.Email);

        if (existingUser != null)
        {
            throw new ArgumentException(
                "Email already registered.");
        }

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                request.Password),
            Role = "Customer"
        };

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return GenerateLoginResponse(user);
    }

    public async Task<LoginResponse> LoginAsync(
        LoginRequest request)
    {
        var user =
            await _context.Users
                .FirstOrDefaultAsync(
                    u => u.Email == request.Email);

        if (user == null)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        var passwordValid =
            BCrypt.Net.BCrypt.Verify(
                request.Password,
                user.PasswordHash);

        if (!passwordValid)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        return GenerateLoginResponse(user);
    }

    private LoginResponse GenerateLoginResponse(
        User user)
    {
        var jwtSettings =
            _configuration
                .GetSection("Jwt")
                .Get<JwtSettings>()!;

        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new Claim(
                ClaimTypes.Name,
                user.Name),

            new Claim(
                ClaimTypes.Email,
                user.Email),

            new Claim(
                ClaimTypes.Role,
                user.Role)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.Key));

        var credentials =
            new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                jwtSettings.ExpiryMinutes),
            signingCredentials: credentials);

        return new LoginResponse
        {
            Token =
                new JwtSecurityTokenHandler()
                    .WriteToken(token),

            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role
        };
    }
}