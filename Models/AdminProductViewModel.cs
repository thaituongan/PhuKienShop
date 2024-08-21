using PhuKienShop.Data;

namespace PhuKienShop.Models
{
    public class AdminProductViewModel
    {
        public Product product { get; set; }
        public Category category { get; set; }

        public List<Product> products { get; set; }
        public List<Category> categories { get; set; }

        
        public AdminProductViewModel(Product _product)
        {
            product = _product;
        }
        public AdminProductViewModel(Product _product, List<Category> _categories)
        {
            product = _product;
            categories = _categories;
        }

       
        public AdminProductViewModel(List<Product> _products, List<Category> _categories)
        {
            products = _products;
            categories = _categories;
        } 
        
        public AdminProductViewModel(List<Product> _products)
        {
            products = _products;
          
        }

        public AdminProductViewModel(Category _category)
        {
            category = _category;
        }
        public AdminProductViewModel()
        {
          
        }
    }

 
}
