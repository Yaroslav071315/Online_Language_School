using Online_Language_School.Models;

namespace Online_Language_School.ViewModels
{
    public class CourseFilterViewModel
    {
        public string? Title { get; set; }
        public string? Format { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? MinStudents { get; set; }
        public int? MaxStudents { get; set; }
        public string? Language { get; set; }
        public string? Level { get; set; }
        public CourseStatus? Status { get; set; }
        public bool OnlySubmitted { get; set; }
    }
}
