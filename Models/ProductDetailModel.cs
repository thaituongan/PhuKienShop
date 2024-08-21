using PhuKienShop.Data;

namespace PhuKienShop.Models
{
	public class ProductDetailModel
	{
		public Product CurrentProduct { get; set; }
		public List<Product> RelatedProducts { get; set; }
	}
}
