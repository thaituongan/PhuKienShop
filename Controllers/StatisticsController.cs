using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhuKienShop.Data;
using PhuKienShop.Models;

namespace PhuKienShop.Controllers
{
    public class StatisticsController : Controller
    {
        private readonly PkShopContext _db;

        public StatisticsController(PkShopContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var previousMonth = currentMonth == 1 ? 12 : currentMonth - 1;
            var previousYear = currentMonth == 1 ? currentYear - 1 : currentYear;

            var totalOrdersLastMonth = _db.Orders
                .Where(o => o.OrderDate.Value.Month == previousMonth && o.OrderDate.Value.Year == previousYear)
                .Count();

            var totalRevenueLastMonth = _db.Orders
                .Where(o => o.OrderDate.Value.Month == previousMonth && o.OrderDate.Value.Year == previousYear)
                .Sum(o => o.TotalAmount);

            var totalCustomersLastMonth = _db.Users
                .Where(c => c.CreatedAt.Value.Month == previousMonth && c.CreatedAt.Value.Year == previousYear)
                .Count();

            var totalProductsSoldLastMonth = _db.OrderDetails
                .Where(od => od.Order.OrderDate.Value.Month == previousMonth && od.Order.OrderDate.Value.Year == previousYear)
                .Sum(od => od.Quantity);

            var viewModel = new StatisticsModel
            {
                TotalOrders = _db.Orders.Count(),
                TotalRevenue = _db.Orders.Sum(o => o.TotalAmount),
                TotalCustomers = _db.Users.Count(),
                TotalProduct = _db.OrderDetails.Sum(o => o.Quantity),
                TotalOrdersThisMonth = _db.Orders
                    .Where(o => o.OrderDate.Value.Month == currentMonth && o.OrderDate.Value.Year == currentYear)
                    .Count(),
                TotalRevenueThisMonth = _db.Orders
                    .Where(o => o.OrderDate.Value.Month == currentMonth && o.OrderDate.Value.Year == currentYear)
                    .Sum(o => o.TotalAmount),
                TotalCustomersThisMonth = _db.Users
                    .Where(c => c.CreatedAt.Value.Month == currentMonth && c.CreatedAt.Value.Year == currentYear)
                    .Count(),
                TotalProductThisMonth = _db.OrderDetails
                .Where(od => od.Order.OrderDate.Value.Month == currentMonth && od.Order.OrderDate.Value.Year == currentYear)
                .Sum(od => od.Quantity),
                OrderChangePercentage = totalOrdersLastMonth == 0 ? 100 : ((double)(_db.Orders.Count() - totalOrdersLastMonth) / totalOrdersLastMonth) * 100,
                CustomerChangePercentage = totalCustomersLastMonth == 0 ? 100 : ((double)(_db.Users.Count() - totalCustomersLastMonth) / totalCustomersLastMonth) * 100,
                RevenueChangePercentage = totalRevenueLastMonth == 0 ? 100 : ((double)(_db.Orders.Sum(o => o.TotalAmount) - totalRevenueLastMonth) / (double)totalRevenueLastMonth) * 100,
                ProductSoldChangePercentage = totalProductsSoldLastMonth == 0 ? 100 : ((double)(_db.Products.Count() - totalProductsSoldLastMonth) / totalProductsSoldLastMonth) * 100
            };
            return View(viewModel);
        }
    }
}
