using PhuKienShop.Data;

namespace PhuKienShop.Models
{
	public class ProductDetailModel
	{
		public Product CurrentProduct { get; set; }
		public List<Product> RelatedProducts { get; set; }

		public IEnumerable<Review> Reviews { get; set; } = new List<Review>();

		public double AverageRating { get; set; }
		public int CurrentPage { get; set; }
		public int TotalPages { get; set; }

		public double FiveStarPercentage { get; set; }
		public double FourStarPercentage { get; set; }
		public double ThreeStarPercentage { get; set; }
		public double TwoStarPercentage { get; set; }
		public double OneStarPercentage { get; set; }
	}
}
