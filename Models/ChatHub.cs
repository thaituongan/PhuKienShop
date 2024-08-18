using Microsoft.AspNetCore.SignalR;
using PhuKienShop.Data;

public class ChatHub : Hub
{
    private static readonly List<Message> ChatHistory = new List<Message>();

    public async Task SendMessageToAdmin(string user, string message)
    {
        var chatMessage = new Message
        {
            Content = message,
            SentAt = DateTime.Now
        };
        ChatHistory.Add(chatMessage);

        // Send the message to all clients with the "admin" role
        //await Clients.Users("1").SendAsync("ReceiveMessage", user, message);
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    public async Task GetChatHistory()
    {
        foreach (var chatMessage in ChatHistory)
        {
            await Clients.Caller.SendAsync("ReceiveMessage", chatMessage.Sender, chatMessage.Content);
        }
    }
}

