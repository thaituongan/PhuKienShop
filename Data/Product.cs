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

	//bỏ dấu . ở trước đường dẫn để đưa ảnh lên view
	public string RelativeImageUrl(string url)
    {
        string str = url.Substring(1);
        return str;
    }
    //lấy giá sale nếu là sản phẩm đang sale
	/*public decimal GetPrice()
	{
		var currentSale = ProductSales
			.Where(ps => ps.StartDate <= DateTime.Now && ps.EndDate >= DateTime.Now)
			.FirstOrDefault();

		return currentSale != null ? currentSale.SalePrice : Price;
	}*/

    public bool isProductSale()
    {
        var listProSale = ProductSales.Where(pd => pd.StartDate <= DateTime.Now &&  pd.EndDate >= DateTime.Now).ToList();
        return listProSale.Any();
    }
    public decimal GetPrice()
    {
       //xem sản phẩm có trong danh sách của sản phẩm đang sale hay không
       var productSale = ProductSales.Where(pd => pd.StartDate <= DateTime.Now && pd.EndDate >= DateTime.Now).FirstOrDefault();
        //neu san pham giam gia thi lay gia khuyen mai, khong thi lay gia goc
        return productSale != null ? productSale.SalePrice : Price;
    }


}

