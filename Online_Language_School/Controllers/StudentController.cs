using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Language_School.Data;
using Online_Language_School.Models;
using Online_Language_School.ViewModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Online_Language_School.Controllers
{
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;


        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }

        private CourseSearchViewModel BuildCoursesVm(CourseSearchViewModel vm, string studentId)
        {
            var enrolledCourseIds = _context.Enrollments
                .Where(e => e.StudentId == studentId)
                .Select(e => e.CourseId)
                .ToList();

            var query = _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Lessons)
                .ThenInclude(l => l.LessonMaterials)
                .Include(c => c.Lessons)
                .ThenInclude(l => l.Tests)
                .Where(c =>
                    c.Status == CourseStatus.Enrollment ||
                    enrolledCourseIds.Contains(c.Id))
                .AsQueryable();

            // ФІЛЬТРИ
            if (!string.IsNullOrWhiteSpace(vm.Title))
                query = query.Where(c => c.Title.Contains(vm.Title));
            if (!string.IsNullOrWhiteSpace(vm.Format))
                query = query.Where(c => c.Format == vm.Format);
            if (!string.IsNullOrWhiteSpace(vm.Language))
                query = query.Where(c => c.Language == vm.Language);
            if (!string.IsNullOrWhiteSpace(vm.Level))
                query = query.Where(c => c.Level == vm.Level);
            if (vm.MinPrice.HasValue)
                query = query.Where(c => c.Price >= vm.MinPrice);
            if (vm.MaxPrice.HasValue)
                query = query.Where(c => c.Price <= vm.MaxPrice);
            if (vm.MinStudents.HasValue)
                query = query.Where(c => c.MaxStudents >= vm.MinStudents);
            if (vm.MaxStudents.HasValue)
                query = query.Where(c => c.MaxStudents <= vm.MaxStudents);

            vm.Courses = query.ToList();
            return vm;
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
                return Unauthorized();

            var planned = _context.PlannedLessons
                .Include(p => p.Lesson)
                .Include(p => p.Lesson.Course)
                .Where(p =>
                    p.StudentId == studentId &&
                    p.Lesson.Course.Status == CourseStatus.Active)
                .OrderBy(p => p.StartTime)
                .ToList();

            return PartialView("_ScheduleStudent", planned);
        }


        [HttpGet]
        public IActionResult Materials()
        {
            var studentId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(studentId))
                return Unauthorized();

            var activeCourseIds = _context.Enrollments
                .Where(e => e.StudentId == studentId && e.Status == "Active")
                .Select(e => e.CourseId)
                .ToList();

            var materials = _context.LessonMaterials
                .Include(m => m.Lesson)
                    .ThenInclude(l => l.Course)
                .Where(m =>
                    activeCourseIds.Contains(m.Lesson.CourseId) &&
                    m.Lesson.Course.Status == CourseStatus.Active)
                .OrderByDescending(m => m.UploadedAt)
                .ToList();

            return PartialView("_MaterialsStudent", materials);
        }

        [HttpGet]
        public IActionResult Progress()
        {
            var studentId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(studentId))
                return Unauthorized();

            var activeCourseIds = _context.Enrollments
                .Where(e => e.StudentId == studentId && e.Status == "Active")
                .Select(e => e.CourseId)
                .ToList();

            var tests = _context.Tests
                .Include(t => t.Lesson)
                    .ThenInclude(l => l.Course)
                .Where(t =>
                    activeCourseIds.Contains(t.Lesson.CourseId) &&
                    t.Lesson.Course.Status == CourseStatus.Active)
                .OrderBy(t => t.Lesson.OrderNumber)
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

        public IActionResult Courses(CourseSearchViewModel vm)
        {
            var studentId = HttpContext.Session.GetString("UserId");

            vm = BuildCoursesVm(new CourseSearchViewModel(), studentId);
            return PartialView("_CoursesPartial", vm);
        }

        // Helper: view student's enrollments (optional)
        public IActionResult MyEnrollments()
        {
            var studentId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(studentId)) return RedirectToAction("Login", "Account");

            var enrollments = _context.Enrollments
                .Include(e => e.Course)
                .Where(e => e.StudentId == studentId)
                .ToList();

            return View(enrollments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadReceipt(int courseId, IFormFile receiptFile)
        {
            var studentId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(studentId))
                return RedirectToAction("Login", "Account");

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
            if (course == null)
            {
                TempData["Errors"] = "Course not found.";
                var vmErr = BuildCoursesVm(new CourseSearchViewModel(), studentId);
                return PartialView("_CoursesPartial", vmErr);
            }

            if (receiptFile == null || receiptFile.Length == 0)
            {
                TempData["Errors"] = "Receipt file required.";
                var vmErr = BuildCoursesVm(new CourseSearchViewModel(), studentId);
                return PartialView("_CoursesPartial", vmErr);
            }

            // Save file
            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "receipts");
            Directory.CreateDirectory(uploadsDir);

            var uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(receiptFile.FileName);
            var fullPath = Path.Combine(uploadsDir, uniqueName);

            using (var fs = new FileStream(fullPath, FileMode.Create))
            {
                await receiptFile.CopyToAsync(fs);
            }

            // Якщо вже є pending payment від цього користувача для цього курсу — оновимо його,
            // інакше створимо новий Payment. (Не створюємо Enrollment тут.)
            var existing = await _context.Payments
                .FirstOrDefaultAsync(p => p.UserId == studentId && p.CourseId == courseId && p.Status == "Pending");

            if (existing != null)
            {
                existing.TransactionId = uniqueName;
                existing.CreatedAt = DateTime.UtcNow;
                // залишаємо Amount як було (він має дорівнювати ціні курсу)
                existing.Amount = course.Price;
                _context.Payments.Update(existing);
            }
            else
            {
                var payment = new Payment
                {
                    Amount = course.Price,
                    Currency = "UAH",
                    Status = "Pending",
                    TransactionId = uniqueName,
                    PaymentMethod = null,
                    CreatedAt = DateTime.UtcNow,
                    CompletedAt = null,
                    UserId = studentId,
                    CourseId = courseId,
                    ReceiptImagePath = $"/uploads/receipts/{uniqueName}"
                };
                await _context.Payments.AddAsync(payment);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Receipt uploaded — payment created (Pending). Admin will review.";

            var vm = BuildCoursesVm(new CourseSearchViewModel(), studentId);
            return PartialView("_CoursesPartial", vm);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelEnrollment(int courseId)
        {
            var userId = HttpContext.Session.GetString("UserId");

            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.CourseId == courseId && e.StudentId == userId);

            if (enrollment != null)
            {
                enrollment.Status = "Cancelled";
                await _context.SaveChangesAsync();

                // REFUND LOGIC HERE
            }

            var vm = BuildCoursesVm(new CourseSearchViewModel(), userId);
            return PartialView("_CoursesPartial", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestRefund(int courseId, string Reason)
        {
            var userId = HttpContext.Session.GetString("UserId");

            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.CourseId == courseId && e.StudentId == userId);

            if (enrollment != null)
            {
                enrollment.Status = "PendingRefund";
                await _context.SaveChangesAsync();

                // Save refund request for admins
            }

            var vm = BuildCoursesVm(new CourseSearchViewModel(), userId);
            return PartialView("_CoursesPartial", vm);
        }

    }
}