using System;
using System.ComponentModel.DataAnnotations;

namespace Online_Language_School.Models
{
    public class Payment
    {
        public int Id { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string Currency { get; set; } = "UAH";

        [Required]
        public string Status { get; set; } // Pending, Success, Failed, Refunded

        public string TransactionId { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        // Foreign Keys
        [Required]
        public string UserId { get; set; }
        public int EnrollmentId { get; set; }

        // Navigation
        public virtual ApplicationUser User { get; set; }
        public virtual Enrollment Enrollment { get; set; }
    }
}