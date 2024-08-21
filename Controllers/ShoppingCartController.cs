using Microsoft.AspNetCore.Mvc;
using PhuKienShop.Data;
using PhuKienShop.Models;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace PhuKienShop.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly PkShopContext _db;

        public ShoppingCartController(PkShopContext db)
        {
            _db = db;
        }

        private CartModel GetCart()
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(cartJson))
            {
                var cart = new CartModel();
                SaveCart(cart);
                return cart;
            }
            return JsonSerializer.Deserialize<CartModel>(cartJson);
        }

        private void SaveCart(CartModel cart)
        {
            var cartJson = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString("Cart", cartJson);
        }

        public IActionResult Index()
        {
			var cart = GetCart();
			var totalAmount = cart.Amount(_db);
            var isSale = cart.IsSale(_db);
            ViewBag.TotalAmount = totalAmount;
            ViewBag.IsSale = isSale;// Truyền giá trị xuống view

            return View(cart);
		}

        [HttpPost]
        public IActionResult Delete(int productId)
        {
            var cart = GetCart();
            cart.RemoveProduct(productId);
            SaveCart(cart);
            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            // Lấy giỏ hàng từ session
            var cart = GetCart();

            // Cập nhật số lượng sản phẩm trong giỏ hàng
            var cartProduct = cart.CartProducts.FirstOrDefault(p => p.Product.ProductId == productId);
            if (cartProduct != null)
            {
                cartProduct.Quantity = quantity;
                SaveCart(cart);
            }

            return Json(new { success = true });
        }

        public IActionResult AddToCart(int productId, int quantity)
        {
            var product = _db.Products.Find(productId);
            if (product != null)
            {
                var cart = GetCart();
                cart.AddProduct(product, quantity);
                SaveCart(cart);
            }
            return RedirectToAction("Index");
        }
    }
}