using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<ApplicationUser> _userManager;


        public AdministratorController(ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ====== ГОЛОВНА СТОРІНКА АДМІНА ======
        public IActionResult Office()
        {
            var model = new AdminOfficeViewModel
            {
                Users = _context.Users.ToList(),
                Courses = _context.Courses.ToList(),
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
        public async Task<IActionResult> ChangeUserRole(string id, string role)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Отримати всі поточні ролі користувача
            var currentRoles = await _userManager.GetRolesAsync(user);

            // Видалити користувача з усіх ролей
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                ModelState.AddModelError("", "Не вдалося видалити старі ролі");
                return RedirectToAction("Office");
            }

            // Додати нову роль
            var addResult = await _userManager.AddToRoleAsync(user, role);
            if (!addResult.Succeeded)
            {
                ModelState.AddModelError("", "Не вдалося додати нову роль");
                return RedirectToAction("Office");
            }

            return RedirectToAction("Office"); // твій метод/в'ю для списку користувачів
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

        // ====== НОВИНИ ======

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

        // GET: /Administrator/Users
        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.ToListAsync();
            return PartialView("_UsersAdmin", users);
        }

        // GET: /Administrator/Courses
        [HttpGet]
        public async Task<IActionResult> Courses(CourseFilterViewModel filter)
        {
            var query = _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Lessons)
                    .ThenInclude(l => l.LessonMaterials)
                .Include(c => c.Lessons)
                    .ThenInclude(l => l.Tests)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Title))
                query = query.Where(c => c.Title.Contains(filter.Title));
            if (!string.IsNullOrWhiteSpace(filter.Format))
                query = query.Where(c => c.Format == filter.Format);
            if (filter.MinPrice.HasValue)
                query = query.Where(c => c.Price >= filter.MinPrice);
            if (filter.MaxPrice.HasValue)
                query = query.Where(c => c.Price <= filter.MaxPrice);
            if (filter.MinStudents.HasValue)
                query = query.Where(c => c.MaxStudents >= filter.MinStudents);
            if (filter.MaxStudents.HasValue)
                query = query.Where(c => c.MaxStudents <= filter.MaxStudents);
            if (!string.IsNullOrWhiteSpace(filter.Language))
                query = query.Where(c => c.Language == filter.Language);
            if (!string.IsNullOrWhiteSpace(filter.Level))
                query = query.Where(c => c.Level == filter.Level);
            if (filter.OnlySubmitted)
                query = query.Where(c => c.Status == CourseStatus.Submitted);
            else if (filter.Status.HasValue)
                query = query.Where(c => c.Status == filter.Status);

            var courses = await query.ToListAsync();
            return PartialView("_CoursesAdmin", courses);
        }

        [HttpGet]
        public IActionResult ViewCourse(int id)
        {
            var course = _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Lessons)
                    .ThenInclude(l => l.LessonMaterials)
                .Include(c => c.Lessons)
                    .ThenInclude(l => l.Tests)
                .FirstOrDefault(c => c.Id == id);

            if (course == null) return NotFound();

            return PartialView("_ViewCourseAdmin", course);
        }

        [HttpPost]
        public IActionResult ApproveCourse(int id)
        {
            var course = _context.Courses.FirstOrDefault(c => c.Id == id);
            if (course == null) return NotFound();

            course.Status = CourseStatus.Approved;
            _context.SaveChanges();

            return RedirectToAction("Courses");
        }

        [HttpPost]
        public async Task<IActionResult> RejectCourse(RejectCourseViewModel model)
        {
            var course = await _context.Courses
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(c => c.Id == model.CourseId);

            if (course == null) return NotFound();

            course.Status = CourseStatus.Draft;
            await _context.SaveChangesAsync();

            //if (!string.IsNullOrWhiteSpace(model.Reason))
            //{
            //    await _emailService.SendAsync(
            //        course.Teacher.Email,
            //        "Course rejected",
            //        $"Your course \"{course.Title}\" was rejected.\nReason: {model.Reason}"
            //    );
            //}

            if(!string.IsNullOrWhiteSpace(model.Reason))
            {
                var sender = await _userManager.GetUserAsync(User);
                var receiverId = _context.Courses
                    .FirstOrDefault(c => c.Id == model.CourseId)
                    .TeacherId;
                var receiver = _context.Users.FirstOrDefault(u => u.Id == receiverId);

                var message = new ChatMessage()
                {
                    Content = $"Your course \"{course.Title}\" was rejected.\nReason: {model.Reason}",
                    Sender = sender,
                    Receiver = receiver
                };

                _context.ChatMessages.Add(message);
                _context.SaveChanges();
            }

            return RedirectToAction("Courses");
        }

        [HttpPost]
        public IActionResult FreezeCourse(int id)
        {
            var course = _context.Courses.FirstOrDefault(c => c.Id == id);
            if (course == null) return NotFound();

            if (course.Status == CourseStatus.Enrollment ||
                course.Status == CourseStatus.Active ||
                course.Status == CourseStatus.Approved)
            {
                course.PreviousStatus = course.Status;
                course.Status = CourseStatus.Draft;
                _context.SaveChanges();
            }

            return RedirectToAction("Courses");
        }

        [HttpPost]
        public IActionResult StartCourse(int id)
        {
            var course = _context.Courses.FirstOrDefault(c => c.Id == id);
            if (course == null) return NotFound();

            if (course.Status == CourseStatus.Draft)
            {
                course.Status = course.PreviousStatus;
                _context.SaveChanges();
            }

            return RedirectToAction("Courses");
        }


        // GET: /Administrator/News
        [HttpGet]
        public async Task<IActionResult> News()
        {
            var news = await _context.News
                .OrderByDescending(n => n.PublishedAt)
                .ToListAsync();
            return PartialView("_NewsAdmin", news);
        }


        // GET: /Administrator/Payments
        [HttpGet]
        public IActionResult Payments()
        {
            var payments = _context.Payments
                .Include(p => p.User)
                .Include(p => p.Course)
                .Where(p => p.ReceiptImagePath != null)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            return PartialView("_PaymentsAdmin", payments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApprovePayment(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Course)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
            {
                TempData["Errors"] = "Payment not found.";
                var pending1 = await _context.Payments
                    .Include(p => p.User)
                    .Include(p => p.Course)
                    .OrderBy(p => p.CreatedAt)
                    .ToListAsync();
                return PartialView("_PaymentsAdmin", pending1);
            }

            // Check capacity if MaxStudents specified
            var course = payment.Course;
            if (course.MaxStudents.HasValue)
            {
                var occupied = await _context.Enrollments
                    .Where(e => e.CourseId == course.Id && e.Status != "Cancelled")
                    .CountAsync();

                if (occupied >= course.MaxStudents.Value)
                {
                    TempData["Errors"] = "Cannot approve: course is full.";
                    var pending2 = await _context.Payments
                        .Include(p => p.User)
                        .Include(p => p.Course)
                        .OrderBy(p => p.CreatedAt)
                        .ToListAsync();
                    return PartialView("_PaymentsAdmin", pending2);
                }
            }

            // Mark payment as success
            payment.Status = "Success";
            payment.IsApproved = true;
            payment.CompletedAt = DateTime.UtcNow;
            _context.Payments.Update(payment);

            // Create Enrollment (admin approved -> make Active)
            var enrollment = new Enrollment
            {
                StudentId = payment.UserId,
                CourseId = payment.CourseId,
                Status = "Active",   // варіант: "Pending" якщо бажаєш ще додаткові кроки
                PaidAmount = payment.Amount,
                EnrollmentDate = DateTime.UtcNow,
                StartDate = null,
                EndDate = null
            };

            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Payment #{payment.Id} approved. Enrollment created for user {payment.UserId}.";

            var pending = await _context.Payments
                .Include(p => p.User)
                .Include(p => p.Course)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();

            return PartialView("_PaymentsAdmin", pending);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectPayment(int paymentId, string? reason)
        {
            var payment = await _context.Payments
                .Include(p => p.User)
                .Include(p => p.Course)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                TempData["Errors"] = "Payment not found.";
                var pendingN = await _context.Payments
                    .Include(p => p.User)
                    .Include(p => p.Course)
                    .OrderBy(p => p.CreatedAt)
                    .ToListAsync();
                return PartialView("_PaymentsAdmin", pendingN);
            }

            payment.Status = "Failed";
            payment.CompletedAt = DateTime.UtcNow;
            payment.RejectReason = reason;
            payment.IsApproved = false;
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();

            // TODO: optional — send notification/email to payment.UserId with reason

            TempData["Success"] = $"Payment #{payment.Id} rejected.";
            var pending = await _context.Payments
                .Include(p => p.User)
                .Include(p => p.Course)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();
            return PartialView("_PaymentsAdmin", pending);
        }


        public IActionResult Enrollments()
        {
            var students = _context.Users
                .Where(u => u.Enrollments.Any())
                .ToList();

            return PartialView("_EnrollmentStudents", students);
        }

        public IActionResult EnrollmentHistory(string id)
        {
            var enrolls = _context.Enrollments
                .Include(e => e.Course)
                .Where(e => e.StudentId == id)
                .OrderByDescending(e => e.StartDate)
                .ToList();

            ViewBag.Student = _context.Users.Find(id);

            return PartialView("_EnrollmentHistory", enrolls);
        }

        public IActionResult Refunds()
        {
            var requests = _context.RefundRequests
                .Include(r => r.Enrollment)
                .ThenInclude(e => e.Student)
                .Include(r => r.Enrollment.Course)
                .ToList();

            return PartialView("_RefundsAdmin", requests);
        }

        [HttpPost]
        public IActionResult ApproveRefund(int id)
        {
            var r = _context.RefundRequests.Find(id);
            r.IsApproved = true;
            _context.SaveChanges();
            return RedirectToAction("Refunds");
        }

        [HttpPost]
        public IActionResult RejectRefund(int id, string reason)
        {
            var r = _context.RefundRequests.Find(id);
            r.IsApproved = false;
            r.RejectReason = reason;
            _context.SaveChanges();
            return RedirectToAction("Refunds");
        }

    }
}