using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fundoo_notes.Domain.Entities
{
    /// <summary>
    /// Login history entity for tracking user login activities
    /// </summary>
    public class LoginHistory : BaseEntity
    {
        [Required]
        public int UserId { get; set; }
        
        [Required]
        [StringLength(45)]
        public string IpAddress { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string UserAgent { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string Location { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string Device { get; set; } = string.Empty;
        
        public bool IsSuccessful { get; set; }
        
        [StringLength(200)]
        public string? FailureReason { get; set; }
        
        public DateTime LoginTime { get; set; }
        
        public DateTime? LogoutTime { get; set; }
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
