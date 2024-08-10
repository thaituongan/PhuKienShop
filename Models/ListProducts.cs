using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            var products = _context.Products
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

            return View(products);
        }
    }

    public class ProductViewModel
    {
        public Product Product { get; set; }
        public bool IsOnSale { get; set; }
        public ProductSale? Sale { get; set; }
    }
}
