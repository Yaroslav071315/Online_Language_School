using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Online_Language_School.Models
{
    public enum UserType
    {
        Student = 0,
        Teacher = 1,
        Administrator = 2
    }

    public class ApplicationUser : IdentityUser
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; }

        [Required, MaxLength(100)]
        public string LastName { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        [Required]
        public UserType UserType { get; set; }

        // Навігаційні властивості
        public virtual ICollection<Enrollment> Enrollments { get; set; }
        public virtual ICollection<CourseReview> CourseReviews { get; set; }
        public virtual ICollection<TestResult> TestResults { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
        public virtual ICollection<ChatMessage> SentMessages { get; set; }
        public virtual ICollection<ChatMessage> ReceivedMessages { get; set; }
        public virtual ICollection<BlogPost> BlogPosts { get; set; }
    }
}
