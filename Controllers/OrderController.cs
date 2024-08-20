using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhuKienShop.Data;

namespace PhuKienShop.Controllers
{
    public class OrderController : Controller
    {
        private readonly PkShopContext _context;

        public OrderController(PkShopContext context)
        {
            _context = context;
        }
        public IActionResult OrderDetails(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product) // Nếu có mối quan hệ với Product
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}
