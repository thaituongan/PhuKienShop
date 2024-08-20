using PhuKienShop.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhuKienShop.Services
{
    public interface IProductService
    {
        Task<List<Product>> SelectByIDsAsync(List<int> pids);
    }
}
