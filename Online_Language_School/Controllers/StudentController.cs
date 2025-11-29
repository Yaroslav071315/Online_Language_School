using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Language_School.Data;
using Online_Language_School.Models;
using Online_Language_School.ViewModels;

namespace Online_Language_School.Controllers
{
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Student/Office
        [HttpGet]
        public IActionResult Office()
        {
            return View("StudentOffice");
        }

        // GET: /Student/ScheduleAll
        [HttpGet]
        public IActionResult ScheduleAll()
        {
            var studentId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(studentId))
            {
                //TempData["Errors"] = "Not logged in.";
                return Unauthorized();
                //return RedirectToAction("Login", "Account");
            }

            // Витягуємо всі уроки з усіх курсів
            var lessons = _context.Lessons
                .Include(l => l.Course)
                .OrderBy(l => l.ScheduledDate)
                .ToList();

            return PartialView("_ScheduleStudent", lessons ?? new List<Lesson>());
        }

        [HttpGet]
        public IActionResult Materials()
        {
            // Просто дістаємо всі LessonMaterials з БД
            var materials = _context.LessonMaterials
                .Include(m => m.Lesson)
                    .ThenInclude(l => l.Course)
                .OrderByDescending(m => m.UploadedAt)
                .ToList();

            // Для зручності можна передати список уроків (не обов’язково)
            ViewBag.Lessons = _context.Lessons
                .Include(l => l.Course)
                .OrderBy(l => l.ScheduledDate)
                .ToList();

            return PartialView("_MaterialsStudent", materials);
        }

        [HttpGet]
        public IActionResult Progress()
        {
            // Витягуємо всі тести з бази з прив’язкою до уроків
            var tests = _context.Tests
                .Include(t => t.Lesson)
                    .ThenInclude(l => l.Course)
                .OrderBy(t => t.Lesson.ScheduledDate)
                .ToList();

            return PartialView("_ProgressStudent", tests);
        }

        [HttpGet]
        public IActionResult Payments()
        {
            var studentId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(studentId))
            {
                TempData["Errors"] = "Not logged in.";
                return RedirectToAction("Login", "Account");
            }

            var payments = _context.Payments
                .Where(p => p.UserId == studentId)
                .Include(p => p.Course)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            return PartialView("_PaymentsStudent", payments);
        }

        [HttpGet]
        public IActionResult Chat()
        {
            var studentId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(studentId))
            {
                TempData["Errors"] = "Not logged in.";
                return RedirectToAction("Login", "Account");
            }

            var messages = _context.ChatMessages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => m.SenderId == studentId || m.ReceiverId == studentId)
                .OrderByDescending(m => m.SentAt)
                .ToList();

            return PartialView("_ChatStudent", messages);
        }

        [HttpPost]
        public IActionResult SendMessage(string receiverId, string content)
        {
            var studentId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(studentId))
            {
                TempData["Errors"] = "Not logged in.";
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(content) || string.IsNullOrWhiteSpace(receiverId))
            {
                TempData["Errors"] = "Message and Receiver are required.";
                return RedirectToAction("Chat");
            }

            var msg = new ChatMessage
            {
                Content = content,
                SentAt = DateTime.UtcNow,
                SenderId = studentId,
                ReceiverId = receiverId
            };

            _context.ChatMessages.Add(msg);
            _context.SaveChanges();

            return RedirectToAction("Office");
        }

        [HttpGet]
        public IActionResult Courses(string level, string language, decimal? price, string format, int? maxStudents)
        {
            var query = _context.Courses.AsQueryable();

            if (!string.IsNullOrEmpty(level))
                query = query.Where(c => c.Level == level);

            if (!string.IsNullOrEmpty(language))
                query = query.Where(c => c.Language == language);

            if (!string.IsNullOrEmpty(format))
                query = query.Where(c => c.Format == format);

            if (price.HasValue)
                query = query.Where(c => c.Price <= price.Value);

            if (maxStudents.HasValue)
                query = query.Where(c => c.MaxStudents <= maxStudents.Value);

            var model = new CourseSearchViewModel
            {
                Level = level,
                Language = language,
                Price = price,
                Format = format,
                MaxStudents = maxStudents,
                Courses = query.ToList()
            };

            return PartialView("_CoursesPartial", model);
        }
    }
}