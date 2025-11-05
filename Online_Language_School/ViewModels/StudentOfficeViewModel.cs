using Online_Language_School.Models;

namespace Online_Language_School.ViewModels
{
    public class StudentOfficeViewModel
    {
        public ApplicationUser Student { get; set; }

        public List<Enrollment> Enrollments { get; set; } = new();
        public List<Lesson> Lessons { get; set; } = new();
        public List<TestResult> TestResults { get; set; } = new();
        public List<Payment> Payments { get; set; } = new();
        public List<ChatMessage> ChatMessages { get; set; } = new();
    }
}