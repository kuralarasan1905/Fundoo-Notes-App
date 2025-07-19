using System.ComponentModel.DataAnnotations;

namespace fundoo_notes.Application.DTOs
{
    /// <summary>
    /// Data Transfer Objects for Label operations
    /// </summary>
    
    public class CreateLabelDto
    {
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        [StringLength(7)]
        public string? Color { get; set; }
    }

    public class UpdateLabelDto
    {
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        [StringLength(7)]
        public string? Color { get; set; }
    }

    public class LabelDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Color { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int NotesCount { get; set; }
    }
}
