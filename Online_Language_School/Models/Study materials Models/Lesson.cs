using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static System.Net.Mime.MediaTypeNames;

namespace Online_Language_School.Models
{
    public class Lesson
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        public string Description { get; set; }
        public int OrderNumber { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public int DurationMinutes { get; set; }

        // Foreign Key
        public int CourseId { get; set; }

        // Navigation
        public virtual Course Course { get; set; }
        public virtual ICollection<LessonMaterial> Materials { get; set; }
        public virtual ICollection<Test> Tests { get; set; }
    }
}