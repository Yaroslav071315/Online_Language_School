using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Language_School.Data;
using Online_Language_School.Models;
using Online_Language_School.ViewModels;

namespace Online_Language_School.Controllers
{
    public class AdministratorController : Controller
    {
        private readonly ApplicationDbContext _context;



        public AdministratorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ====== ГОЛОВНА СТОРІНКА АДМІНА ======
        public IActionResult Office()
        {
            var model = new AdminOfficeViewModel
            {
                Users = _context.Users.ToList(),
                Courses = _context.Courses.Include(c => c.Administrator).ToList(),
                News = _context.News.Include(n => n.Admin).ToList(),
                Payments = _context.Payments
                    .Include(p => p.User)
                    .Include(p => p.Course)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToList(),
                ChatMessages = _context.ChatMessages
                    .Include(c => c.Sender)
                    .Include(c => c.Receiver)
                    .OrderByDescending(c => c.SentAt)
                    .Take(50)
                    .ToList(),
                Enrollments = _context.Enrollments
                    .Include(e => e.Student)
                    .Include(e => e.Course)
                    .ToList(),
            };

            return View("AdministratorOffice", model);
        }

        // ====== КОРИСТУВАЧІ ======
        [HttpPost]
        public IActionResult ToggleUserActive(string id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                user.IsActive = !user.IsActive;
                _context.SaveChanges();
            }
            return RedirectToAction("Office");
        }

        [HttpPost]
        public IActionResult ChangeUserRole(string id, string role)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            // Перетворюємо string role у enum UserType
            user.UserType = role switch
            {
                "Teacher" => UserType.Teacher,
                "Administrator" => UserType.Administrator,
                _ => UserType.Student
            };

            _context.SaveChanges();

            return RedirectToAction("Office"); // твій метод/в'ю для списку користувачів
        }



        // POST: /Administrator/CreateCourse
        [HttpPost]
        public IActionResult CreateCourse(
            string title,
            string description,
            decimal price,
            string language,
            string level,
            string format,
            int? maxStudents,
            string imageUrl,
            string administratorId,
            string teacherId)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description) ||
                string.IsNullOrWhiteSpace(language) || string.IsNullOrWhiteSpace(level) ||
                string.IsNullOrWhiteSpace(format) || string.IsNullOrWhiteSpace(administratorId) ||
                string.IsNullOrWhiteSpace(teacherId))
            {
                TempData["Errors"] = "All required fields must be filled.";
                return RedirectToAction("Office");
            }

            if (_context.Courses.Any(c => c.Title == title && c.Language == language && c.Level == level))
            {
                TempData["Errors"] = "Course with the same Title, Language and Level already exists.";
                return RedirectToAction("Office");
            }

            var course = new Course
            {
                Title = title,
                Description = description,
                Price = price,
                Language = language,
                Level = level,
                Format = format,
                MaxStudents = maxStudents,
                ImageUrl = imageUrl,
                AdministratorId = administratorId,
                TeacherId = teacherId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Courses.Add(course);
            _context.SaveChanges();

            return RedirectToAction("Office");
        }

        // POST: /Administrator/EditCourse
        [HttpPost]
        public IActionResult EditCourse(
            int id,
            string title,
            string description,
            decimal price,
            string language,
            string level,
            string format,
            int? maxStudents,
            string imageUrl,
            string administratorId,
            string teacherId)
        {
            if (id <= 0)
            {
                TempData["Errors"] = "Invalid course Id.";
                return RedirectToAction("Office");
            }

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description) ||
                string.IsNullOrWhiteSpace(language) || string.IsNullOrWhiteSpace(level) ||
                string.IsNullOrWhiteSpace(format) || string.IsNullOrWhiteSpace(administratorId) ||
                string.IsNullOrWhiteSpace(teacherId))
            {
                TempData["Errors"] = "All required fields must be filled.";
                return RedirectToAction("Office");
            }

            var course = _context.Courses.FirstOrDefault(c => c.Id == id);
            if (course == null)
            {
                TempData["Errors"] = "Course not found.";
                return RedirectToAction("Office");
            }

            // Оновлюємо дані
            course.Title = title;
            course.Description = description;
            course.Price = price;
            course.Language = language;
            course.Level = level;
            course.Format = format;
            course.MaxStudents = maxStudents;
            course.ImageUrl = imageUrl;
            course.AdministratorId = administratorId;
            course.TeacherId = teacherId;

            _context.Courses.Update(course);
            _context.SaveChanges();

            return RedirectToAction("Office");
        }

        // POST: /Administrator/DeleteCourse
        [HttpPost]
        public IActionResult DeleteCourse(int id)
        {
            var course = _context.Courses.FirstOrDefault(c => c.Id == id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                _context.SaveChanges();
            }
            return RedirectToAction("Office");
        }

        // POST: /Administrator/ToggleCourseActive
        [HttpPost]
        public IActionResult ToggleCourseActive(int id)
        {
            var course = _context.Courses.FirstOrDefault(c => c.Id == id);
            if (course != null)
            {
                course.IsActive = !course.IsActive;
                _context.SaveChanges();
            }
            return RedirectToAction("Office");
        }

        // ====== НОВИНИ ======


        // POST: /Administrator/CreatePayment
        [HttpPost]
        public IActionResult CreatePayment(string userId, int courseId, decimal amount, string currency, string status, DateTime createdAt)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(currency) || string.IsNullOrWhiteSpace(status))
            {
                TempData["Errors"] = "All required fields must be filled.";
                return RedirectToAction("Office");
            }

            var payment = new Payment
            {
                UserId = userId,
                CourseId = courseId,
                Amount = amount,
                Currency = currency,
                Status = status,
                CreatedAt = createdAt
            };

            _context.Payments.Add(payment);
            _context.SaveChanges();

            return RedirectToAction("Office");
        }

        // POST: /Administrator/EditPayment
        [HttpPost]
        public IActionResult EditPayment(int id, string userId, int courseId, decimal amount, string currency, string status, DateTime createdAt)
        {
            var payment = _context.Payments.FirstOrDefault(p => p.Id == id);
            if (payment == null)
            {
                TempData["Errors"] = "Payment not found.";
                return RedirectToAction("Office");
            }

            payment.UserId = userId;
            payment.CourseId = courseId;
            payment.Amount = amount;
            payment.Currency = currency;
            payment.Status = status;
            payment.CreatedAt = createdAt;

            _context.Payments.Update(payment);
            _context.SaveChanges();

            return RedirectToAction("Office");
        }

        // POST: /Administrator/DeletePayment
        [HttpPost]
        public IActionResult DeletePayment(int id)
        {
            var payment = _context.Payments.FirstOrDefault(p => p.Id == id);
            if (payment != null)
            {
                _context.Payments.Remove(payment);
                _context.SaveChanges();
            }
            return RedirectToAction("Office");
        }

        // ====== ПЛАТЕЖІ (звіт) ======
        public IActionResult PaymentsReport()
        {
            var report = _context.Payments
                .Include(p => p.User)
                .Include(p => p.Course)
                .GroupBy(p => p.Course.Title)
                .Select(g => new
                {
                    Course = g.Key,
                    TotalAmount = g.Sum(p => p.Amount),
                    Count = g.Count()
                })
                .ToList();

            return Json(report); // можна віддати JSON або View
        }

        // POST: /Administrator/CreateNews
        [HttpPost]
        public IActionResult CreateNews(string title, string content, string? imageUrl, DateTime publishedAt, bool isPublished, string adminId)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content) || string.IsNullOrWhiteSpace(adminId))
            {
                TempData["Errors"] = "All required fields must be filled.";
                return RedirectToAction("Office");
            }

            var post = new BlogPost
            {
                Title = title,
                Content = content,
                ImageUrl = imageUrl,
                PublishedAt = publishedAt,
                IsPublished = isPublished,
                AdminId = adminId
            };

            _context.News.Add(post);
            _context.SaveChanges();

            return RedirectToAction("Office");
        }

        // POST: /Administrator/EditNews
        [HttpPost]
        public IActionResult EditNews(int id, string title, string content, string? imageUrl, DateTime publishedAt, bool isPublished, string adminId)
        {
            var post = _context.News.FirstOrDefault(p => p.Id == id);
            if (post == null)
            {
                TempData["Errors"] = "News not found.";
                return RedirectToAction("Office");
            }

            post.Title = title;
            post.Content = content;
            post.ImageUrl = imageUrl;
            post.PublishedAt = publishedAt;
            post.IsPublished = isPublished;
            post.AdminId = adminId;

            _context.News.Update(post);
            _context.SaveChanges();

            return RedirectToAction("Office");
        }

        // POST: /Administrator/DeleteNews
        [HttpPost]
        public IActionResult DeleteNews(int id)
        {
            var post = _context.News.FirstOrDefault(p => p.Id == id);
            if (post != null)
            {
                _context.News.Remove(post);
                _context.SaveChanges();
            }
            return RedirectToAction("Office");
        }


        [HttpGet]
        public IActionResult Chat()
        {
            var adminId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(adminId))
            {
                TempData["Errors"] = "Not logged in.";
                return RedirectToAction("Login", "Account");
            }

            var messages = _context.ChatMessages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => m.SenderId == adminId || m.ReceiverId == adminId)
                .OrderByDescending(m => m.SentAt)
                .ToList();



            return View(messages);
        }



        [HttpPost]
        public IActionResult SendMessage(string receiverId, string content)
        {
            var adminId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(adminId))
            {
                TempData["Errors"] = "Not logged in.";
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(content) || string.IsNullOrWhiteSpace(receiverId))
            {
                TempData["Errors"] = "Message and Receiver are required.";
                return RedirectToAction("Office");
            }

            var msg = new ChatMessage
            {
                Content = content,
                SentAt = DateTime.UtcNow,
                SenderId = adminId,
                ReceiverId = receiverId
            };

            _context.ChatMessages.Add(msg);
            _context.SaveChanges();

            return RedirectToAction("Office");
        }

        // POST: /Administrator/CreateEnrollment
        [HttpPost]
        public IActionResult CreateEnrollment(string studentId, int courseId, string status, decimal paidAmount, DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrWhiteSpace(studentId) || string.IsNullOrWhiteSpace(status))
            {
                TempData["Errors"] = "StudentId and Status are required.";
                return RedirectToAction("Office");
            }

            var enrollment = new Enrollment
            {
                StudentId = studentId,
                CourseId = courseId,
                Status = status,
                PaidAmount = paidAmount,
                StartDate = startDate,
                EndDate = endDate,
                EnrollmentDate = DateTime.UtcNow
            };

            _context.Enrollments.Add(enrollment);
            _context.SaveChanges();

            return RedirectToAction("Office");
        }

        // POST: /Administrator/EditEnrollment
        [HttpPost]
        public IActionResult EditEnrollment(int id, string studentId, int courseId, string status, decimal paidAmount, DateTime? startDate, DateTime? endDate)
        {
            var enrollment = _context.Enrollments.FirstOrDefault(e => e.Id == id);
            if (enrollment == null)
            {
                TempData["Errors"] = "Enrollment not found.";
                return RedirectToAction("Office");
            }

            enrollment.StudentId = studentId;
            enrollment.CourseId = courseId;
            enrollment.Status = status;
            enrollment.PaidAmount = paidAmount;
            enrollment.StartDate = startDate;
            enrollment.EndDate = endDate;

            _context.Enrollments.Update(enrollment);
            _context.SaveChanges();

            return RedirectToAction("Office");
        }

        // POST: /Administrator/DeleteEnrollment
        [HttpPost]
        public IActionResult DeleteEnrollment(int id)
        {
            var enrollment = _context.Enrollments.FirstOrDefault(e => e.Id == id);
            if (enrollment != null)
            {
                _context.Enrollments.Remove(enrollment);
                _context.SaveChanges();
            }
            return RedirectToAction("Office");
        }

    }
}