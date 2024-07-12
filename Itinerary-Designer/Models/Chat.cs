namespace Trips.Models;

public class Chat 
{
    public int Id { get; set; }
    public string? Email {get; set;}
    public string SenderId { get; set; }
    public string RecipientId { get; set; } 
    public string? Message { get; set; }
    public DateTime Date {get; set;}
    public string UserId { get; set; }
    public ChatUser ChatUsers {get; set;}


}