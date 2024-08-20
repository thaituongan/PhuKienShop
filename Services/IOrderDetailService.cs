using System.Collections.Generic;
using System.Threading.Tasks;
using PhuKienShop.Data; // Đảm bảo bạn sử dụng namespace chính xác

namespace PhuKienShop.Services
{
    public interface IOrderDetailService
    {
        Task<List<OrderDetail>> SelectByOrdersAsync(List<int> oids);
    }
}
