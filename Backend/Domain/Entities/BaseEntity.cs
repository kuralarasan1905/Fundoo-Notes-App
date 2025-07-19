using System.ComponentModel.DataAnnotations;

namespace fundoo_notes.Domain.Entities
{
    /// <summary>
    /// Base entity class that provides common properties for all entities
    /// </summary>
    public abstract class BaseEntity
    {
        [Key]
        public int Id { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsDeleted { get; set; } = false;
        
        public DateTime? DeletedAt { get; set; }
    }
}
