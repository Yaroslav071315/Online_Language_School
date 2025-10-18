using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Online_Language_School.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required, Range(0, 999999)]
        public decimal Price { get; set; }

        [Required]
        public string Language { get; set; }

        [Required]
        public string Level { get; set; } // Beginner, Intermediate, Advanced

        [Required]
        public string Format { get; set; } // Group, Individual

        public int MaxStudents { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 👇 тепер курс створює адміністратор
        [Required]
        public string AdministratorId { get; set; }

        public virtual ApplicationUser Administrator { get; set; }
        public virtual ICollection<Lesson> Lessons { get; set; }
        public virtual ICollection<Enrollment> Enrollments { get; set; }
        public virtual ICollection<CourseReview> Reviews { get; set; }
    }
}