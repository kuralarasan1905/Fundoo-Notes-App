using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace fundoo_notes.Infrastructure.Services
{
    /// <summary>
    /// Password service implementation
    /// </summary>
    public class PasswordService : IPasswordService
    {
        private readonly ILogger<PasswordService> _logger;

        public PasswordService(ILogger<PasswordService> logger)
        {
            _logger = logger;
        }
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt());
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
                {
                    _logger.LogWarning("Password verification failed: null or empty input");
                    return false;
                }

                var result = BCrypt.Net.BCrypt.Verify(password, hashedPassword);
                _logger.LogDebug("Password verification result: {Result} for hash starting with: {HashPrefix}",
                    result, hashedPassword.Length > 10 ? hashedPassword.Substring(0, 10) : hashedPassword);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password verification");
                return false;
            }
        }

        public string GenerateRandomToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        public bool IsPasswordStrong(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                return false;

            // Check for at least one uppercase letter
            if (!Regex.IsMatch(password, @"[A-Z]"))
                return false;

            // Check for at least one lowercase letter
            if (!Regex.IsMatch(password, @"[a-z]"))
                return false;

            // Check for at least one digit
            if (!Regex.IsMatch(password, @"\d"))
                return false;

            // Check for at least one special character
            if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]"))
                return false;

            return true;
        }
    }
}
