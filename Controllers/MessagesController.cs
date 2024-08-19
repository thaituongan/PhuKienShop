using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PhuKienShop.Data;
using System.Security.Claims;
[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly PkShopContext _context;
    private readonly IHubContext<ChatHub> _hubContext;

    public MessagesController(PkShopContext context, IHubContext<ChatHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] Message message)
    {
        if (message == null || string.IsNullOrWhiteSpace(message.Content))
        {
            return BadRequest("Invalid message.");
        }

        var username = User.Identity?.Name;
        if (username == null)
        {
            return Unauthorized("User not authenticated.");
        }

        // Get the sender's user ID
        var sender = await _context.Users.SingleOrDefaultAsync(u => u.Username == username);
        if (sender == null)
        {
            return NotFound("Sender not found.");
        }

        // Create the message and save it to the database
        var msg = new PhuKienShop.Data.Message
        {
            SenderId = 1,
            ReceiverId = sender.UserId,
            Content = message.Content,
            SentAt = DateTime.UtcNow

        };

        _context.Messages.Add(msg);
        await _context.SaveChangesAsync();

        // Find all admin users
        var admins = await _context.Users.Where(u => u.Role == "Admin").ToListAsync();

        // Broadcast the message to all admin users
        foreach (var admin in admins)
        {
            await _hubContext.Clients.User(admin.UserId.ToString()).SendAsync("ReceiveMessage", sender.Username, message.Content);
        }

        return Ok();
    }
}
