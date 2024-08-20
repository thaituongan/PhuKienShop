using PhuKienShop.Data;

public class AdminMessagesViewModel
{
    public IEnumerable<User> UserList { get; set; } = new List<User>(); // Khởi tạo danh sách trống
    public IEnumerable<Message> Messages { get; set; } = new List<Message>(); // Khởi tạo danh sách trống
    public User SelectedUser { get; set; }
}
