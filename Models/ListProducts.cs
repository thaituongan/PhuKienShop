using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhuKienShop.Models;
using System.Linq;

namespace PhuKienShop.Data
{
    public class ListProducts : ViewComponent
    {
        private readonly PkShopContext _context;

        public ListProducts(PkShopContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            // Lấy danh sách sản phẩm từ cơ sở dữ liệu, bao gồm cả thông tin danh mục và giảm giá.
            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductSales)
                .Select(p => new ProductViewModel
                {
                    Product = p,
                    IsOnSale = p.ProductSales.Any(ps => ps.StartDate <= DateTime.Now && ps.EndDate >= DateTime.Now),
                    Sale = p.ProductSales
                            .Where(ps => ps.StartDate <= DateTime.Now && ps.EndDate >= DateTime.Now)
                            .OrderByDescending(ps => ps.CreatedAt)
                            .FirstOrDefault()
                })
                .ToList();

            // Trả về view với viewModel đã tạo.
            return View(products);
        }
    }

        public class ProductViewModel
    {
        public Product Product { get; set; }
        public bool IsOnSale { get; set; }
        public ProductSale? Sale { get; set; }
    }

    public class ProductListViewModel
    {
        public List<ProductViewModel> Products { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int? CategoryId { get; set; }
    }
}
