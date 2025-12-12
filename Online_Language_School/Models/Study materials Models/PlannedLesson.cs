namespace Online_Language_School.Models
{
    public class PlannedLesson
    {
        public int Id { get; set; }

        public int LessonId { get; set; }
        public Lesson Lesson { get; set; }

        // Only for individual courses
        public string? StudentId { get; set; }
        public ApplicationUser? Student { get; set; }

        public DateTime StartTime { get; set; }
        public int DurationMinutes { get; set; }

        public DateTime EndTime => StartTime.AddMinutes(DurationMinutes);
    }
}
