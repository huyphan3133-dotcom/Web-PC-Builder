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
        private readonly My_StoreEntities db = new My_StoreEntities();

        // GET: Product
        public ActionResult Home()
        {
            ViewBag.StockJson = StockJson();
            return View();
        }

        public ActionResult Details()
        {
            ViewBag.StockJson = StockJson();
            return View();
        }

        // Tồn kho theo tên sản phẩm, dạng JSON: {"Tên SP": 20, ...}
        // (danh sách sản phẩm trên web đang nằm trong JS nên nối với DB bằng tên, giống PlaceOrder)
        private string StockJson()
        {
            try
            {
                Dictionary<string, int> stock = db.Products
                    .Select(p => new { p.ProductName, p.Stock })
                    .ToList()
                    .GroupBy(x => x.ProductName)
                    .ToDictionary(g => g.Key, g => g.First().Stock);
                return new JavaScriptSerializer().Serialize(stock);
            }
            catch (Exception)
            {
                return "{}";   // DB lỗi / chưa có cột Stock -> web vẫn chạy, chỉ không hiện tồn kho
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
