using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Online_Language_School.Models;

namespace Online_Language_School.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager,
                                 SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Please enter both email and password.";
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(email, password, false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(email);

                HttpContext.Session.SetString("UserId", user.Id);

                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Student"))
                    return RedirectToAction("Office", "Student");
                if (roles.Contains("Teacher"))
                    return RedirectToAction("Office", "Teacher");
                if (roles.Contains("Administrator"))
                    return RedirectToAction("Office", "Administrator");

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Invalid email or password.";
            return View();
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(string firstName, string lastName, string email, string password, string repeatPassword, string role)
        {
            if (password != repeatPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return View();
            }

            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                ModelState.AddModelError("", "User with this email already exists.");
                return View();
            }

            // Роль завжди Student
            string normalizedRole = "Student";


            var user = new ApplicationUser
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                UserName = email,
                RegistrationDate = DateTime.UtcNow,
                IsActive = true,
                UserType = normalizedRole switch
                {
                    "Student" => UserType.Student,
                    "Teacher" => UserType.Teacher,
                    "Administrator" => UserType.Administrator,
                    _ => UserType.Student
                }
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, normalizedRole);
                await _signInManager.SignInAsync(user, isPersistent: false);

                HttpContext.Session.SetString("UserId", user.Id);

                return normalizedRole switch
                {
                    "Student" => RedirectToAction("Office", "Student"),
                    "Teacher" => RedirectToAction("Office", "Teacher"),
                    "Administrator" => RedirectToAction("Office", "Administrator"),
                    _ => RedirectToAction("Index", "Home")
                };
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View();
        }
    }
}
