using System;
using System.Collections.Generic;

namespace PhuKienShop.Data;

public partial class Order
{
    public static string WAITING = "chờ xác nhận";
    public static string CONFIRMED = "đã xác nhận";
    public static string PACKAGEED = "đã đóng gói";
    public static string DELIVERYED = "đang vận chuyển";
    public static string COMPLETEED = "đã hoàn thành";
    public static string CANCELED = "đã hủy";
    public static string UNDEFINED = "không xác định";

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


    public DateTime getEstimatedDelivery()
    {
        DateTime temp = OrderDate ?? DateTime.Now;
        return temp.AddDays(3);
    }

    public static string tranlateOrderStatus(string status)
    {
        status = status.ToUpper();
        switch (status)
        {
            case "WAITING": return Order.WAITING;
            case "CONFIRMED": return Order.CONFIRMED;
            case "PACKAGEED": return Order.PACKAGEED;
            case "DELIVERYED": return Order.DELIVERYED;
            case "COMPLETEED": return Order.COMPLETEED;
            case "CANCELED": return Order.CANCELED;
            default: return Order.UNDEFINED;   
        }
        
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
