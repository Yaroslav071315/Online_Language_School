namespace Online_Language_School.ViewModels.Chat
{
    public class ChatMessageViewModel
    {
        public bool IsMine { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
    }
}
