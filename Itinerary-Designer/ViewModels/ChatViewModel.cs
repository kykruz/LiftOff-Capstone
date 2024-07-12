using Trips.Models;

namespace Trips.ViewModels;

public class ChatViewModel
{
    public string? Username {get; set;}
    public string? Message { get; set; }
    public DateTime Date {get; set;}
    public List<Chat?> ChatLog {get; set;}

    public ChatViewModel ()
    {
        ChatLog = new List<Chat?>();
    }
    public ChatViewModel(List<Chat?> chatLog)
    {
        ChatLog = chatLog;
    }
  
        
}