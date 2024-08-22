using Microsoft.AspNetCore.SignalR;
using PhuKienShop.Data;
using System.Collections.Concurrent;
using System.Security.Claims;
public class ChatHub : Hub
{
	private readonly PkShopContext _context;

	public ChatHub(PkShopContext context)
	{
		_context = context;
	}

	public async Task SendMessageToAdmin(string user, string message)
	{
		var chatMessage = new Message
		{
			Content = message,
			SentAt = DateTime.Now.AddHours(-7.0),
			SenderId = GetUserIdByUsername(user),
			ReceiverId = GetAdminId(),

		};

		_context.Messages.Add(chatMessage);
		await _context.SaveChangesAsync();
        await Clients.All.SendAsync("ReceiveMessage", user, message);
        // Send the message to all connected admin clients
        //var adminConnections = GetAdminConnections();
		//await Clients.Clients(adminConnections).SendAsync("ReceiveMessage", user, message);
	}

	private int? GetUserIdByUsername(string username)
	{
		var user = _context.Users.SingleOrDefault(u => u.Username == username);
		return user?.UserId;
	}

	private int? GetAdminId()
	{
		var admin = _context.Users.Take(1).SingleOrDefault(u => u.Role == "Admin");
		return admin?.UserId;
	}

	private List<string> GetAdminConnections()
	{
		// This will depend on how you're tracking connections. Assume you have a method to get admin connections.
		// For instance, you might keep a dictionary of connection IDs for each user.
		return ConnectionMapping.GetConnectionsByRole("Admin");
	}
	public override Task OnConnectedAsync()
	{
		var role = Context.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
		if (role != null)
		{
			ConnectionMapping.Add(role, Context.ConnectionId);
		}

		return base.OnConnectedAsync();
	}

	public override Task OnDisconnectedAsync(Exception exception)
	{
		var role = Context.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
		if (role != null)
		{
			ConnectionMapping.Remove(role, Context.ConnectionId);
		}

		return base.OnDisconnectedAsync(exception);
	}

}

public class ConnectionMapping
{
	private static readonly ConcurrentDictionary<string, List<string>> _connectionsByRole = new ConcurrentDictionary<string, List<string>>();

	public static void Add(string role, string connectionId)
	{
		if (!_connectionsByRole.ContainsKey(role))
		{
			_connectionsByRole[role] = new List<string>();
		}

		_connectionsByRole[role].Add(connectionId);
	}

	public static void Remove(string role, string connectionId)
	{
		if (_connectionsByRole.ContainsKey(role))
		{
			_connectionsByRole[role].Remove(connectionId);

			if (_connectionsByRole[role].Count == 0)
			{
				_connectionsByRole.TryRemove(role, out _);
			}
		}
	}

	public static List<string> GetConnectionsByRole(string role)
	{
		if (_connectionsByRole.ContainsKey(role))
		{
			return _connectionsByRole[role];
		}

		return new List<string>();
	}
}
