using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Web_PC_Builder.Models;

namespace Web_PC_Builder.Controllers
{
    public class ProductController : Controller
    {
        public ActionResult Home()
        {
            ViewBag.StockJson = BuildStockJson();
            return View();
        }

        
        public ActionResult Details(int? id)
        {
            ViewBag.StockJson = BuildStockJson();
            return View();
        }

    
        private static string BuildStockJson()
        {
            using (var db = new My_StoreEntities())
            {
                // GroupBy phòng trường hợp trùng tên (ToDictionary sẽ ném lỗi nếu trùng khóa).
                var stock = db.Products
                    .Select(p => new { p.ProductName, p.Stock })
                    .ToList()
                    .GroupBy(p => p.ProductName)
                    .ToDictionary(g => g.Key, g => g.First().Stock);

                // Thay "<" để tên sản phẩm chứa "</script>" không phá thẻ <script> trong view.
                return new JavaScriptSerializer().Serialize(stock).Replace("<", "\\u003c");
            }
        }
    }
}