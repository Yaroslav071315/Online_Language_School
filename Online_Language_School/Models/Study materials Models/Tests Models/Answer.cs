using System.ComponentModel.DataAnnotations;

namespace Online_Language_School.Models
{
    public class Answer
    {
        public int Id { get; set; }

        [Required]
        public string AnswerText { get; set; }

        public bool IsCorrect { get; set; }

        // Foreign Key
        public int QuestionId { get; set; }

        // Navigation
        public virtual Question Question { get; set; }
    }
}