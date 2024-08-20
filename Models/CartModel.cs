using PhuKienShop.Data;

public class CartModel
{
    public List<CartProduct> CartProducts { get; set; } = new List<CartProduct>();

    public decimal Amount()
    {
        return CartProducts.Sum(cp => cp.Product.Price * cp.Quantity);
    }

    public void RemoveProduct(int productId)
    {
        CartProducts.RemoveAll(cp => cp.Product.ProductId == productId);
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