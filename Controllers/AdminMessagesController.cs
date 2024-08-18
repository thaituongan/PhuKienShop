using Microsoft.AspNetCore.Mvc;
using PhuKienShop.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
[Route("admin/messages")]
public class AdminMessagesController : Controller
{
    private readonly PkShopContext _context;

    public AdminMessagesController(PkShopContext context)
    {
        _context = context;
    }

    // Get the list of users who have messaged the admin and the chat history of a specific user (if selected)
    [HttpGet("{userId?}")]
    public async Task<IActionResult> Index(int? userId)
    {
        var users = await _context.Messages
                                  .Where(m => m.ReceiverId == null) // Messages to admin
                                  .Select(m => m.Sender)
                                  .Distinct()
                                  .ToListAsync();

        var userMessages = new List<Message>();
        User selectedUser = null;

        if (userId.HasValue)
        {
            userMessages = await _context.Messages
                                         .Include(m => m.Sender)
                                         .Where(m => m.SenderId == userId.Value || m.ReceiverId == userId.Value)
                                         .OrderBy(m => m.SentAt)
                                         .ToListAsync();

            selectedUser = await _context.Users.FindAsync(userId.Value);
        }

        var model = new AdminMessagesViewModel
        {
            Users = users,
            Messages = userMessages,
            SelectedUser = selectedUser
        };

        return View(model);
    }

    // Send a reply to the user
    [HttpPost("Chat/{userId}")]
    public async Task<IActionResult> Reply(int userId, string messageContent)
    {
        var adminUser = await _context.Users.SingleOrDefaultAsync(u => u.Username == User.Identity.Name);

        var message = new Message
        {
            SenderId = adminUser.UserId,
            ReceiverId = userId,
            Content = messageContent,
            SentAt = DateTime.UtcNow
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", new { userId });
    }
}
