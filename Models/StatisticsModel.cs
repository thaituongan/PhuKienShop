namespace PhuKienShop.Models
{
    public class StatisticsModel
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalProduct { get; set; }
        public int TotalOrdersThisMonth { get; set; }
        public decimal TotalRevenueThisMonth { get; set; }
        public int TotalCustomersThisMonth { get; set; }
        public int TotalProductThisMonth { get; set; }
        public double OrderChangePercentage { get; set; }
        public double CustomerChangePercentage { get; set; }
        public double RevenueChangePercentage { get; set; }
        public double ProductSoldChangePercentage { get; set; }



    }


}
