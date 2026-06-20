using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PulseGuard.Api.Configuration;
using PulseGuard.Api.Data;
using PulseGuard.Api.DTOs;
using PulseGuard.Api.Models;

namespace PulseGuard.Api.Services;

public sealed class AuthService(
    AppDbContext dbContext,
    IPasswordHasher<User> passwordHasher,
    IOptions<JwtSettings> jwtOptions)
{
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;

    public AuthResponse? Register(RegisterRequest request)
    {
        var email = NormalizeEmail(request.Email);
        if (dbContext.Users.Any(user => user.Email == email))
        {
            return null;
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            CreatedAtUtc = DateTime.UtcNow
        };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        dbContext.Users.Add(user);
        dbContext.SaveChanges();

        return CreateAuthResponse(user);
    }

    public AuthResponse? Login(LoginRequest request)
    {
        var email = NormalizeEmail(request.Email);
        var user = dbContext.Users.SingleOrDefault(candidate => candidate.Email == email);
        if (user is null)
        {
            return null;
        }

        var verificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return null;
        }

        return CreateAuthResponse(user);
    }

    private AuthResponse CreateAuthResponse(User user)
    {
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresInMinutes);
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key)),
            SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Email, user.Email)
        };
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAtUtc,
            signingCredentials: signingCredentials);

        return new AuthResponse(
            user.Id,
            user.Email,
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAtUtc);
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
