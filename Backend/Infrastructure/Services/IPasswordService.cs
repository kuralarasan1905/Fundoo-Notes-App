namespace fundoo_notes.Infrastructure.Services
{
    /// <summary>
    /// Interface for password service
    /// </summary>
    public interface IPasswordService
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
        string GenerateRandomToken();
        bool IsPasswordStrong(string password);
    }
}
