using System;
using System.Collections.Generic;

namespace PhuKienShop.Data;

public partial class Product
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public int CategoryId { get; set; }

    //dâu ? biểu thị biến có thể là nullable
    public string? Branch { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<ProductSale> ProductSales { get; set; } = new List<ProductSale>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

	public bool isProductSale(int productId)
	{
		// In thông tin để kiểm tra
		var productSales = ProductSales
			.Where(ps => ps.ProductId == productId)
			.ToList();

		// Kiểm tra nội dung của productSales
		foreach (var sale in productSales)
		{
			Console.WriteLine($"ProductId: {sale.ProductId}, StartDate: {sale.StartDate}, EndDate: {sale.EndDate}");
		}

		// Kiểm tra xem có bất kỳ chương trình giảm giá nào hiện tại áp dụng cho sản phẩm này không
		if( productSales.Any(ps => ps.StartDate <= DateTime.Now && ps.EndDate >= DateTime.Now))
        {
            return true;
        }
        return false;
	}



	public decimal getSalePrice()
    {
        var ps=  ProductSales.Where(ps => ps.StartDate <= DateTime.Now && ps.EndDate >= DateTime.Now)
                            .OrderByDescending(ps => ps.CreatedAt)
                            .FirstOrDefault();
        if(ps != null)
        {
            return ps.SalePrice;
        }
        else
        {
            return Price;
        }

	}
	//bỏ dấu . ở trước đường dẫn để đưa ảnh lên view
	public string RelativeImageUrl(string url)
    {
        string str = url.Substring(1);
        return str;
    }
   
  
}

