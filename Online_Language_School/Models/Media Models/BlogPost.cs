using System.ComponentModel.DataAnnotations;

namespace Online_Language_School.Models
{
    public class BlogPost
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public string? ImageUrl { get; set; }
        public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
        public bool IsPublished { get; set; } = true;

        // Foreign Key 
        [Required]
        public string AdminId { get; set; }

        // Navigation 
        public virtual ApplicationUser Admin { get; set; }
    }
}