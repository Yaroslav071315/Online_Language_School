using System.ComponentModel.DataAnnotations;

namespace Online_Language_School.Models
{
    public class Enrollment
    {
        [Key]
        public int Id { get; set; }
        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [Required]
        public string Status { get; set; } // Pending, Active, Completed, Cancelled

        public decimal PaidAmount { get; set; }

        // Foreign Keys
        [Required]
        public string StudentId { get; set; }
        public int CourseId { get; set; }

        // Navigation
        public virtual ApplicationUser Student { get; set; }
        public virtual Course Course { get; set; }
    }
}