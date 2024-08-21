using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging.Signing;
using PhuKienShop.Data;

namespace PhuKienShop.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly PkShopContext _context;

        public CategoryService(PkShopContext context)
        {
            _context = context;
        }

        public async Task<List<Category>> SelectAllAsync()
        {
            return await _context.Categories
                .ToListAsync();
        }

       

    }
}

