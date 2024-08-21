using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging.Signing;
using PhuKienShop.Data;

namespace PhuKienShop.Services
{
    public class OrderDetailService : IOrderDetailService
    {
        private readonly PkShopContext _context;

        public OrderDetailService(PkShopContext context)
        {
            _context = context;
        }

        public async Task<List<OrderDetail>> SelectByOrdersAsync(List<int> oids)
        {
            return await _context.OrderDetails
                .Include(o => o.Order)
                .Include(o => o.Product)
                .Where(od => od.OrderId.HasValue && oids.Contains(od.OrderId.Value))
                .ToListAsync();
        }

        public async Task<List<OrderDetail>> SelectByOrderIDAsync(int id)
        {
            return await _context.OrderDetails
              .Include(o => o.Order)
            .Include(o => o.Product)
              .Where(od => od.OrderId.HasValue && od.OrderId.Value == id)
              .ToListAsync();
        }

    }
}

