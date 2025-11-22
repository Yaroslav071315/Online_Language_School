using System.ComponentModel.DataAnnotations;

namespace Online_Language_School.Models
{
    public class Question
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string QuestionText { get; set; }

        [Required]
        public string QuestionType { get; set; } // MultipleChoice, OpenText, Matching

        public int Points { get; set; }
        public int OrderNumber { get; set; }

        // Foreign Key
        public int TestId { get; set; }

        // Navigation
        public virtual Test Test { get; set; }
        public virtual ICollection<Answer> Answers { get; set; }
    }
}
