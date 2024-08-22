using Microsoft.AspNetCore.Mvc;
using PhuKienShop.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
[Authorize(Policy = "AdminOnly")]
[Route("admin/messages")]
public class AdminMessagesController : Controller
{
    private readonly PkShopContext _context;
    private readonly IHubContext<ChatHub> _hubContext;
    public AdminMessagesController(PkShopContext context, IHubContext<ChatHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }
    [HttpGet("Chat/GetMessages/{userId}")]
    public async Task<IActionResult> GetMessages(int userId)
    {
        var messages = await _context.Messages
                                     .Include(m => m.Sender)
                                     .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                                     .OrderBy(m => m.SentAt)
                                     .ToListAsync();

        return PartialView("_MessagesPartial", messages);
    }
    [HttpGet("Chat/GetLatestMessages/{userId}/{lastMessageId}")]
    public async Task<IActionResult> GetLatestMessages(int userId, int lastMessageId)
    {
        var newMessages = await _context.Messages
                                        .Where(m => m.SenderId == userId && m.MessageId > lastMessageId)
                                        .OrderBy(m => m.SentAt)
                                        .ToListAsync();

        return PartialView("_MessagesPartial", newMessages);
    }


    // Get the list of users who have messaged the admin and the chat history of a specific user (if selected)
    [HttpGet("{userId?}")]
    public async Task<IActionResult> Index(int? userId)
    {
        var users = await _context.Messages
                                  .Where(m => m.ReceiverId == 1) // Messages to admin
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
        // Loại bỏ các phần tử null (nếu có) trong danh sách
        users = users.Where(u => u != null).ToList();
        var model = new AdminMessagesViewModel
        {
            UserList = users ?? new List<User>(),
            Messages = userMessages,
            SelectedUser = selectedUser
        };

        return View(model);
    }

    // Send a reply to the user
    [HttpPost("Chat/{userId}")]
    public async Task<IActionResult> Reply(int userId, string messageContent)
    {
        //var adminUser = await _context.Users.SingleOrDefaultAsync(u => u.Username == User.Identity.Name);

        var message = new PhuKienShop.Data.Message
        {
            SenderId = 1,
            ReceiverId = userId,
            Content = messageContent,
            SentAt = DateTime.UtcNow
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();
        await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveMessage", "Admin", message.Content);
        // You can also send a separate notification to alert the admin
        //await _hubContext.Clients.User(userId.ToString()).SendAsync("NewMessageNotification", "Admin", message.Content);
        //await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Admin", message.Content);
        return RedirectToAction("Index", new { userId });
    }
}
