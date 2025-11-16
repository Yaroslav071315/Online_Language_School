using Online_Language_School.Models;

namespace Online_Language_School.ViewModels
{
    public class CourseSearchViewModel
    {
        public int? MaxStudents { get; set; }
        public string Level { get; set; }
        public string Language { get; set; }
        public decimal? Price { get; set; }
        public string Format { get; set; }

        public List<Course> Courses { get; set; } = new();
    }
}
