using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Language_School.Data;
using Online_Language_School.Models;

namespace Online_Language_School.Controllers
{
    public class TeacherController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TeacherController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Teacher/Office
        [HttpGet]
        public IActionResult Office()
        {
            return View("TeacherOffice");
        }

        // GET: /Teacher/Schedule
        [HttpGet]
        public IActionResult Schedule()
        {
            var lessons = _context.Lessons
                .Include(l => l.Course)
                .OrderBy(l => l.ScheduledDate)
                .ToList();

            ViewBag.Lessons = lessons; // для випадаючого меню

            return PartialView("_ScheduleTeacher", lessons);
        }

        [HttpGet]
        public IActionResult Materials()
        {
            var materials = _context.LessonMaterials
                .Include(m => m.Lesson)
                .OrderByDescending(m => m.UploadedAt)
                .ToList();

            ViewBag.Lessons = _context.Lessons
                .OrderBy(l => l.ScheduledDate)
                .ToList();

            return PartialView("_MaterialsTeacher", materials);
        }

        [HttpGet]
        public IActionResult Homeworks() => PartialView("_HomeworksTeacher");

        //[HttpGet]
        //public IActionResult Chat() => PartialView("_ChatTeacher");

        // POST: /Teacher/CreateLesson
        [HttpPost]
        public IActionResult CreateLesson(string title, string description, int orderNumber, DateTime? scheduledDate, int durationMinutes, int courseId)
        {
            if (string.IsNullOrWhiteSpace(title) || courseId <= 0)
            {
                TempData["Errors"] = "Title and Course are required.";
                return RedirectToAction("Schedule");
            }

            var lesson = new Lesson
            {
                Title = title,
                Description = description,
                OrderNumber = orderNumber,
                ScheduledDate = scheduledDate,
                DurationMinutes = durationMinutes,
                CourseId = courseId
            };

            _context.Lessons.Add(lesson);
            _context.SaveChanges();

            return RedirectToAction("Office");
        }

        // POST: /Teacher/EditLesson
        [HttpPost]
        public IActionResult EditLesson(int id, string title, string description, int orderNumber, DateTime? scheduledDate, int durationMinutes, int courseId)
        {
            var lesson = _context.Lessons.FirstOrDefault(l => l.Id == id);
            if (lesson == null) return RedirectToAction("Schedule");

            lesson.Title = title;
            lesson.Description = description;
            lesson.OrderNumber = orderNumber;
            lesson.ScheduledDate = scheduledDate;
            lesson.DurationMinutes = durationMinutes;
            lesson.CourseId = courseId;

            _context.Lessons.Update(lesson);
            _context.SaveChanges();

            return RedirectToAction("Office");
        }

        // POST: /Teacher/DeleteLesson
        [HttpPost]
        public IActionResult DeleteLesson(int id)
        {
            var lesson = _context.Lessons.FirstOrDefault(l => l.Id == id);
            if (lesson != null)
            {
                _context.Lessons.Remove(lesson);
                _context.SaveChanges();
            }
            return RedirectToAction("Office");
        }


        // POST: /Teacher/CreateMaterial
        [HttpPost]
        public IActionResult CreateMaterial(string title, string fileUrl, string fileType, long fileSize, DateTime? expiryDate, int lessonId)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(fileUrl) || string.IsNullOrWhiteSpace(fileType) || lessonId <= 0)
            {
                TempData["Errors"] = "All required fields must be filled.";
                return RedirectToAction("Materials");
            }

            var material = new LessonMaterial
            {
                Title = title,
                FileUrl = fileUrl,
                FileType = fileType,
                FileSize = fileSize,
                ExpiryDate = expiryDate,
                LessonId = lessonId,
                UploadedAt = DateTime.UtcNow
            };

            _context.LessonMaterials.Add(material);
            _context.SaveChanges();

            return RedirectToAction("Office");
        }

        // POST: /Teacher/EditMaterial
        [HttpPost]
        public IActionResult EditMaterial(int id, string title, string fileUrl, string fileType, long fileSize, DateTime? expiryDate, int lessonId)
        {
            var material = _context.LessonMaterials.FirstOrDefault(m => m.Id == id);
            if (material == null) return RedirectToAction("Materials");

            material.Title = title;
            material.FileUrl = fileUrl;
            material.FileType = fileType;
            material.FileSize = fileSize;
            material.ExpiryDate = expiryDate;
            material.LessonId = lessonId;

            _context.LessonMaterials.Update(material);
            _context.SaveChanges();

            return RedirectToAction("Office");
        }

        // POST: /Teacher/DeleteMaterial
        [HttpPost]
        public IActionResult DeleteMaterial(int id)
        {
            var material = _context.LessonMaterials.FirstOrDefault(m => m.Id == id);
            if (material != null)
            {
                _context.LessonMaterials.Remove(material);
                _context.SaveChanges();
            }
            return RedirectToAction("Office");
        }

        [HttpGet]
        public IActionResult Tests()
        {
            var tests = _context.Tests
                .Include(t => t.Lesson)
                .Include(t => t.Questions)
                    .ThenInclude(q => q.Answers)
                .ToList();

            ViewBag.Lessons = _context.Lessons.ToList();

            return PartialView("_TestsTeacher", tests);
        }

        [HttpPost]
        public IActionResult CreateTest(string title, string description, int timeLimit, int passingScore, int lessonId)
        {
            if (string.IsNullOrWhiteSpace(title) || lessonId <= 0)
                return RedirectToAction("Tests");

            var test = new Test
            {
                Title = title,
                Description = description,
                TimeLimit = timeLimit,
                PassingScore = passingScore,
                LessonId = lessonId,
                IsActive = true
            };

            _context.Tests.Add(test);
            _context.SaveChanges();

            return RedirectToAction("Office");
        }

        [HttpPost]
        public IActionResult EditTest(int id, string title, string description, int timeLimit, int passingScore, bool isActive, int lessonId)
        {
            var test = _context.Tests.FirstOrDefault(t => t.Id == id);
            if (test == null) return RedirectToAction("Tests");

            test.Title = title;
            test.Description = description;
            test.TimeLimit = timeLimit;
            test.PassingScore = passingScore;
            test.IsActive = isActive;
            test.LessonId = lessonId;

            _context.Tests.Update(test);
            _context.SaveChanges();

            return RedirectToAction("Office");
        }

        [HttpPost]
        public IActionResult DeleteTest(int id)
        {
            var test = _context.Tests.FirstOrDefault(t => t.Id == id);
            if (test != null)
            {
                _context.Tests.Remove(test);
                _context.SaveChanges();
            }
            return RedirectToAction("Office");
        }

        [HttpGet]
        public IActionResult Chat()
        {
            var teacherId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(teacherId))
            {
                TempData["Errors"] = "Not logged in.";
                return RedirectToAction("Login", "Account");
            }

            var messages = _context.ChatMessages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => m.SenderId == teacherId || m.ReceiverId == teacherId)
                .OrderByDescending(m => m.SentAt)
                .ToList();

            return PartialView("_ChatTeacher", messages);
        }

        [HttpPost]
        public IActionResult SendMessage(string receiverId, string content)
        {
            var teacherId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(teacherId))
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
                SenderId = teacherId,
                ReceiverId = receiverId
            };

            _context.ChatMessages.Add(msg);
            _context.SaveChanges();

            return RedirectToAction("Office");
        }

    }
}