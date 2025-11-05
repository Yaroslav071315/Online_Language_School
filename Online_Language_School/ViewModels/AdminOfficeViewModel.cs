using Online_Language_School.Models;

namespace Online_Language_School.ViewModels
{
    public class AdminOfficeViewModel
    {
        public IEnumerable<Course> Courses { get; set; } = new List<Course>();
        public IEnumerable<Payment> Payments { get; set; } = new List<Payment>();
        public IEnumerable<BlogPost> News { get; set; } = new List<BlogPost>();
        public IEnumerable<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
        public IEnumerable<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public IEnumerable<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    }
}