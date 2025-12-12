using System.ComponentModel.DataAnnotations;

namespace Online_Language_School.Models
{
    public class Lesson
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        public string Description { get; set; }
        public int OrderNumber { get; set; }
        public int DurationMinutes { get; set; }

        // Foreign Key
        public int CourseId { get; set; }

        // Navigation
        public virtual Course Course { get; set; }
        public virtual ICollection<LessonMaterial> LessonMaterials { get; set; }
        public virtual ICollection<Test> Tests { get; set; }
    }
}