using System.ComponentModel.DataAnnotations;

namespace Online_Language_School.Models
{
    public class Test
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        public string Description { get; set; }
        public int TimeLimit { get; set; } // minutes
        public int PassingScore { get; set; }
        public bool IsActive { get; set; } = true;


        // Foreign Key
        public int LessonId { get; set; }

        // Navigation
        public virtual Lesson Lesson { get; set; }
        public virtual ICollection<Question> Questions { get; set; }
        public virtual ICollection<TestResult> TestResults { get; set; }
    }
}