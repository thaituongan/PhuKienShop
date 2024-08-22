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
            var productPrices = new Dictionary<int, decimal>();

            foreach (var cartProduct in cart.CartProducts)
            {
                var price = cart.IsSale(_db, cartProduct.Product.ProductId);
                productPrices[cartProduct.Product.ProductId] = price;
            }

            ViewBag.ProductPrices = productPrices;
            var totalAmount = cart.Amount(_db);
         
            ViewBag.TotalAmount = totalAmount;

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