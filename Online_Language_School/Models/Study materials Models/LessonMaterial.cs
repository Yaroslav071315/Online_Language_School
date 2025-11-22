using System.ComponentModel.DataAnnotations;

namespace Online_Language_School.Models
{
    public class LessonMaterial
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        [Required]
        public string FileUrl { get; set; }

        [Required]
        public string FileType { get; set; } // PDF, Video, Document

        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiryDate { get; set; }

        // Foreign Key
        public int LessonId { get; set; }

        // Navigation
        public virtual Lesson Lesson { get; set; }
    }
}