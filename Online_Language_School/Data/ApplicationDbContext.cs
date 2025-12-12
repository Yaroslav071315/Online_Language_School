using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Online_Language_School.Models;

namespace Online_Language_School.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // Основні сутності
        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<BlogPost> News { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<PlannedLesson> PlannedLessons { get; set; }
        public DbSet<LessonMaterial> LessonMaterials { get; set; }

        // Тестування
        public DbSet<Test> Tests { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<TestResult> TestResults { get; set; }

        // Студенти та активності
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<CourseReview> CourseReviews { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<RefundRequest> RefundRequests { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // BlogPost (News) → Admin
            builder.Entity<BlogPost>()
                .HasOne(b => b.Admin)
                .WithMany(u => u.BlogPosts)
                .HasForeignKey(b => b.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            // Course → Lessons
            builder.Entity<Lesson>()
                .HasOne(l => l.Course)
                .WithMany(c => c.Lessons)
                .HasForeignKey(l => l.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Lesson → LessonMaterials
            builder.Entity<LessonMaterial>()
                .HasOne(m => m.Lesson)
                .WithMany(l => l.LessonMaterials)
                .HasForeignKey(m => m.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            // Lesson → Tests
            builder.Entity<Test>()
                .HasOne(t => t.Lesson)
                .WithMany(l => l.Tests)
                .HasForeignKey(t => t.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            // Test → Questions
            builder.Entity<Question>()
                .HasOne(q => q.Test)
                .WithMany(t => t.Questions)
                .HasForeignKey(q => q.TestId)
                .OnDelete(DeleteBehavior.Cascade);

            // Question → Answers
            builder.Entity<Answer>()
                .HasOne(a => a.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Enrollment → Student
            builder.Entity<Enrollment>()
                .HasOne(e => e.Student)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Enrollment → Course
            builder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // CourseReview → Student
            builder.Entity<CourseReview>()
                .HasOne(r => r.Student)
                .WithMany(u => u.CourseReviews)
                .HasForeignKey(r => r.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // CourseReview → Course
            builder.Entity<CourseReview>()
                .HasOne(r => r.Course)
                .WithMany(c => c.Reviews)
                .HasForeignKey(r => r.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // TestResult → Student
            builder.Entity<TestResult>()
                .HasOne(tr => tr.Student)
                .WithMany(u => u.TestResults)
                .HasForeignKey(tr => tr.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // TestResult → Test
            builder.Entity<TestResult>()
                .HasOne(tr => tr.Test)
                .WithMany(t => t.TestResults)
                .HasForeignKey(tr => tr.TestId)
                .OnDelete(DeleteBehavior.Cascade);

            // Payment → User
            builder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Payment → Course
            builder.Entity<Payment>()
                .HasOne(p => p.Course)
                .WithMany(c => c.Payments)
                .HasForeignKey(p => p.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // ChatMessage → Sender/Receiver
            builder.Entity<ChatMessage>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ChatMessage>()
                .HasOne(m => m.Receiver)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}