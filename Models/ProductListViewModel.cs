using PhuKienShop.Data;

public class ProductListViewModel
{
    public IEnumerable<ProductViewModel> Products { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int? CategoryId { get; set; }
}