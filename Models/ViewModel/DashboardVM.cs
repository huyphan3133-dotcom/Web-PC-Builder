using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web_PC_Builder.Models.ViewModel
{
    /// <summary>Số liệu thống kê cho trang Home của Admin (biểu đồ dùng Google Charts).</summary>
    public class DashboardVM
    {
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalOrders { get; set; }
        public int OpenOrders { get; set; }             // chờ thanh toán + đang xử lý
        public decimal TotalRevenue { get; set; }       // không tính đơn đã hủy

        // JSON cho Google Charts: [["Tháng","Doanh thu"],["10/2026",1000000],...]
        public string RevenueByMonthJson { get; set; }
        public string OrdersByStatusJson { get; set; }
        public string TopProductsJson { get; set; }

        public List<Order> RecentOrders { get; set; }
    }
}
