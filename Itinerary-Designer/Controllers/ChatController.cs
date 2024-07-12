using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Trips.Data;
using Trips.Models;
using Trips.ViewModels;

namespace Trips.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly TripDbContext context;
        private readonly UserManager<ApplicationUser> userManager;
        private const string AdminRole = "Admin"; // Assuming you have roles set up and this is the role name for admin

        public ChatController(TripDbContext _context, UserManager<ApplicationUser> _userManager)
        {
            context = _context;
            userManager = _userManager;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        }

        private string GetCurrentUserEmail()
        {
            return User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        }

        [HttpGet]
        public async Task<IActionResult> Messaging()
        {
            string userId = GetCurrentUserId();
            List<Chat> chatLog = await context.Chats
                .Where(c => c.SenderId == userId || c.RecipientId == userId)
                .ToListAsync();

            ChatViewModel chatViewModel = new ChatViewModel(chatLog);
            return View(chatViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Messaging(ChatViewModel chatViewModel, string recipientId = null)
        {
            if (ModelState.IsValid)
            {
                string senderId = GetCurrentUserId();
                string email = GetCurrentUserEmail();

                // Set recipient ID to admin if the sender is not an admin
                if (!User.IsInRole(AdminRole))
                {
                    recipientId = await userManager.Users
                        .Where(u => userManager.IsInRoleAsync(u, AdminRole).Result)
                        .Select(u => u.Id)
                        .FirstOrDefaultAsync();
                }

                Chat chat = new Chat
                {
                    Email = email,
                    Message = chatViewModel.Message,
                    SenderId = senderId,
                    RecipientId = recipientId,
                    Date = DateTime.Now
                };

                context.Chats.Add(chat);
                await context.SaveChangesAsync();

                return RedirectToAction("Messaging");
            }

            return View(chatViewModel);
        }
    }
}
