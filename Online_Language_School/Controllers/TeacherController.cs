using Microsoft.AspNetCore.Mvc;

namespace Online_Language_School.Controllers
{
    public class TeacherController : Controller
    {
        // GET: /Teacher/Office
        [HttpGet]
        public IActionResult Office()
        {
            // Повертає представлення Views/Teacher/TeacherOffice.cshtml
            return View("TeacherOffice");
        }

        // Опціонально: можна додати окремі методи для вкладок
        [HttpGet]
        public IActionResult Schedule()
        {
            return PartialView("_ScheduleTeacher"); // якщо захочеш винести у partial
        }

        [HttpGet]
        public IActionResult Materials()
        {
            return PartialView("_MaterialsTeacher");
        }

        [HttpGet]
        public IActionResult Homeworks()
        {
            return PartialView("_HomeworksTeacher");
        }

        [HttpGet]
        public IActionResult Chat()
        {
            return PartialView("_ChatTeacher");
        }
    }
}