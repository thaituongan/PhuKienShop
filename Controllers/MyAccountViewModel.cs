
using PhuKienShop.Data;

public class MyAccountViewModel
{
    public User User { get; set; }
    public IEnumerable<Order> Orders { get; set; }
    public string UserPhotoUrl { get; set; }
}
