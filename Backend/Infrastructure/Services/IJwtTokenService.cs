using fundoo_notes.Domain.Entities;

namespace fundoo_notes.Infrastructure.Services
{
    /// <summary>
    /// Interface for JWT token service
    /// </summary>
    public interface IJwtTokenService
    {
        string GenerateToken(User user);
        bool ValidateToken(string token);
        int? GetUserIdFromToken(string token);
        DateTime GetTokenExpiration(string token);
    }
}
