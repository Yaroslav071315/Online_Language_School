using Microsoft.AspNetCore.Mvc;

namespace Online_Language_School.Controllers
{
    public class AdministratorController : Controller
    {
        // GET: /Administrator/Office
        [HttpGet]
        public IActionResult Office()
        {
            // Повертає представлення Views/Administrator/AdministratorOffice.cshtml
            return View("AdministratorOffice");
        }

        // Опціонально: окремі partial views для вкладок
        [HttpGet]
        public IActionResult Users()
        {
            return PartialView("_UsersAdmin");
        }

        [HttpGet]
        public IActionResult Courses()
        {
            return PartialView("_CoursesAdmin");
        }

        [HttpGet]
        public IActionResult News()
        {
            return PartialView("_NewsAdmin");
        }

        [HttpGet]
        public IActionResult Payments()
        {
            return PartialView("_PaymentsAdmin");
        }
    }
}