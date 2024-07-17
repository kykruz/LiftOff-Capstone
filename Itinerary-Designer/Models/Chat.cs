namespace Trips.Models
{
    public class Chat
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }
        public string UserId { get; set; }
        public ChatUser ChatUser { get; set; }
        public bool IsAdminResponse { get; set; }
        public int? OriginalChatId { get; set; }

        public Chat OriginalChat { get; set; } 
    }
}