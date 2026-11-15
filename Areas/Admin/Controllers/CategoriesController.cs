using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Web_PC_Builder.Models;

namespace Web_PC_Builder.Areas.Admin.Controllers
{
    public class CategoriesController : AdminBaseController
    {
        // GET: Admin/Categories
        public ActionResult Index()
        {
            // Số sản phẩm của từng danh mục -> hiển thị ở cột "Số sản phẩm".
            Dictionary<int, int> counts = db.Products
                .GroupBy(p => p.CategoryID)
                .Select(g => new { CategoryID = g.Key, Total = g.Count() })
                .ToDictionary(x => x.CategoryID, x => x.Total);
            ViewBag.ProductCounts = counts;

            return View(db.Categories.OrderBy(c => c.CategoryID).ToList());
        }

        // GET: Admin/Categories/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id.Value);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // GET: Admin/Categories/Create
        public ActionResult Create()
        {
            return View(new Category());
        }

        // POST: Admin/Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CategoryName")] Category category)
        {
            NormalizeAndCheckName(category, 0);

            if (ModelState.IsValid)
            {
                db.Categories.Add(category);
                db.SaveChanges();
                TempData["Success"] = "Đã thêm danh mục \"" + category.CategoryName + "\".";
                return RedirectToAction("Index");
            }

            return View(category);
        }

        // GET: Admin/Categories/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id.Value);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // POST: Admin/Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CategoryID,CategoryName")] Category category)
        {
            NormalizeAndCheckName(category, category.CategoryID);

            if (ModelState.IsValid)
            {
                Category existing = db.Categories.Find(category.CategoryID);
                if (existing == null)
                {
                    return HttpNotFound();
                }
                existing.CategoryName = category.CategoryName;
                db.SaveChanges();
                TempData["Success"] = "Đã cập nhật danh mục.";
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // GET: Admin/Categories/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            int categoryId = id.Value;
            Category category = db.Categories.Find(categoryId);
            if (category == null)
            {
                return HttpNotFound();
            }
            ViewBag.ProductCount = db.Products.Count(p => p.CategoryID == categoryId);
            return View(category);
        }

        // POST: Admin/Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }

            int productCount = db.Products.Count(p => p.CategoryID == id);
            if (productCount > 0)
            {
                ViewBag.ProductCount = productCount;
                ModelState.AddModelError("", "Không thể xóa: danh mục đang có " + productCount + " sản phẩm.");
                return View("Delete", category);
            }

            db.Categories.Remove(category);
            db.SaveChanges();
            TempData["Success"] = "Đã xóa danh mục \"" + category.CategoryName + "\".";
            return RedirectToAction("Index");
        }

        /// <summary>Cắt khoảng trắng và không cho trùng tên danh mục (không phân biệt hoa/thường).</summary>
        private void NormalizeAndCheckName(Category category, int currentId)
        {
            if (category.CategoryName != null)
            {
                category.CategoryName = category.CategoryName.Trim();
            }
            if (!string.IsNullOrEmpty(category.CategoryName))
            {
                string name = category.CategoryName;
                bool exists = db.Categories.Any(c => c.CategoryName == name && c.CategoryID != currentId);
                if (exists)
                {
                    ModelState.AddModelError("CategoryName", "Tên danh mục này đã tồn tại.");
                }
            }
        }
    }
}
