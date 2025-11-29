using Microsoft.AspNetCore.Mvc;
using Online_Language_School.Data;
using Online_Language_School.Models;

namespace Online_Language_School.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

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
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Please enter both email and password.";
                return View();
            }

            // Перевірка користувача в БД
            var user = _context.Users
                .FirstOrDefault(u => u.Email == email && u.PasswordHash == password);

            if (user == null)
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            // після перевірки користувача
            HttpContext.Session.SetString("UserId", user.Id);
            HttpContext.Session.SetString("UserRole", user.UserType.ToString());

            return user.UserType switch
            {
                UserType.Student => RedirectToAction("Office", "Student"),
                UserType.Teacher => RedirectToAction("Office", "Teacher"),
                UserType.Administrator => RedirectToAction("Office", "Administrator"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public IActionResult Register(string firstName, string lastName, string email, string password, string repeatPassword, string role)
        {
            if (password != repeatPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                return View();
            }

            if (_context.Users.Any(u => u.Email == email))
            {
                ViewBag.Error = "User with this email already exists.";
                return View();
            }

            //// Визначаємо роль
            //UserType userType = role switch
            //{
            //    "Student" => UserType.Student,
            //    "Teacher" => UserType.Teacher,
            //    "Administrator" => UserType.Administrator,
            //    _ => UserType.Student
            //};

            // Роль завжди Student
            UserType userType = UserType.Student;


            // Створюємо користувача
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                UserName = email,
                PasswordHash = password, // ⚠️ у реальному проєкті треба хешувати!
                RegistrationDate = DateTime.UtcNow,
                IsActive = true,
                UserType = userType
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return RedirectToAction("Login", "Account");
        }
    }
}