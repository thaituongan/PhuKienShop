using PhuKienShop.Data;

public class CartModel
{
    public List<CartProduct> CartProducts { get; set; } = new List<CartProduct>();
	public decimal Amount(PkShopContext _db)
	{
		decimal totalAmount = 0;

		foreach (var cartProduct in CartProducts)
		{
			var product = cartProduct.Product;
			var sale = _db.ProductSales
				.FirstOrDefault(ps => ps.ProductId == product.ProductId &&
									  ps.StartDate <= DateTime.Now &&
									  ps.EndDate >= DateTime.Now);

			decimal priceToUse = sale != null ? sale.SalePrice : product.Price;
			totalAmount += priceToUse * cartProduct.Quantity;
		}

		return totalAmount;
	}
    public decimal IsSale(PkShopContext _db, int productId)
    {
        var cartProduct = CartProducts.FirstOrDefault(cp => cp.Product.ProductId == productId);

        if (cartProduct == null)
        {
            throw new ArgumentException("Product not found in cart.");
        }

        var product = cartProduct.Product;
        var sale = _db.ProductSales
            .FirstOrDefault(ps => ps.ProductId == product.ProductId &&
                                  ps.StartDate <= DateTime.Now &&
                                  ps.EndDate >= DateTime.Now);

        return sale != null ? sale.SalePrice : product.Price;
    }

    public void RemoveProduct(int productId)
    {
        CartProducts.RemoveAll(cp => cp.Product.ProductId == productId);
    }
	public int GetTotal()
	{
		return CartProducts.Sum(cp => cp.Quantity);
	}
	public void AddProduct(Product product, int quantity)
    {
        var existingProduct = CartProducts.FirstOrDefault(cp => cp.Product.ProductId == product.ProductId);
        if (existingProduct != null)
        {
            existingProduct.Quantity += quantity;
        }
        else
        {
            CartProducts.Add(new CartProduct { Product = product, Quantity = quantity });
        }
    }
}
public class CartProduct
{
    public Product Product { get; set; }
    public int Quantity { get; set; }
}