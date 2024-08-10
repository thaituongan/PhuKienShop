using Microsoft.AspNetCore.Mvc;
using PhuKienShop.Data;

namespace PhuKienShop.Models
{
	public class ListSales : ViewComponent
	{
			private readonly PkShopContext _context;

			public ListSales(PkShopContext context)
			{
				_context = context;
			}

		public IViewComponentResult Invoke()
		{

			return View(_context.ProductSales.ToList());
		}
			
		}
	}

