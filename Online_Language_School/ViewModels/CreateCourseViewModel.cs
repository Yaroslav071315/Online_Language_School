using System.ComponentModel.DataAnnotations;

namespace Online_Language_School.ViewModels
{
    public class CreateCourseViewModel
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Language { get; set; }

        [Required]
        public string Level { get; set; }

        [Required]
        public string Format { get; set; }

        [Range(0, 999999)]
        public decimal Price { get; set; }

        public int? MaxStudents { get; set; }

        public string? ImageUrl { get; set; }

        [Required]
        public string Description { get; set; }
    }

}
