using System;
using System.Collections.Generic;

namespace PhuKienShop.Data;

public partial class Order
{
    public int OrderId { get; set; }

    public int? UserId { get; set; }

    public DateTime? OrderDate { get; set; }

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual User? User { get; set; }

    public override bool Equals(object obj)
    {
        if (obj is Order other)
        {
            return OrderId == other.OrderId;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return OrderId.GetHashCode();
    }

    public Order()
    {
    }

    public Order(int id)
    {
        OrderId = id;
    }
}
