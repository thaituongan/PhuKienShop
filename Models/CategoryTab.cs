using Microsoft.AspNetCore.Mvc;

namespace PhuKienShop.Data

{
	public class CategoryTab : ViewComponent
	{
		private readonly PkShopContext _context;
		public CategoryTab(PkShopContext context)
		{
			_context = context;
		}
		public IViewComponentResult Invoke()
		{
			return View(_context.Categories.ToList());
		}

	}
}
