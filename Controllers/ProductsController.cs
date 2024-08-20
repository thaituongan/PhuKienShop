using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PhuKienShop.Data;
using PhuKienShop.Models;
namespace PhuKienShop.Controllers
{
	public class ProductsController : Controller
	{
		private readonly PkShopContext _context;

		public ProductsController(PkShopContext context)
		{
			_context = context;
		}

		// GET: Products
		public async Task<IActionResult> Index()
		{
			var pkShopContext = _context.Products.Include(p => p.Category);
			return View(await pkShopContext.ToListAsync());
		}

		// GET: Products/Details/5
		public async Task<IActionResult> Details(int? id, int categoryId)
		{
			if (id == null)
			{
				return NotFound();
			}
			//lấy ra sản phẩm được chọn
			var product = await _context.Products
				.Include(p => p.Category)
				.Include(p => p.ProductSales) // Thêm dòng này để nạp đầy đủ dữ liệu ProductSales
				.FirstOrDefaultAsync(m => m.ProductId == id);

			if (product == null)
			{
				return NotFound();
			}
			//lấy ra sản phấm liên quan đến sản phẩm đó
			var relatedProduct = _context.Products
				.Include(p => p.Category)
				.Include(p => p.ProductSales)
				//lay cac san pham cung category ngoai tru san pham hien tai
				.Where(p => p.CategoryId == categoryId && p.ProductId != id)
				.Take(4)
				.ToList();

			//truyen vao ProductDetailModel
			var viewModel = new ProductDetailModel
			{
				CurrentProduct = product,
				RelatedProducts = relatedProduct
			};
			return View(viewModel);
		}

		// SEARCH:   
		//trả về phần gợi ý sản phẩm dưới dạng PartialView
		public IActionResult SearchPartial(string searchTerm)
		{
			var results = _context.Products
				.Include(p => p.ProductSales) //nạp thêm ProductSales để lấy được giá giảm
				.Where(pd => pd.ProductName.Contains(searchTerm))
				.Take(4) //lấy ra chỉ 4 sản phẩm đầu tiên
				.ToList(); //lấy ra các sản phẩm có chứa searchTerm từ người dùng
			return PartialView("SearchPartial", results);// đưa results truyền tới SearchPartial.cshtml
		}

		//trả về các kết quả khi người dùng nhấn Tìm kiếm từ name searchTerm
		public IActionResult Search(string searchTerm, int category)
		{
			var query = _context.Products
					  .Include(p => p.ProductSales)
					  .Include(p => p.Category).AsQueryable();
			//loc theo tim kiem
			if (!string.IsNullOrEmpty(searchTerm))
			{
				query = query.Where(p => p.ProductName.Contains(searchTerm));
			}
			//loc theo select danh muc
			if (category > 0)
			{
				query = query.Where(p => p.CategoryId == category);
			}
			var result = query.ToList();
			return View(result);
		}

		// GET: Products/Create                
		public IActionResult Create()
		{
			ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId");
			return View();
		}

		// POST: Products/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("ProductId,ProductName,Description,Price,StockQuantity,CategoryId,Branch,ImageUrl,CreatedAt,UpdatedAt")] Product product)
		{
			if (ModelState.IsValid)
			{
				_context.Add(product);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId", product.CategoryId);
			return View(product);
		}

		// GET: Products/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var product = await _context.Products.FindAsync(id);
			if (product == null)
			{
				return NotFound();
			}
			ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId", product.CategoryId);
			return View(product);
		}

		// POST: Products/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("ProductId,ProductName,Description,Price,StockQuantity,CategoryId,Branch,ImageUrl,CreatedAt,UpdatedAt")] Product product)
		{
			if (id != product.ProductId)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(product);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!ProductExists(product.ProductId))
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
			ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId", product.CategoryId);
			return View(product);
		}

		// GET: Products/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var product = await _context.Products
				.Include(p => p.Category)
				.FirstOrDefaultAsync(m => m.ProductId == id);
			if (product == null)
			{
				return NotFound();
			}

			return View(product);
		}

		// POST: Products/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var product = await _context.Products.FindAsync(id);
			if (product != null)
			{
				_context.Products.Remove(product);
			}

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}
		private bool ProductExists(int id)
		{
			return _context.Products.Any(e => e.ProductId == id);
		}
	}
}
