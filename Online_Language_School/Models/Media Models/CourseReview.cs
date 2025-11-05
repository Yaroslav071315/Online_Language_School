using System.ComponentModel.DataAnnotations;

namespace Online_Language_School.Models
{
    public class CourseReview
    {
        public int Id { get; set; }

        [Required, Range(1, 5)]
        public int Rating { get; set; }

        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Keys
        [Required]
        public string StudentId { get; set; }
        public int CourseId { get; set; }

        // Navigation
        public virtual ApplicationUser Student { get; set; }
        public virtual Course Course { get; set; }
    }
}