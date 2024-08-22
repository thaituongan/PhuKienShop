using Microsoft.AspNetCore.Authorization;
using PhuKienShop.Data;

namespace PhuKienShop.Controllers
{
    [Authorize(Policy = "UserOnly")]
    public class ConfirmationViewModel
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderDetailViewModel> OrderDetails { get; set; } = new List<OrderDetailViewModel>();

        public User User { get; set; }
    }

    public class OrderDetailViewModel
    {
        public Product Product { get; set; }  // Thêm thuộc tính Product
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

}
