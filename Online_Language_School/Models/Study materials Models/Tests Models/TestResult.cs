using System.ComponentModel.DataAnnotations;

namespace Online_Language_School.Models
{
    public class TestResult
    {
        [Key]
        public int Id { get; set; }
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
        public int Score { get; set; }
        public int MaxScore { get; set; }
        public bool Passed { get; set; }
        public int TimeSpent { get; set; } // seconds

        // Foreign Keys
        [Required]
        public string StudentId { get; set; }
        public int TestId { get; set; }

        // Navigation
        public virtual ApplicationUser Student { get; set; }
        public virtual Test Test { get; set; }
    }
}