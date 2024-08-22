using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.IdentityModel.Tokens;
using PhuKienShop.Data;
using PhuKienShop.Models;
namespace PhuKienShop.Controllers
{
    [AllowAnonymous]

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
        public async Task<List<Product>> SelectByIDsAsync(List<int> ids)
        {
            return await _context.Products
                                 .Include(o => o.OrderDetails)
                                 .Where(od => ids.Contains(od.ProductId))
                                 .ToListAsync();
        }

		// GET: Products/Details/5
		public async Task<IActionResult> Details(int? id, int categoryId, int page = 1)
		{
			if (id == null)
			{
				return NotFound();
			}

			// Lấy sản phẩm được chọn
			var product = await _context.Products
				.Include(p => p.Category)
				.Include(p => p.ProductSales)
				.Include(p => p.Reviews)
				.ThenInclude(r => r.User)
				.FirstOrDefaultAsync(m => m.ProductId == id);

			if (product == null)
			{
				return NotFound();
			}

			// Lấy sản phẩm liên quan
			var relatedProducts = _context.Products
				.Include(p => p.Category)
				.Include(p => p.ProductSales)
				.Where(p => p.CategoryId == categoryId && p.ProductId != id)
				.Take(4)
				.ToList();

			// Xử lý phân trang cho bình luận
			var pageSize = 2; // số bình luận mỗi trang
			var reviews = product.Reviews
				.OrderByDescending(r => r.CreatedAt)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToList();

			var totalReviews = product.Reviews.Count();
			var totalPages = (int)Math.Ceiling(totalReviews / (double)pageSize);

			// Tính toán các chỉ số đánh giá
			int fiveStar = product.Reviews.Count(r => r.Rating == 5);
			int fourStar = product.Reviews.Count(r => r.Rating == 4);
			int threeStar = product.Reviews.Count(r => r.Rating == 3);
			int twoStar = product.Reviews.Count(r => r.Rating == 2);
			int oneStar = product.Reviews.Count(r => r.Rating == 1);

			double fiveStarPercentage = totalReviews > 0 ? (double)fiveStar / totalReviews * 100 : 0;
			double fourStarPercentage = totalReviews > 0 ? (double)fourStar / totalReviews * 100 : 0;
			double threeStarPercentage = totalReviews > 0 ? (double)threeStar / totalReviews * 100 : 0;
			double twoStarPercentage = totalReviews > 0 ? (double)twoStar / totalReviews * 100 : 0;
			double oneStarPercentage = totalReviews > 0 ? (double)oneStar / totalReviews * 100 : 0;

			double averageRating = product.Reviews.Any() ? product.Reviews.Average(r => r.Rating) : 0;

			var viewModel = new ProductDetailModel
			{
				CurrentProduct = product,
				RelatedProducts = relatedProducts,
				Reviews = reviews,
				AverageRating = averageRating,
				FiveStarPercentage = fiveStarPercentage,
				FourStarPercentage = fourStarPercentage,
				ThreeStarPercentage = threeStarPercentage,
				TwoStarPercentage = twoStarPercentage,
				OneStarPercentage = oneStarPercentage,
				TotalPages = totalPages,
				CurrentPage = page
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

		[HttpPost]
		public async Task<IActionResult> AddReview(int productId, string comment, int rating)
		{
			if (User.Identity.IsAuthenticated)
			{
				var email = User.FindFirst(ClaimTypes.Email)?.Value;
				var userDetails = _context.Users.FirstOrDefault(u => u.Email == email);
				int userId = userDetails.UserId;

				var review = new Review
				{
					ProductId = productId,
					UserId = userId,
					Rating = rating,
					Comment = comment,
					CreatedAt = DateTime.Now
				};

				_context.Reviews.Add(review);
				await _context.SaveChangesAsync();

				return RedirectToAction("Details", new { id = productId });
			}

			return RedirectToAction("Login", "Account");
		}

	}
}
