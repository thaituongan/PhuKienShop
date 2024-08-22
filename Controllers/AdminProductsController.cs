using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NuGet.Packaging.Signing;
using PhuKienShop.Data;
using PhuKienShop.Models;
using PhuKienShop.Services;
namespace PhuKienShop.Controllers
{
	public class AdminProductsController : Controller
	{
		private readonly PkShopContext _context;
		private readonly ICategoryService _categoryService;

		public AdminProductsController(PkShopContext context, ICategoryService categoryService)
		{
			_context = context;
			_categoryService = categoryService;

        }

		//them san pham
        public async Task<Product> InsertProductAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        [HttpPost]
        public async Task<IActionResult> addProduct(string name, string price, string quantity, string category, string branch, string img)
        {
			double nprice = Double.Parse(price);
			quantity = quantity ?? "0";
			int nqty = int.Parse(quantity);
			category = category ?? "1";
			int ncate = int.Parse(category);
			Console.WriteLine("thong tin product");
			Console.WriteLine(name);
			Console.WriteLine(price);
			Console.WriteLine(quantity);
			Console.WriteLine(category);
			Console.WriteLine(branch);
			Console.WriteLine(img);
			Product product = new Product(name, nprice, nqty, ncate, branch,img);
			product = await InsertProductAsync(product);
            return PartialView("_AdminProductRowUpdate", product);

        }

        [HttpPost]
        public async Task<IActionResult> prepareUpdate(string id)
		{
            if (id == null) id = "0";
            int idin = int.Parse(id);
            var product = await _context.Products.FindAsync(idin) ?? new Product();
            var categories = await _categoryService.SelectAllAsync()?? new List<Category>();
            AdminProductViewModel adminProductViewModel = new AdminProductViewModel(product, categories);
            return PartialView("_AdminProductEdit", adminProductViewModel);
        }


		[HttpPost]
		public async Task<IActionResult> updateProduct(string id, string name, string price, string quantity, string category, string branch, string img, string des)
        {
            if (id == null) id = "0";
			int idin = int.Parse(id);
            double nprice = Double.Parse(price);
            quantity = quantity ?? "0";
            int nqty = int.Parse(quantity);
            category = category ?? "1";
            int ncate = int.Parse(category);
            Product product = new Product(idin,name, nprice, nqty, ncate, branch, img,des);
            var existingProduct = await _context.Products.FindAsync(product.ProductId);
            if (existingProduct == null)
            {
                return NotFound();
            }
            existingProduct.ProductName = product.ProductName;
            existingProduct.Price = product.Price;
            existingProduct.StockQuantity = product.StockQuantity;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.Branch = product.Branch;
            existingProduct.ImageUrl = product.ImageUrl;
            existingProduct.Description = product.Description;
            await _context.SaveChangesAsync();
            return PartialView("_AdminProductRowUpdate", existingProduct);
           
        }

        public async Task<IActionResult> deleteProductById(string id)
        {
			if (id == null) id = "0";
			int idin = int.Parse(id);
            var product = _context.Products.Find(idin);
			product.StockQuantity = -1;
            _context.SaveChanges();

            var products = await _context.Products
                    .Include(p => p.Category)
                     .Where(p => p.StockQuantity >= 0)
                    .ToListAsync();

            AdminProductViewModel adminProductViewModel = new AdminProductViewModel(products);
            return PartialView("_AdminProductTableUpdate",adminProductViewModel);

        }

        // GET: Products
        public async Task<IActionResult> Index()
		{
            var products = await _context.Products
                             .Include(p => p.Category)
                             .ToListAsync();

            var categories = await _categoryService.SelectAllAsync();
			AdminProductViewModel adminProductViewModel = new AdminProductViewModel(products, categories);
            return View(adminProductViewModel);

        }
        public async Task<List<Product>> SelectByIDsAsync(List<int> ids)
        {
            return await _context.Products
                                 .Include(o => o.OrderDetails)
                                 .Where(od => ids.Contains(od.ProductId))
                                 .ToListAsync();
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
		public IActionResult Search(string searchTerm)
		{
			var product = _context.Products
                      .Include(p => p.ProductSales)
                      .Include(p => p.Category)
					  .Where(p=> p.ProductName.Contains(searchTerm))
					  .ToList();
			return View(product);
		}

        [HttpPost]
        public async Task<IActionResult> searchProduct(string input)
        {
            // Kiểm tra nếu input là số để so sánh với ID
            if (int.TryParse(input, out int id))
            {
                var products = await _context.Products
                    .Where(p => p.ProductId == id)
                    .ToListAsync();
				AdminProductViewModel pView = new AdminProductViewModel(products);
				return PartialView("_AdminProductTable", pView);
            }
            else
            {
                // Nếu input không phải là số, tìm kiếm theo Name
                var products = await _context.Products
                    .Where(p => p.ProductName.Contains(input))
                    .ToListAsync();
                AdminProductViewModel pView = new AdminProductViewModel(products);
                return PartialView("_AdminProductTable", pView);
            }

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
