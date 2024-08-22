using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhuKienShop.Data;
using PhuKienShop.Models;

namespace PhuKienShop.Controllers
{
    [Authorize(Policy = "AdminOnly")]
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
                .Where(c => c.Role == "User" && _db.Orders.Any(o => o.UserId == c.UserId &&
                                       o.OrderDate.HasValue &&
                                       o.OrderDate.Value.Month == previousMonth &&
                                       o.OrderDate.Value.Year == previousYear))
                .Count();

            var totalProductsSoldLastMonth = _db.OrderDetails
                .Where(od => od.Order.OrderDate.Value.Month == previousMonth && od.Order.OrderDate.Value.Year == previousYear)
                .Sum(od => od.Quantity);

            var totalOrders = _db.Orders.Count();
            var totalRevenue = _db.Orders.Sum(o => o.TotalAmount);
            var totalCustomers = _db.Users.Count();
            var totalProduct = _db.OrderDetails.Sum(o => o.Quantity);

            var totalOrdersThisMonth = _db.Orders
                .Where(o => o.OrderDate.Value.Month == currentMonth && o.OrderDate.Value.Year == currentYear)
                .Count();

            var totalRevenueThisMonth = _db.Orders
                .Where(o => o.OrderDate.Value.Month == currentMonth && o.OrderDate.Value.Year == currentYear)
                .Sum(o => o.TotalAmount);

            var totalCustomersThisMonth = _db.Users
                .Where(c => c.Role == "User" &&
                 _db.Orders.Any(o => o.UserId == c.UserId &&
                                     o.OrderDate.HasValue &&
                                     o.OrderDate.Value.Month == currentMonth &&
                                     o.OrderDate.Value.Year == currentYear))
                .Count();


            var totalProductThisMonth = _db.OrderDetails
                .Where(od => od.Order.OrderDate.Value.Month == currentMonth && od.Order.OrderDate.Value.Year == currentYear)
                .Sum(od => od.Quantity);
            var viewModel = new StatisticsModel
            {
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                TotalCustomers = totalCustomers,
                TotalProduct = totalProduct,
                TotalOrdersThisMonth = totalOrdersThisMonth,
                TotalRevenueThisMonth = totalRevenueThisMonth,
                TotalCustomersThisMonth = totalCustomersThisMonth,
                TotalProductThisMonth = totalProductThisMonth,
                TotalOrdersLastMonth = totalOrdersLastMonth,
                TotalCustomersLastMonth = totalCustomersLastMonth,
                TotalRevenueLastMonth = totalRevenueLastMonth,
                TotalProductLastMonth = totalProductsSoldLastMonth
            };

            // Convert to formatted strings
            ViewData["FormattedTotalRevenue"] = viewModel.TotalRevenue.ToString("N0");
            ViewData["FormattedTotalRevenueThisMonth"] = viewModel.TotalRevenueThisMonth.ToString("N0");

            return View(viewModel);
        }
    }
}
