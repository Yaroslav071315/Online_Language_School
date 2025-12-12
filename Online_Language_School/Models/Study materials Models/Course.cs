using System.ComponentModel.DataAnnotations;

namespace Online_Language_School.Models
{
    public enum CourseStatus
    {
        Draft = 0,       // Чорновик
        Submitted = 1,   // Відправлений на перевірку
        Approved = 2,    // Затверджений адміном
        Enrollment = 3, // Йде набір
        Active = 4      // Навчання йде
    }

    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        [Required]
        public string? Description { get; set; }

        [Required, Range(0, 999999)]
        public decimal Price { get; set; }

        [Required]
        public string Language { get; set; }

        [Required]
        public string Level { get; set; } // Beginner, Intermediate, Advanced

        [Required]
        public string Format { get; set; } // Group, Individual

        public int? MaxStudents { get; set; }
        public string? ImageUrl { get; set; }
        public CourseStatus Status { get; set; } = CourseStatus.Draft;
        public CourseStatus PreviousStatus { get; set; } = CourseStatus.Approved;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Викладач, який веде курс
        [Required]
        public string? TeacherId { get; set; }
        public virtual ApplicationUser? Teacher { get; set; }

        // Навігаційні властивості
        public virtual ICollection<Lesson> Lessons { get; set; }
        public virtual ICollection<Enrollment> Enrollments { get; set; }
        public virtual ICollection<CourseReview> Reviews { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
    }
}