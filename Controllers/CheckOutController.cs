using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhuKienShop.Data;
using PhuKienShop.Models;

namespace PhuKienShop.Controllers
{
 
    public class CheckOutController : Controller
	{
		private readonly PkShopContext _context;

		public CheckOutController(PkShopContext context)
		{
			_context = context;
		}

		// GET: CheckOut
		public IActionResult Index()
		{
			// Trả về view checkout
			return View();
		}

		// POST: CheckOut
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Index(CheckoutViewModel model)
		{
			if (ModelState.IsValid)
			{
				// Tạo đơn hàng mới
				var order = new Order
				{
					UserId = model.UserId,
					OrderDate = DateTime.Now,
					TotalAmount = model.TotalAmount,
					Status = "Pending",
					CreatedAt = DateTime.Now
				};

				_context.Orders.Add(order);
				await _context.SaveChangesAsync();

				// Thêm chi tiết đơn hàng
				foreach (var item in model.OrderDetails)
				{
					var orderDetail = new OrderDetail
					{
						OrderId = order.OrderId,
						ProductId = item.ProductId,
						Quantity = item.Quantity,
						UnitPrice = item.UnitPrice,
						TotalPrice = item.Quantity * item.UnitPrice
					};

					_context.OrderDetails.Add(orderDetail);
				}

				await _context.SaveChangesAsync();

				// Thực hiện các bước sau khi lưu đơn hàng thành công
				// Ví dụ: chuyển hướng đến trang xác nhận đơn hàng
				return RedirectToAction("Confirmation", new { orderId = order.OrderId });
			}

			// Nếu dữ liệu không hợp lệ, hiển thị lại trang checkout với thông báo lỗi
			return View(model);
		}

		// GET: CheckOut/Confirmation
		public async Task<IActionResult> Confirmation(int orderId)
		{
			var order = await _context.Orders
				.Include(o => o.OrderDetails)
				.ThenInclude(od => od.Product)
				.FirstOrDefaultAsync(o => o.OrderId == orderId);

			if (order == null)
			{
				return NotFound();
			}

			return View(order);
		}
	}
}
