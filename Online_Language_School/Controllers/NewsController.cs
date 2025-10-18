using Microsoft.AspNetCore.Mvc;

namespace Online_Language_School.Controllers
{
    public class NewsController : Controller
    {
        public IActionResult Index()
        {
            return View("News");
        }
    }
}
