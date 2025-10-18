using Microsoft.AspNetCore.Mvc;

namespace Online_Language_School.Controllers
{
    public class CoursesController : Controller
    {
        // GET: /Courses/Index
        [HttpGet]
        public IActionResult Index()
        {
            // Тут можна буде вивести список усіх курсів
            return View();
        }

        // GET: /Courses/Details/{id}
        [HttpGet]
        public IActionResult Details(int id)
        {
            // Для прикладу ми просто повертаємо CourseDetails.cshtml
            // У майбутньому тут можна буде завантажувати дані курсу з БД за id
            return View("CourseDetails");
        }
    }
}
