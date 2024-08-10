using System;
using System.Collections.Generic;

namespace PhuKienShop.Data;

public partial class ProductSale
{
    public int SaleId { get; set; }

    public int? ProductId { get; set; }

    public decimal OriginalPrice { get; set; }

    public decimal DiscountPercentage { get; set; }

    public decimal SalePrice { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Product? Product { get; set; }
}
