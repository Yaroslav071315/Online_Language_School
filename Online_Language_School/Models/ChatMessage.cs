using System;
using System.ComponentModel.DataAnnotations;

namespace Online_Language_School.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }

        [Required]
        public string Message { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; }

        // Foreign Keys
        [Required]
        public string SenderId { get; set; }

        [Required]
        public string ReceiverId { get; set; }

        // Navigation
        public virtual ApplicationUser Sender { get; set; }
        public virtual ApplicationUser Receiver { get; set; }
    }
}