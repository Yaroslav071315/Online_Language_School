using Microsoft.AspNetCore.Mvc;
using Online_Language_School.Models;

namespace Online_Language_School.Controllers
{
    public class AccountController : Controller
    {
        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            // TODO: логіка перевірки користувача через Identity
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Please enter both email and password.";
                return View();
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public IActionResult Register(string name, string email, string password, string repeatPassword, string role)
        {
            if (password != repeatPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                return View();
            }

            // TODO: створення користувача через UserManager
            // var user = new ApplicationUser { UserName = name, Email = email, UserType = ... };

            return RedirectToAction("Login", "Account");
        }
    }
}