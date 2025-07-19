using System.ComponentModel.DataAnnotations;

namespace fundoo_notes.Domain.Entities
{
    /// <summary>
    /// User entity representing application users
    /// </summary>
    public class User : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(500)]
        public string? Bio { get; set; }

        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        public bool IsEmailVerified { get; set; } = false;
        
        public string? EmailVerificationToken { get; set; }
        
        public DateTime? EmailVerificationTokenExpiry { get; set; }
        
        public string? PasswordResetToken { get; set; }
        
        public DateTime? PasswordResetTokenExpiry { get; set; }
        
        public DateTime? LastLoginAt { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual ICollection<Note> Notes { get; set; } = new List<Note>();
        
        public virtual ICollection<Label> Labels { get; set; } = new List<Label>();
        
        // Computed property
        public string FullName => $"{FirstName} {LastName}";
    }
}
