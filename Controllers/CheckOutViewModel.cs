using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PhuKienShop.Models
{
	public class CheckoutViewModel
	{
		[Required]
		public int UserId { get; set; }

		[Required]
		public decimal TotalAmount { get; set; }

		public List<OrderDetailViewModel> OrderDetails { get; set; } = new List<OrderDetailViewModel>();

		public class OrderDetailViewModel
		{
			[Required]
			public int ProductId { get; set; }

			[Required]
			public int Quantity { get; set; }

			[Required]
			public decimal UnitPrice { get; set; }
		}
	}
}
