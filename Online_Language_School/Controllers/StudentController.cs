using Microsoft.AspNetCore.Mvc;

namespace Online_Language_School.Controllers
{
    public class StudentController : Controller
    {
        // GET: /Student/Office
        [HttpGet]
        public IActionResult Office()
        {
            // Повертає представлення Views/Student/StudentOffice.cshtml
            return View("StudentOffice");
        }

        // Додаткові дії для вкладок (опціонально)
        [HttpGet]
        public IActionResult Schedule()
        {
            return PartialView("_Schedule"); // можна зробити окремий partial
        }

        [HttpGet]
        public IActionResult Materials()
        {
            return PartialView("_Materials");
        }

        [HttpGet]
        public IActionResult Progress()
        {
            return PartialView("_Progress");
        }

        [HttpGet]
        public IActionResult Payments()
        {
            return PartialView("_Payments");
        }

        [HttpGet]
        public IActionResult Chat()
        {
            return PartialView("_Chat");
        }
    }
}
