using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhuKienShop.Data;
using PhuKienShop.Models;
using System.Diagnostics;

namespace PhuKienShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly PkShopContext _context;

        public HomeController(PkShopContext context)
        {
            _context = context;
        }

        public IActionResult Index()
		{
			DateTime dealEndTime = DateTime.Now.AddDays(2);
			String deal = dealEndTime.ToString("yyyy-MM-ddTHH:mm:ss");
			ViewBag.DealEndTime = deal;

			return View();
            //_context.Products.ToList()
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
