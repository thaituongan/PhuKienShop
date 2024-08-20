using PhuKienShop.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PhuKienShop.Services
{
    public class ProductService : IProductService
    {
        private readonly PkShopContext _context;

        public ProductService(PkShopContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> SelectByIDsAsync(List<int> pids)
        {
            return await _context.Products
                .Where(p => pids.Contains(p.ProductId))
                .ToListAsync();
        }
    }
}
