using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhuKienShop.Data;
using PhuKienShop.Models;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Linq;
using System.Threading.Tasks;
using static PhuKienShop.Models.CheckoutViewModel;
using Microsoft.AspNetCore.Authorization;

namespace PhuKienShop.Controllers
{
    [Authorize(Policy = "UserOnly")]
    public class CheckOutController : Controller
    {
        private readonly PkShopContext _context;

        public CheckOutController(PkShopContext context)
        {
            _context = context;
        }

        // Lấy giỏ hàng từ session
        private CartModel GetCart()
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(cartJson))
            {
                return new CartModel();
            }
            try
            {
                return JsonSerializer.Deserialize<CartModel>(cartJson) ?? new CartModel();
            }
            catch (JsonException)
            {
                // Log exception or handle it as needed
                return new CartModel();
            }
        }

        // Lưu giỏ hàng vào session
        private void SaveCart(CartModel cart)
        {
            var cartJson = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString("Cart", cartJson);
        }

        [HttpGet]
        public IActionResult Index()
        {
            var cart = GetCart();
            if (!cart.CartProducts.Any())
            {
                // Redirect hoặc thông báo giỏ hàng trống nếu không có sản phẩm
                return RedirectToAction("Index", "Home");
            }

            // Giả sử bạn đã xác thực người dùng và lấy thông tin người dùng từ HttpContext.User
            var userName = HttpContext.User.Identity.Name; // hoặc từ session nếu lưu trữ thông tin người dùng khác
            var user = _context.Users.FirstOrDefault(u => u.Username == userName);

            // Tạo một dictionary để lưu trữ giá của từng sản phẩm
            var productPrices = cart.CartProducts.ToDictionary(
                cp => cp.Product.ProductId,
                cp => cart.IsSale(_context, cp.Product.ProductId));

            var viewModel = new CheckoutViewModel
            {
                CartProducts = cart.CartProducts,
                TotalAmount = cart.Amount(_context),
                Name = user?.FullName,
                Email = user?.Email,
                Address = user?.Address,
                Phone = user?.PhoneNumber
            };

            // Truyền giá sản phẩm vào ViewBag
            ViewBag.ProductPrices = productPrices;

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CheckoutViewModel model)
        {
            var cart = GetCart();
            if (!cart.CartProducts.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng trống. Không thể đặt hàng.";
                return RedirectToAction("Error");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                TempData["ErrorMessage"] = "Người dùng không tồn tại với email này.";
                return RedirectToAction("Error");
            }

            model.UserId = user.UserId;
            model.TotalAmount = cart.Amount(_context);

            var productPrices = cart.CartProducts.ToDictionary(
                cp => cp.Product.ProductId,
                cp => cart.IsSale(_context, cp.Product.ProductId));

            // Kiểm tra tồn kho trước khi xử lý đơn hàng
            foreach (var cartProduct in cart.CartProducts)
            {
                var product = await _context.Products.FindAsync(cartProduct.Product.ProductId);
                if (product == null || product.StockQuantity < cartProduct.Quantity)
                {
                    TempData["ErrorMessage"] = $"Số lượng sản phẩm {cartProduct.Product.ProductName} không đủ. Số lượng tồn kho hiện tại: {product?.StockQuantity ?? 0}.";
                    return RedirectToAction("Error");
                }
            }

            // Tạo đơn hàng mới
            var order = new Order
            {
                UserId = model.UserId,
                OrderDate = DateTime.Now,
                TotalAmount = model.TotalAmount,
                Status = "WAITING",
                CreatedAt = DateTime.Now
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Thêm chi tiết đơn hàng và cập nhật số lượng sản phẩm
            foreach (var cartProduct in cart.CartProducts)
            {
                var unitPrice = productPrices[cartProduct.Product.ProductId];

                var orderDetail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    ProductId = cartProduct.Product.ProductId,
                    Quantity = cartProduct.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = cartProduct.Quantity * unitPrice
                };

                _context.OrderDetails.Add(orderDetail);

                // Cập nhật số lượng sản phẩm trong kho
                var product = await _context.Products.FindAsync(cartProduct.Product.ProductId);
                if (product != null)
                {
                    product.StockQuantity -= cartProduct.Quantity;
                    _context.Products.Update(product);
                }
            }

            await _context.SaveChangesAsync();

            // Xóa giỏ hàng sau khi đặt hàng thành công
            HttpContext.Session.Remove("Cart");

            return RedirectToAction("Confirmation", new { orderId = order.OrderId });
        }

        public IActionResult Error()
        {
            return View();
        }

        public async Task<IActionResult> Confirmation(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Include(o => o.User)  // Bao gồm thông tin người dùng
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound();
            }

            var viewModel = new ConfirmationViewModel
            {
                OrderId = order.OrderId,
                OrderDate = DateTime.Now,
                TotalAmount = order.TotalAmount,
                OrderDetails = order.OrderDetails.Select(od => new OrderDetailViewModel
                {
                    Product = od.Product,
                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice,
                    TotalPrice = od.TotalPrice
                }).ToList(),
                User = order.User  // Gán thông tin người dùng
            };

            return View(viewModel);
        }
    }
}
