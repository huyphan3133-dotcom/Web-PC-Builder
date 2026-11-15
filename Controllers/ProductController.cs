using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Globalization;
namespace Web_PC_Builder.Controllers
{
    public class ProductController : Controller
    {
        //private MyStoreEntities db = new MyStoreEntities();

        //public ActionResult Home() { return View(); }

        //public ActionResult Details(int id)
        //{
        //    var p = db.Products.Find(id);
        //    if (p == null) return HttpNotFound();
        //    ViewBag.Id = id;
        //    return View();
        //}

        //// Trả JSON danh sách sản phẩm, lọc theo q và loai
        //[ChildActionOnly]
        //public ActionResult ProductsJson(string q, string loai)
        //{
        //    var list = db.Products.ToList();   // sửa Products cho đúng tên DbSet

        //    if (!string.IsNullOrWhiteSpace(loai))
        //        list = list.Where(p => p.Category.CategoryName == loai).ToList();

        //    if (!string.IsNullOrWhiteSpace(q))
        //    {
        //        var words = Norm(q).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        //        list = list.Where(p =>
        //        {
        //            var hay = Norm(p.ProductName + " " + p.Brand);
        //            return words.All(w => hay.Contains(w));
        //        }).ToList();
        //    }

        //    var data = list.Select(p => new {
        //        id = p.ProductID,
        //        name = p.ProductName,
        //        brand = p.Brand,
        //        cat = p.Category.CategoryName,
        //        price = (long)p.Price,
        //        img = p.ImageUrl,
        //        stock = p.Stock
        //        // thêm các trường card đang dùng (old, specs, tags...) cho khớp
        //    });

        //    return Content(new System.Web.Script.Serialization.JavaScriptSerializer
        //    { MaxJsonLength = int.MaxValue }.Serialize(data), "application/json");
        //}

        //static string Norm(string s)
        //{
        //    s = (s ?? "").ToLowerInvariant().Replace('đ', 'd').Normalize(NormalizationForm.FormD);
        //    var sb = new StringBuilder();
        //    foreach (var c in s)
        //        if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
        //    return sb.ToString();
        //}
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Home()

        {
            return View();
        }
    }
}