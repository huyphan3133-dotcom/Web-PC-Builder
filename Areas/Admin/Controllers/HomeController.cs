using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
using Web_PC_Builder.Helpers;
using Web_PC_Builder.Models;
using Web_PC_Builder.Models.ViewModel;

namespace Web_PC_Builder.Areas.Admin.Controllers
{
    /// <summary>Trang Home của Admin: số liệu tổng quan + biểu đồ Google Charts.</summary>
    [AdminAuthorize]
    public class HomeController : Controller
    {
        private My_StoreEntities db = new My_StoreEntities();

        // GET: Admin/Home
        public ActionResult Index()
        {
            var vm = new DashboardVM();

            vm.TotalProducts = db.Products.Count();
            vm.TotalCategories = db.Categories.Count();
            vm.TotalCustomers = db.Customers.Count();
            vm.TotalOrders = db.Orders.Count();
            vm.OpenOrders = db.Orders.Count(o => o.OrderStatus == OrderConst.WaitingPayment
                                                 || o.OrderStatus == OrderConst.Processing);
            vm.TotalRevenue = db.Orders
                .Where(o => o.OrderStatus != OrderConst.Cancelled)
                .Sum(o => (decimal?)o.TotalAmount) ?? 0m;

            // ---- Doanh thu 6 tháng gần nhất
            DateTime firstMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-5);
            var monthly = db.Orders
                .Where(o => o.OrderDate >= firstMonth && o.OrderStatus != OrderConst.Cancelled)
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.TotalAmount) })
                .ToList();

            var revenueRows = new List<object[]>();
            revenueRows.Add(new object[] { "Tháng", "Doanh thu (VNĐ)" });
            for (int i = 0; i < 6; i++)
            {
                DateTime m = firstMonth.AddMonths(i);
                var found = monthly.FirstOrDefault(x => x.Year == m.Year && x.Month == m.Month);
                revenueRows.Add(new object[] { m.ToString("MM/yyyy"), found == null ? 0m : found.Total });
            }
            vm.RevenueByMonthJson = ToJson(revenueRows);

            // ---- Số đơn theo trạng thái
            var byStatus = db.Orders
                .GroupBy(o => o.OrderStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();
            var statusRows = new List<object[]>();
            statusRows.Add(new object[] { "Trạng thái", "Số đơn" });
            foreach (var s in byStatus)
            {
                statusRows.Add(new object[] { s.Status, s.Count });
            }
            vm.OrdersByStatusJson = ToJson(statusRows);

            // ---- Top 5 sản phẩm bán chạy (không tính đơn đã hủy)
            var top = db.OrderDetails
                .Where(d => d.Order.OrderStatus != OrderConst.Cancelled)
                .GroupBy(d => d.ProductID)
                .Select(g => new { ProductID = g.Key, Qty = g.Sum(x => x.Quantity) })
                .OrderByDescending(x => x.Qty)
                .Take(5)
                .ToList();
            var topIds = top.Select(t => t.ProductID).ToList();
            var names = db.Products
                .Where(p => topIds.Contains(p.ProductID))
                .Select(p => new { p.ProductID, p.ProductName })
                .ToList()
                .ToDictionary(p => p.ProductID, p => p.ProductName);

            var topRows = new List<object[]>();
            topRows.Add(new object[] { "Sản phẩm", "Số lượng đã bán" });
            foreach (var t in top)
            {
                string name = names.ContainsKey(t.ProductID) ? names[t.ProductID] : ("#" + t.ProductID);
                if (name.Length > 28) name = name.Substring(0, 28) + "…";
                topRows.Add(new object[] { name, t.Qty });
            }
            vm.TopProductsJson = ToJson(topRows);

            // ---- 5 đơn mới nhất
            vm.RecentOrders = db.Orders
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate)
                .ThenByDescending(o => o.OrderID)
                .Take(5)
                .ToList();

            return View(vm);
        }

        private static string ToJson(object value)
        {
            // EscapeHtml: tránh chuỗi "</script>" trong dữ liệu làm hỏng trang khi nhúng JSON vào <script>.
            return JsonConvert.SerializeObject(value, new JsonSerializerSettings
            {
                StringEscapeHandling = StringEscapeHandling.EscapeHtml
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
