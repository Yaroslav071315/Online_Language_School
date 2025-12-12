using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Language_School.Data;
using Online_Language_School.Models;
using Online_Language_School.ViewModels;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Online_Language_School.Controllers
{
    public class TeacherController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TeacherController(ApplicationDbContext context)
        {
            _context = context;
        }

        private Course GetCourseForTeacherOrThrow(int courseId, string teacherId)
        {
            var course = _context.Courses
                .Include(c => c.Lessons)
                    .ThenInclude(l => l.LessonMaterials)
                .Include(c => c.Lessons)
                    .ThenInclude(l => l.Tests)
                .FirstOrDefault(c => c.Id == courseId);

            if (course == null) throw new Exception("CourseNotFound");
            if (course.TeacherId != teacherId) throw new UnauthorizedAccessException();
            return course;
        }

        private bool IsEditable(Course course) => course.Status == CourseStatus.Draft;

        // GET: /Teacher/Office
        [HttpGet]
        public IActionResult Office()
        {
            return View("TeacherOffice");
        }

        // GET: /Teacher/Schedule
        public async Task<IActionResult> Schedule()
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            // Завантажуємо всі курси викладача (для назв у таблиці)
            var teacherCourses = await _context.Courses
                .Where(c => c.TeacherId == userId)
                .Where(c => c.Status == CourseStatus.Active)
                .ToListAsync();

            var plannedLessons = await _context.PlannedLessons
                .Where(p => p.Lesson.Course.TeacherId == userId)
                .ToListAsync();

            // Завантажуємо всі уроки викладача
            // Завантажуємо всі уроки викладача, які ще не мають PlannedLesson
            var lessons = await _context.Lessons
                .Where(l => l.Course.TeacherId == userId)
                .Where(l => !_context.PlannedLessons.Any(p => p.LessonId == l.Id))
                .Include(l => l.Course)
                .OrderBy(l => l.OrderNumber)
                .ToListAsync();

            // Для селекту в EditLesson
            ViewBag.ActiveCourses = teacherCourses;
            ViewBag.Lessons = lessons;

            return PartialView("_ScheduleTeacher", plannedLessons);
        }


        [HttpGet]
        public IActionResult Materials()
        {
            var materials = _context.LessonMaterials
                .Include(m => m.Lesson)
                .OrderByDescending(m => m.UploadedAt)
                .ToList();

            ViewBag.Lessons = _context.Lessons
                .OrderBy(l => l.OrderNumber)
                .ToList();

            return PartialView("_MaterialsTeacher", materials);
        }

        [HttpGet]
        public IActionResult Homeworks() => PartialView("_HomeworksTeacher");



        /////////////////////////////
        // LESSONS
        /////////////////////////////

        // POST: /Teacher/CreateLesson
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateLesson(int courseId, string title, string description, int orderNumber, int durationMinutes)
        {
            var teacherId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(teacherId))
                return RedirectToAction("Login", "Account");

            Course course;
            try
            {
                course = GetCourseForTeacherOrThrow(courseId, teacherId);
            }
            catch (Exception ex) when (ex.Message == "CourseNotFound")
            {
                TempData["Errors"] = "Course not found.";
                return RedirectToAction("Office");
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }

            // Якщо курс не редагований — повертаємо поточний стан курсу у _EditCourse (з помилкою у TempData)
            if (!IsEditable(course))
            {
                TempData["Errors"] = "Course can be edited only in Draft state.";
                return PartialView("_EditCourse", course);
            }

            // Валідація заголовка — повертаємо той самий partial з повідомленням
            if (string.IsNullOrWhiteSpace(title))
            {
                TempData["Errors"] = "Title required.";
                return PartialView("_EditCourse", course);
            }

            var lesson = new Lesson
            {
                Title = title,
                Description = description,
                OrderNumber = orderNumber,
                DurationMinutes = durationMinutes,
                CourseId = courseId
            };

            _context.Lessons.Add(lesson);
            _context.SaveChanges();

            // Після успішного створення уроку — оновлюємо курс і повертаємо _EditCourse із актуальними даними
            var updatedCourse = GetCourseForTeacherOrThrow(courseId, teacherId);
            return PartialView("_EditCourse", updatedCourse);
        }

        // POST: /Teacher/EditLesson
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditLesson(int id, string title, string description, int orderNumber, int durationMinutes)
        {
            var teacherId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(teacherId))
                return RedirectToAction("Login", "Account");

            var lesson = _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefault(l => l.Id == id);

            if (lesson == null)
            {
                TempData["Errors"] = "Lesson not found.";
                return RedirectToAction("Office");
            }

            if (lesson.Course.TeacherId != teacherId)
                return Forbid();

            if (!IsEditable(lesson.Course))
            {
                TempData["Errors"] = "Course can be edited only in Draft state.";
                return PartialView("_EditCourse", lesson.Course);
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                TempData["Errors"] = "Title required.";
                return PartialView("_EditCourse", lesson.Course);
            }

            // Оновлюємо дані уроку
            lesson.Title = title;
            lesson.Description = description;
            lesson.OrderNumber = orderNumber;
            lesson.DurationMinutes = durationMinutes;

            _context.Lessons.Update(lesson);
            _context.SaveChanges();

            // Повертаємо оновлений курс у PartialView
            var updatedCourse = _context.Courses
                .Include(c => c.Lessons)
                .FirstOrDefault(c => c.Id == lesson.CourseId);

            return PartialView("_EditCourse", updatedCourse);
        }


        // POST: /Teacher/DeleteLesson
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteLesson(int id)
        {
            var teacherId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(teacherId))
                return RedirectToAction("Login", "Account");

            var lesson = _context.Lessons
                .Include(l => l.Course)
                .Include(l => l.LessonMaterials)
                .Include(l => l.Tests)
                .FirstOrDefault(l => l.Id == id);

            if (lesson == null)
            {
                TempData["Errors"] = "Lesson not found.";
                return RedirectToAction("Office");
            }

            if (lesson.Course.TeacherId != teacherId)
                return Forbid();

            if (!IsEditable(lesson.Course))
            {
                TempData["Errors"] = "Course can be edited only in Draft state.";
                return PartialView("_EditCourse", lesson.Course);
            }

            // Видаляємо залежні сутності (якщо каскад не налаштований)
            if (lesson.LessonMaterials != null && lesson.LessonMaterials.Any())
                _context.LessonMaterials.RemoveRange(lesson.LessonMaterials);

            if (lesson.Tests != null && lesson.Tests.Any())
                _context.Tests.RemoveRange(lesson.Tests);

            _context.Lessons.Remove(lesson);
            _context.SaveChanges();

            // Після видалення дістаємо оновлений курс і повертаємо його у PartialView
            var updatedCourse = _context.Courses
                .Include(c => c.Lessons)
                .FirstOrDefault(c => c.Id == lesson.CourseId);

            return PartialView("_EditCourse", updatedCourse);
        }




        /////////////////////////////
        // MATERIALS
        /////////////////////////////

        // POST: /Teacher/CreateMaterial
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateMaterial(int lessonId, string title, string fileUrl, string fileType, long fileSize, DateTime? expiryDate)
        {
            var teacherId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(teacherId))
                return RedirectToAction("Login", "Account");

            var lesson = _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefault(l => l.Id == lessonId);

            if (lesson == null)
            {
                TempData["Errors"] = "Lesson not found.";
                return RedirectToAction("Office");
            }

            if (lesson.Course.TeacherId != teacherId)
                return Forbid();

            if (!IsEditable(lesson.Course))
            {
                TempData["Errors"] = "Course can be edited only in Draft state.";
                return PartialView("_EditCourse", lesson.Course);
            }

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(fileUrl) || string.IsNullOrWhiteSpace(fileType))
            {
                TempData["Errors"] = "Required fields missing.";
                return PartialView("_EditCourse", lesson.Course);
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

            // Після створення матеріалу дістаємо оновлений курс і повертаємо його у PartialView
            var updatedCourse = _context.Courses
                .Include(c => c.Lessons)
                    .ThenInclude(l => l.LessonMaterials)
                .Include(c => c.Lessons)
                    .ThenInclude(l => l.Tests)
                .FirstOrDefault(c => c.Id == lesson.CourseId);

            return PartialView("_EditCourse", updatedCourse);
        }

        // POST: /Teacher/EditMaterial
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditMaterial(int id, string title, string fileUrl, string fileType, long fileSize, DateTime? expiryDate)
        {
            var teacherId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(teacherId))
                return RedirectToAction("Login", "Account");

            var material = _context.LessonMaterials
                .Include(m => m.Lesson)
                    .ThenInclude(l => l.Course)
                .FirstOrDefault(m => m.Id == id);

            if (material == null)
            {
                TempData["Errors"] = "Material not found.";
                return RedirectToAction("Office");
            }

            if (material.Lesson.Course.TeacherId != teacherId)
                return Forbid();

            if (!IsEditable(material.Lesson.Course))
            {
                TempData["Errors"] = "Course can be edited only in Draft state.";
                return PartialView("_EditCourse", material.Lesson.Course);
            }

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(fileUrl) || string.IsNullOrWhiteSpace(fileType))
            {
                TempData["Errors"] = "Required fields missing.";
                return PartialView("_EditCourse", material.Lesson.Course);
            }

            // Оновлюємо дані матеріалу
            material.Title = title;
            material.FileUrl = fileUrl;
            material.FileType = fileType;
            material.FileSize = fileSize;
            material.ExpiryDate = expiryDate;

            _context.LessonMaterials.Update(material);
            _context.SaveChanges();

            // Після збереження дістаємо оновлений курс і повертаємо його у PartialView
            var updatedCourse = _context.Courses
                .Include(c => c.Lessons)
                    .ThenInclude(l => l.LessonMaterials)
                .Include(c => c.Lessons)
                    .ThenInclude(l => l.Tests)
                .FirstOrDefault(c => c.Id == material.Lesson.CourseId);

            return PartialView("_EditCourse", updatedCourse);
        }

        // POST: /Teacher/DeleteMaterial
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteMaterial(int id)
        {
            var teacherId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(teacherId))
                return RedirectToAction("Login", "Account");

            var material = _context.LessonMaterials
                .Include(m => m.Lesson)
                    .ThenInclude(l => l.Course)
                .FirstOrDefault(m => m.Id == id);

            if (material == null)
            {
                TempData["Errors"] = "Material not found.";
                return RedirectToAction("Office");
            }

            if (material.Lesson.Course.TeacherId != teacherId)
                return Forbid();

            if (!IsEditable(material.Lesson.Course))
            {
                TempData["Errors"] = "Course can be edited only in Draft state.";
                return PartialView("_EditCourse", material.Lesson.Course);
            }

            _context.LessonMaterials.Remove(material);
            _context.SaveChanges();

            // Після видалення дістаємо оновлений курс і повертаємо його у PartialView
            var updatedCourse = _context.Courses
                .Include(c => c.Lessons)
                    .ThenInclude(l => l.LessonMaterials)
                .Include(c => c.Lessons)
                    .ThenInclude(l => l.Tests)
                .FirstOrDefault(c => c.Id == material.Lesson.CourseId);

            return PartialView("_EditCourse", updatedCourse);
        }




        /////////////////////////////
        // TESTS (заглушки)
        /////////////////////////////

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
        [ValidateAntiForgeryToken]
        public IActionResult CreateTest(int lessonId, string title, string description)
        {
            var teacherId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(teacherId))
                return RedirectToAction("Login", "Account");

            var lesson = _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefault(l => l.Id == lessonId);

            if (lesson == null)
            {
                TempData["Errors"] = "Lesson not found.";
                return RedirectToAction("Office");
            }

            if (lesson.Course.TeacherId != teacherId)
                return Forbid();

            if (!IsEditable(lesson.Course))
            {
                TempData["Errors"] = "Course can be edited only in Draft state.";
                return PartialView("_EditCourse", lesson.Course);
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                TempData["Errors"] = "Title required.";
                return PartialView("_EditCourse", lesson.Course);
            }

            var test = new Test
            {
                Title = title,
                Description = description,
                TimeLimit = 0,
                PassingScore = 0,
                IsActive = false,
                LessonId = lessonId
            };

            _context.Tests.Add(test);
            _context.SaveChanges();

            // Після створення тесту дістаємо оновлений курс з уроками і тестами
            var updatedCourse = _context.Courses
                .Include(c => c.Lessons)
                    .ThenInclude(l => l.LessonMaterials)
                .Include(c => c.Lessons)
                    .ThenInclude(l => l.Tests)
                .FirstOrDefault(c => c.Id == lesson.CourseId);

            return PartialView("_EditCourse", updatedCourse);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditTest(int id, string title, string description)
        {
            var teacherId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(teacherId))
                return RedirectToAction("Login", "Account");

            var test = _context.Tests
                .Include(t => t.Lesson)
                    .ThenInclude(l => l.Course)
                .FirstOrDefault(t => t.Id == id);

            if (test == null)
            {
                TempData["Errors"] = "Test not found.";
                return RedirectToAction("Office");
            }

            if (test.Lesson.Course.TeacherId != teacherId)
                return Forbid();

            if (!IsEditable(test.Lesson.Course))
            {
                TempData["Errors"] = "Course can be edited only in Draft state.";
                return PartialView("_EditCourse", test.Lesson.Course);
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                TempData["Errors"] = "Title required.";
                return PartialView("_EditCourse", test.Lesson.Course);
            }

            // Оновлюємо дані тесту
            test.Title = title;
            test.Description = description;

            _context.Tests.Update(test);
            _context.SaveChanges();

            // Після збереження дістаємо оновлений курс з уроками і тестами
            var updatedCourse = _context.Courses
                .Include(c => c.Lessons)
                    .ThenInclude(l => l.LessonMaterials)
                .Include(c => c.Lessons)
                    .ThenInclude(l => l.Tests)
                .FirstOrDefault(c => c.Id == test.Lesson.CourseId);

            return PartialView("_EditCourse", updatedCourse);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteTest(int id)
        {
            var teacherId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(teacherId))
                return RedirectToAction("Login", "Account");

            var test = _context.Tests
                .Include(t => t.Lesson)
                    .ThenInclude(l => l.Course)
                .FirstOrDefault(t => t.Id == id);

            if (test == null)
            {
                TempData["Errors"] = "Test not found.";
                return RedirectToAction("Office");
            }

            if (test.Lesson.Course.TeacherId != teacherId)
                return Forbid();

            if (!IsEditable(test.Lesson.Course))
            {
                TempData["Errors"] = "Course can be edited only in Draft state.";
                return PartialView("_EditCourse", test.Lesson.Course);
            }

            _context.Tests.Remove(test);
            _context.SaveChanges();

            // Після видалення дістаємо оновлений курс з уроками і тестами
            var updatedCourse = _context.Courses
                .Include(c => c.Lessons)
                    .ThenInclude(l => l.LessonMaterials)
                .Include(c => c.Lessons)
                    .ThenInclude(l => l.Tests)
                .FirstOrDefault(c => c.Id == test.Lesson.CourseId);

            return PartialView("_EditCourse", updatedCourse);
        }

        [HttpGet]
        public IActionResult Courses()
        {
            var teacherId = HttpContext.Session.GetString("UserId");

            var courses = _context.Courses
                .Where(c => c.TeacherId == teacherId)
                .Include(c => c.Lessons)
                    .ThenInclude(l => l.LessonMaterials)
                .Include(c => c.Lessons)
                    .ThenInclude(l => l.Tests)
                .ToList();

            return PartialView("_CoursesTeacher", courses);
        }

        [HttpPost]
        public IActionResult CreateCourse(CreateCourseViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var teacherId = HttpContext.Session.GetString("UserId");

            var course = new Course
            {
                Title = model.Title,
                Language = model.Language,
                Level = model.Level,
                Format = model.Format,
                Price = model.Price,
                MaxStudents = model.MaxStudents,
                ImageUrl = model.ImageUrl,
                Description = model.Description,
                TeacherId = teacherId,
                Status = CourseStatus.Draft
            };

            _context.Courses.Add(course);
            _context.SaveChanges();

            return RedirectToAction("Office");
        }


        [HttpPost]
        public IActionResult SubmitCourse(int id)
        {
            var teacherId = HttpContext.Session.GetString("UserId");
            if (teacherId == null) return RedirectToAction("Login", "Account");

            var course = _context.Courses.FirstOrDefault(c => c.Id == id && c.TeacherId == teacherId);
            if (course == null) return NotFound();

            course.Status = CourseStatus.Submitted;
            _context.SaveChanges();

            //return RedirectToAction("Office");
            return PartialView("_EditCourse", course);
        }

        // GET: /Teacher/EditCourse/5
        [HttpGet]
        public IActionResult EditCourse(int id)
        {
            var teacherId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(teacherId))
            {
                TempData["Errors"] = "Not logged in.";
                return RedirectToAction("Login", "Account");
            }

            var course = _context.Courses
                .Include(c => c.Lessons)
                    .ThenInclude(l => l.LessonMaterials)
                .Include(c => c.Lessons)
                    .ThenInclude(l => l.Tests)
                .FirstOrDefault(c => c.Id == id);


            if (course == null)
                return NotFound();

            if (course.TeacherId != teacherId)
                return Forbid();

            // Якщо курс не в Draft — заборонити редагування (показати лише перегляд)
            if (course.Status != CourseStatus.Draft)
            {
                // можна повернути часткове в'юшку тільки для перегляду
                return PartialView("_EditCourseReadOnly", course);
            }

            return PartialView("_EditCourse", course);
        }

        // POST: /Teacher/EditCourse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCourse(int id, CreateCourseViewModel model)
        {
            var teacherId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(teacherId))
            {
                TempData["Errors"] = "Not logged in.";
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                // Повертаємо BadRequest з валідатором або можна повернути часткове в'юшко з помилками
                TempData["Errors"] = "Form contains errors.";
                return RedirectToAction("Office");
            }

            var course = _context.Courses.FirstOrDefault(c => c.Id == id);
            if (course == null)
            {
                TempData["Errors"] = "Course not found.";
                return RedirectToAction("Office");
            }

            if (course.TeacherId != teacherId)
            {
                return Forbid();
            }

            if (course.Status != CourseStatus.Draft)
            {
                // не дозволяємо редагувати курс якщо не Draft
                TempData["Errors"] = "Course can be edited only in Draft state.";
                return RedirectToAction("Office");
            }

            // Оновлюємо поля
            course.Title = model.Title;
            course.Language = model.Language;
            course.Level = model.Level;
            course.Format = model.Format;
            course.Price = model.Price;
            course.MaxStudents = model.MaxStudents;
            course.ImageUrl = string.IsNullOrWhiteSpace(model.ImageUrl) ? null : model.ImageUrl;
            course.Description = model.Description;

            _context.Courses.Update(course);
            _context.SaveChanges();

            TempData["Success"] = "Course updated.";
            return RedirectToAction("Office");
        }

        [HttpPost]
        public IActionResult StartEnrollment(int id)
        {
            var course = _context.Courses.Find(id);
            if (course == null) return NotFound();

            course.Status = CourseStatus.Enrollment;
            _context.Courses.Update(course);
            _context.SaveChanges();

            return PartialView("_EditCourseReadOnly", course);
        }

        [HttpPost]
        public IActionResult StartTeaching(int id)
        {
            var course = _context.Courses.Find(id);
            if (course == null) return NotFound();

            course.Status = CourseStatus.Active;
            _context.Courses.Update(course);
            _context.SaveChanges();

            return PartialView("_EditCourseReadOnly", course);
        }

        [HttpPost]
        public IActionResult CancelEnrollment(int id)
        {
            var course = _context.Courses.Find(id);
            if(course == null) return NotFound();
            //RefundStudents(id); // returning costs

            course.Status = CourseStatus.Approved;
            _context.Courses.Update(course);
            _context.SaveChanges();

            return PartialView("_EditCourseReadOnly", course);
        }

        [HttpPost]
        public IActionResult CancelTeaching(int id)
        {
            var course = _context.Courses.Find(id);
            if (course == null) return NotFound();
            //RefundStudents(id); // returning costs

            course.Status = CourseStatus.Enrollment;
            _context.Courses.Update(course);
            _context.SaveChanges();

            return PartialView("_EditCourseReadOnly", course);
        }

        [HttpGet]
        public IActionResult GetLessonsForCourse(int id)
        {
            var lessons = _context.Lessons
                .Where(l => l.CourseId == id)
                .OrderBy(l => l.OrderNumber)
                .Select(l => new { l.Id, l.Title, l.OrderNumber })
                .ToList();

            return Json(lessons);
        }

        [HttpGet]
        public IActionResult GetStudentsForCourse(int id)
        {
            var course = _context.Courses.Find(id);
            if (course == null) return Json(new List<object>());

            if (course.Format == "Group")
                return Json(new List<object>());

            var students = _context.Enrollments
                .Where(e => e.Status == "Active")
                .Where(e => e.CourseId == id)
                .Select(e => new { e.Student.Id, Name = e.Student.FirstName + " " + e.Student.LastName })
                .ToList();

            return Json(students);
        }

        [HttpPost]
        public IActionResult CreatePlannedLesson(int CourseId, int lessonId, DateTime startTime, string? studentId)
        {
            var teacherId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(teacherId))
                return RedirectToAction("Login", "Account");

            var course = _context.Courses
                .Include(c => c.Lessons)
                .Include(c => c.Enrollments)
                .FirstOrDefault(c => c.Id == CourseId);

            if (course == null)
                return NotFound();

            if (course.TeacherId != teacherId)
                return Forbid();

            var lesson = _context.Lessons.FirstOrDefault(l => l.Id == lessonId);

            if (lesson == null)
                return NotFound();

            var duration = lesson.DurationMinutes;
            var endTime = startTime.AddMinutes(duration);

            //---------------------------------------------------------
            // 1. ВИЗНАЧАЄМО СПИСОК СТУДЕНТІВ ЯКІ ПОВИННІ БРАТИ УРОК
            //---------------------------------------------------------

            List<string> targetStudents = new();

            if (course.Format == "Group")
            {
                // беремо лише тих, хто реально навчається на курсі
                targetStudents = course.Enrollments
                    .Where(e => e.Status == "Active")
                    .Select(e => e.StudentId)
                    .ToList();
            }
            else // Individual
            {
                if (string.IsNullOrEmpty(studentId))
                {
                    TempData["Errors"] = "Student is required for individual lessons.";
                    return RedirectToAction("Schedule");
                }

                targetStudents.Add(studentId);
            }

            //---------------------------------------------------------
            // 2. ПЕРЕВІРКА КОНФЛІКТІВ ЧАСУ (ВЧИТЕЛЬ + СТУДЕНТ(И))
            //---------------------------------------------------------

            var conflicts = _context.PlannedLessons
                .Include(p => p.Lesson)
                .Where(p =>
                    // конфлікт якщо
                    // a) вчитель зайнятий у цей час
                    p.Lesson.Course.TeacherId == teacherId ||

                    // b) студент зайнятий у цей час
                    (p.StudentId != null && targetStudents.Contains(p.StudentId))
                )
                .Where(p =>
                    (
                        (startTime >= p.StartTime && startTime < p.EndTime) ||
                        (endTime > p.StartTime && endTime <= p.EndTime)
                    )
                )
                .Any();

            if (conflicts)
            {
                TempData["Errors"] = "Time slot is already occupied for the teacher or a student.";
                return RedirectToAction("Schedule");
            }

            //---------------------------------------------------------
            // 3. ПЕРЕВІРКА ПОРЯДКОВОГО НОМЕРА УРОКІВ
            //---------------------------------------------------------

            var existing = _context.PlannedLessons
                .Include(p => p.Lesson)
                .Where(p => p.StudentId != null && targetStudents.Contains(p.StudentId))
                .OrderBy(p => p.StartTime)
                .ToList();

            foreach (var pl in existing)
            {
                // урок, який ставиться пізніше — не може мати менший order
                if (lesson.OrderNumber < pl.Lesson.OrderNumber && startTime > pl.StartTime)
                {
                    TempData["Errors"] = "Lesson order is incorrect (this lesson has lower order but is placed later).";
                    return RedirectToAction("Schedule");
                }

                // урок, який ставиться раніше — не може мати більший order
                if (lesson.OrderNumber > pl.Lesson.OrderNumber && startTime < pl.StartTime)
                {
                    TempData["Errors"] = "Lesson order is incorrect (this lesson has higher order but is placed earlier).";
                    return RedirectToAction("Schedule");
                }
            }

            //---------------------------------------------------------
            // 4. СТВОРЕННЯ ЗАПИСУ
            //---------------------------------------------------------

            var planned = new PlannedLesson
            {
                LessonId = lessonId,
                StudentId = course.Format == "Individual" ? studentId : null,
                StartTime = startTime,
                DurationMinutes = lesson.DurationMinutes
            };

            _context.PlannedLessons.Add(planned);
            _context.SaveChanges();

            return RedirectToAction("Schedule");
        }

        [HttpGet]
        public async Task<IActionResult> GetLessonsByCourse(int id)
        {
            var lessons = await _context.Lessons
                .Where(l => l.CourseId == id)
                .OrderBy(l => l.OrderNumber)
                .ToListAsync();

            var sb = new System.Text.StringBuilder();
            sb.Append("<option value=\"\">-- Select lesson --</option>");

            foreach (var l in lessons)
                sb.Append($"<option value=\"{l.Id}\">{l.Title}</option>");

            return Content(sb.ToString(), "text/html");
        }

        [HttpGet]
        public async Task<IActionResult> GetStudentsByCourse(int id)
        {
            var students = await _context.Enrollments
                .Where(e => e.CourseId == id && e.Status == "Active")
                .Select(e => e.Student)
                .ToListAsync();

            var sb = new System.Text.StringBuilder();
            sb.Append("<option value=\"\">-- Select student --</option>");

            foreach (var s in students)
                sb.Append($"<option value=\"{s.Id}\">{s.FirstName} {s.LastName}</option>");

            return Content(sb.ToString(), "text/html");
        }
    }
}