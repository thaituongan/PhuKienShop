using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PhuKienShop.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PhuKienShop.Controllers
{
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


		// GET: ProductSales/Details/5
		public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productSale = await _context.ProductSales
                .Include(p => p.Product)
                .FirstOrDefaultAsync(m => m.SaleId == id);
            if (productSale == null)
            {
                return NotFound();
            }

            return View(productSale);
        }

        // GET: ProductSales/Create
        public IActionResult Create()
        {
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductName");
            return View();
        }

        // POST: ProductSales/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,DiscountPercentage,StartDate,EndDate")] ProductSale productSale)
        {


            // Tìm sản phẩm dựa trên ProductId
            var product = await _context.Products.SingleOrDefaultAsync(p => p.ProductId == productSale.ProductId);

            if (product != null)
            {
                // Set OriginalPrice based on the selected product's price
                productSale.OriginalPrice = product.Price;
                productSale.SalePrice = product.Price - (product.Price * productSale.DiscountPercentage / 100);
                // Set CreatedAt and UpdatedAt to default values
                productSale.CreatedAt = DateTime.Now;
                productSale.UpdatedAt = DateTime.Now;


                // SalePrice không cần đặt, nó được tự động tính toán trong database

                // Thêm ProductSale vào database
                _context.Add(productSale);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError("", "Product not found.");
            }



            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductName", productSale.ProductId);
            return View(productSale);
        }



		// GET: ProductSales/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var productSale = await _context.ProductSales
				.Include(ps => ps.Product)
				.FirstOrDefaultAsync(ps => ps.SaleId == id);

			if (productSale == null)
			{
				return NotFound();
			}

			ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductName", productSale.ProductId);
			return View(productSale);
		}

		// POST: ProductSales/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("SaleId,ProductId,DiscountPercentage,StartDate,EndDate")] ProductSale productSale)
		{
			if (id != productSale.SaleId)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					var existingProductSale = await _context.ProductSales
						.Include(ps => ps.Product)
						.FirstOrDefaultAsync(ps => ps.SaleId == id);

					if (existingProductSale == null)
					{
						return NotFound();
					}

					// Cập nhật thông tin dựa trên form
					existingProductSale.DiscountPercentage = productSale.DiscountPercentage;
					existingProductSale.StartDate = productSale.StartDate;
					existingProductSale.EndDate = productSale.EndDate;
					existingProductSale.SalePrice = existingProductSale.OriginalPrice - (existingProductSale.OriginalPrice * productSale.DiscountPercentage / 100);
					existingProductSale.UpdatedAt = DateTime.Now;

					_context.Update(existingProductSale);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!ProductSaleExists(productSale.SaleId))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
				return RedirectToAction(nameof(Index));
			}

			ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductName", productSale.ProductId);
			return View(productSale);
		}

		// GET: ProductSales/Delete/5
		public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productSale = await _context.ProductSales
                .Include(p => p.Product)
                .FirstOrDefaultAsync(m => m.SaleId == id);
            if (productSale == null)
            {
                return NotFound();
            }

            return View(productSale);
        }

        // POST: ProductSales/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var productSale = await _context.ProductSales.FindAsync(id);
            if (productSale != null)
            {
                _context.ProductSales.Remove(productSale);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductSaleExists(int id)
        {
            return _context.ProductSales.Any(e => e.SaleId == id);
        }
    }
}
