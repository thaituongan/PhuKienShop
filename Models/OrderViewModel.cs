using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhuKienShop.Data;

namespace PhuKienShop.Models
{
    public class OrderViewModel
    {
        public Order Order { get; set; }
        public string Details { get; set; }
    }

}
