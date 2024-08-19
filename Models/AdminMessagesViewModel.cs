using PhuKienShop.Data;
using System.Collections.Generic;

public class AdminMessagesViewModel
{
    public IEnumerable<User> Users { get; set; }
    public IEnumerable<Message> Messages { get; set; }
    public User SelectedUser { get; set; }
}
