using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhuKienShop.Data;
using PhuKienShop.Models;

namespace PhuKienShop.Controllers
{
    public class ListProductController : Controller
    {
        private readonly PkShopContext _context;

        public ListProductController(PkShopContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? categoryId, int page = 1)
        {
            int pageSize = 9; // Số sản phẩm trên mỗi trang

            var productsQuery = _context.Products
                .Include(p => p.ProductSales)
                .Select(p => new ProductViewModel
                {
                    Product = p,
                    IsOnSale = p.ProductSales.Any(ps => ps.StartDate <= DateTime.Now && ps.EndDate >= DateTime.Now),
                    Sale = p.ProductSales
                            .Where(ps => ps.StartDate <= DateTime.Now && ps.EndDate >= DateTime.Now)
                            .OrderByDescending(ps => ps.CreatedAt)
                            .FirstOrDefault()
                });

            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Product.CategoryId == categoryId.Value);
            }

            var totalProducts = await productsQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            var products = await productsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new ProductListViewModel
            {
                Products = products,
                CurrentPage = page,
                TotalPages = totalPages,
                CategoryId = categoryId
            };

            // Lấy danh sách danh mục từ cơ sở dữ liệu
            ViewData["Categories"] = await _context.Categories.ToListAsync();

            return View(viewModel);
        }
    }
}