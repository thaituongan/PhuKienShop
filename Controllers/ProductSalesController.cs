using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PhuKienShop.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PhuKienShop.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class ProductSalesController : Controller
    {
        private readonly PkShopContext _context;

        public ProductSalesController(PkShopContext context)
        {
            _context = context;
        }

		// GET: ProductSales
		public async Task<IActionResult> Index()
		{
			// Lấy danh sách các ProductSale bao gồm cả Product liên quan
			var pkshopContext = _context.ProductSales.Include(ps => ps.Product);
			return View(await pkshopContext.ToListAsync());
		}
        [HttpPost]
        public async Task<IActionResult> Update(int SaleId, decimal DiscountPercentage, DateTime StartDate, DateTime EndDate)
        {
            var productSale = await _context.ProductSales.FindAsync(SaleId);

            if (productSale == null)
            {
                return NotFound();
            }

            productSale.DiscountPercentage = DiscountPercentage;
            productSale.SalePrice = productSale.OriginalPrice - (productSale.OriginalPrice * DiscountPercentage / 100);
            productSale.StartDate = StartDate;
            productSale.EndDate = EndDate;
            productSale.UpdatedAt = DateTime.Now;

            _context.Update(productSale);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // GET: ProductSales/Create
        public IActionResult Create()
        {
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductName");
            return View();
        }
        // Action to get the price of a product by its ID
        [HttpGet]
        public async Task<IActionResult> GetProductPrice(int id)
        {
            var product = await _context.Products
                .Where(p => p.ProductId == id)
                .Select(p => new { p.Price })
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound();
            }

            return Json(new { price = product.Price });
        }

        // POST: ProductSales/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,DiscountPercentage,StartDate,EndDate")] ProductSale productSale)
        {
            if (ModelState.IsValid)
            {
                var product = await _context.Products.SingleOrDefaultAsync(p => p.ProductId == productSale.ProductId);

                if (product != null)
                {
                    productSale.OriginalPrice = product.Price;
                    productSale.SalePrice = product.Price - (product.Price * productSale.DiscountPercentage / 100);
                    productSale.CreatedAt = DateTime.Now;
                    productSale.UpdatedAt = DateTime.Now;

                    _context.Add(productSale);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Product not found.");
                }
            }

            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductName", productSale.ProductId);
            return View(productSale);
        }
        private bool ProductSaleExists(int id)
        {
            return _context.ProductSales.Any(e => e.SaleId == id);
        }
    }
}
