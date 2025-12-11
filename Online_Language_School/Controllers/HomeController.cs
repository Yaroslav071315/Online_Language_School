using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Online_Language_School.Controllers
{
    public class HomeController : Controller
    {
        // Головна сторінка (Main Page)
        [HttpGet]
        public IActionResult Index()
        {
            return View("MainPage"); // шукає Views/Home/MainPage.cshtml
        }

        // Секція "Про школу"
        [HttpGet]
        public IActionResult About()
        {
            return View();
        }

        // Секція "Переваги"
        [HttpGet]
        public IActionResult Advantages()
        {
            return View();
        }

        // Секція "Контакти"
        [HttpGet]
        public IActionResult Contacts()
        {
            return View();
        }

        // Секція "Новини"
        [HttpGet]
        public IActionResult News()
        {
            // тут можна підтягувати новини з БД
            return View();
        }


        //layout
        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            if (string.IsNullOrEmpty(culture))
                culture = "en"; // дефолт

            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl ?? "/");
        }


    }
}
