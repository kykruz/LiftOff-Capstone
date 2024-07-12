using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.EntityFrameworkCore;
using Trips.Data;
using Trips.Models;
using Trips.ViewModels;
// 
namespace Trips.Controllers;

[Authorize]
public class ChatController : Controller
{
  private TripDbContext context;

private string GetCurrentUserId()
{
 return User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
}
private string GetCurrentUserEmail()
{
    return User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
}

  public ChatController(TripDbContext _context)
  {
    context = _context;
  }

[HttpGet]
    public async Task<IActionResult> Messaging()
    {
        string userId = GetCurrentUserId();
        List<Chat> chatLog = await context.Chats
            .Where(c => c.UserId == userId)
            .ToListAsync();
        ChatViewModel chatViewModel = new ChatViewModel(chatLog);
        return View(chatViewModel);
    }
        
[HttpPost]
  public async Task<IActionResult> Messaging(ChatViewModel chatViewModel)
  {
    if(ModelState.IsValid)
    {
       string userId = GetCurrentUserId();
       string email = GetCurrentUserEmail();

    Chat chat = new Chat
    {
        Email = email,
        Message = chatViewModel.Message,
        UserId = userId,
        Date = DateTime.Now
    };
    context.Chats.Add(chat);
    await context.SaveChangesAsync();

    return RedirectToAction("Messaging");
    }

    return View(chatViewModel);
  }
        
}