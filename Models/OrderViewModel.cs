using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhuKienShop.Data;

namespace PhuKienShop.Models
{
    public class OrderViewModel
    {
        public Order Order { get; set; }
        public string Details { get; set; }

        public List<OrderDetail> orderDetails { get; set; }
        public Dictionary<Order, string> orderAndDetails { get; set; }

        public OrderViewModel(Dictionary<Order, string> _orderAndDetails)
        {
            orderAndDetails = _orderAndDetails;
        } 
        
        public OrderViewModel(List<OrderDetail> _orderDetails)
        {
            orderDetails = _orderDetails;
        }
        public OrderViewModel(int id)
        {
            Order = new Order(id);
            Details = "";
        }
        public OrderViewModel(Order _order)
        {
            Order = _order;
        }

        public OrderViewModel(Order _order, string _details)
        {
            Order =_order;
            Details = _details; 
        }

        public OrderViewModel(int id, string _details)
        {
            Order = new Order(id);
            Details = _details; 
        }

        public OrderViewModel()
        {
          
        }

        public static string translateOrderStatus(string status)
        {
            return Order.tranlateOrderStatus(status);
        }


    }

    

}
