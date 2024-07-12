namespace Trips.Models;

public class Chat 
{
    public int Id { get; set; }
    public string? Username {get; set;}
    public string? Message { get; set; }
    public DateTime Date {get; set;}
    public string UserId { get; set; }
    public ChatUser ChatUsers {get; set;}

    // public Chat(string username)
    // {
    //     Username = username;
    // }

}