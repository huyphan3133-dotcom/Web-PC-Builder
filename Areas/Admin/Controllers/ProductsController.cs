using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PagedList;
using Web_PC_Builder.Helpers;
using Web_PC_Builder.Models;
using Web_PC_Builder.Models.ViewModel;

namespace Web_PC_Builder.Areas.Admin.Controllers
{
    public class ProductsController : AdminBaseController
    {
        private const int MaxUploadBytes = 5 * 1024 * 1024; // 5 MB
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg" };

        // GET: Admin/Products?searchTerm=&minPrice=&maxPrice=&sortOrder=&page=
        public ActionResult Index(string searchTerm, decimal? minPrice, decimal? maxPrice, string sortOrder, int page = 1)
        {
            if (page < 1) page = 1;

            IQueryable<Product> query = db.Products.Include(p => p.Category);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string t = searchTerm.Trim();
                query = query.Where(p => p.ProductName.Contains(t)
                                         || p.ProductDescription.Contains(t)
                                         || p.Brand.Contains(t)
                                         || p.Category.CategoryName.Contains(t));
            }
            if (minPrice.HasValue)
            {
                decimal min = minPrice.Value;
                query = query.Where(p => p.ProductPrice >= min);
            }
            if (maxPrice.HasValue)
            {
                decimal max = maxPrice.Value;
                query = query.Where(p => p.ProductPrice <= max);
            }

            switch (sortOrder)
            {
                case "name_asc":
                    query = query.OrderBy(p => p.ProductName).ThenBy(p => p.ProductID);
                    break;
                case "name_desc":
                    query = query.OrderByDescending(p => p.ProductName).ThenBy(p => p.ProductID);
                    break;
                case "price_asc":
                    query = query.OrderBy(p => p.ProductPrice).ThenBy(p => p.ProductID);
                    break;
                case "price_desc":
                    query = query.OrderByDescending(p => p.ProductPrice).ThenBy(p => p.ProductID);
                    break;
                default:
                    query = query.OrderByDescending(p => p.ProductID);
                    break;
            }

            var vm = new ProductSearchVM
            {
                SearchTerm = searchTerm,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SortOrder = sortOrder,
                PageNumber = page,
                PageSize = DefaultPageSize,
                Products = query.ToPagedList(page, DefaultPageSize)
            };
            return View(vm);
        }

        // GET: Admin/Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            int productId = id.Value;
            Product product = db.Products.Include(p => p.Category).SingleOrDefault(p => p.ProductID == productId);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // GET: Admin/Products/Create
        public ActionResult Create()
        {
            FillCategories(null);
            return View(new Product());
        }

        // POST: Admin/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "CategoryID,ProductName,ProductDescription,ProductPrice,ProductImage,OldPrice,Brand,Stock")] Product product,
            HttpPostedFileBase UploadImg)
        {
            ValidateProduct(product, UploadImg);

            if (ModelState.IsValid)
            {
                if (HasFile(UploadImg))
                {
                    product.ProductImage = SaveImage(UploadImg);
                }
                db.Products.Add(product);
                db.SaveChanges();
                TempData["Success"] = "Đã thêm sản phẩm \"" + product.ProductName + "\".";
                return RedirectToAction("Index");
            }

            FillCategories(product.CategoryID);
            return View(product);
        }

        // GET: Admin/Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id.Value);
            if (product == null)
            {
                return HttpNotFound();
            }
            FillCategories(product.CategoryID);
            return View(product);
        }

        // POST: Admin/Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "ProductID,CategoryID,ProductName,ProductDescription,ProductPrice,ProductImage,OldPrice,Brand,Stock")] Product product,
            HttpPostedFileBase UploadImg)
        {
            ValidateProduct(product, UploadImg);

            if (ModelState.IsValid)
            {
                Product existing = db.Products.Find(product.ProductID);
                if (existing == null)
                {
                    return HttpNotFound();
                }

                existing.CategoryID = product.CategoryID;
                existing.ProductName = product.ProductName;
                existing.ProductDescription = product.ProductDescription;
                existing.ProductPrice = product.ProductPrice;
                existing.OldPrice = product.OldPrice;
                existing.Brand = product.Brand;
                existing.Stock = product.Stock;
                if (HasFile(UploadImg))
                {
                    existing.ProductImage = SaveImage(UploadImg);
                }

                db.SaveChanges();
                TempData["Success"] = "Đã cập nhật sản phẩm.";
                return RedirectToAction("Index");
            }

            FillCategories(product.CategoryID);
            return View(product);
        }

        // GET: Admin/Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            int productId = id.Value;
            Product product = db.Products.Include(p => p.Category).SingleOrDefault(p => p.ProductID == productId);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Include(p => p.Category).SingleOrDefault(p => p.ProductID == id);
            if (product == null)
            {
                return HttpNotFound();
            }

            // Sản phẩm đã nằm trong đơn hàng thì không xóa (vi phạm khóa ngoại OrderDetail -> Product).
            if (db.OrderDetails.Any(d => d.ProductID == id))
            {
                ModelState.AddModelError("", "Không thể xóa: sản phẩm này đã có trong đơn hàng.");
                return View("Delete", product);
            }

            db.Products.Remove(product);
            db.SaveChanges();
            TempData["Success"] = "Đã xóa sản phẩm \"" + product.ProductName + "\".";
            return RedirectToAction("Index");
        }

        // ------------------------------------------------------------------ helpers

        private void FillCategories(int? selectedId)
        {
            ViewBag.Categories = new SelectList(
                db.Categories.OrderBy(c => c.CategoryName).ToList(),
                "CategoryID", "CategoryName", selectedId);
        }

        private static bool HasFile(HttpPostedFileBase file)
        {
            return file != null && file.ContentLength > 0;
        }

        /// <summary>Kiểm tra danh mục tồn tại, giá hợp lệ và file ảnh (nếu có).</summary>
        private void ValidateProduct(Product product, HttpPostedFileBase upload)
        {
            if (product.ProductName != null) product.ProductName = product.ProductName.Trim();
            if (product.Brand != null) product.Brand = product.Brand.Trim();

            int categoryId = product.CategoryID;
            if (categoryId > 0 && !db.Categories.Any(c => c.CategoryID == categoryId))
            {
                ModelState.AddModelError("CategoryID", "Danh mục không tồn tại.");
            }

            if (HasFile(upload))
            {
                string ext = (Path.GetExtension(upload.FileName) ?? "").ToLowerInvariant();
                if (!AllowedExtensions.Contains(ext))
                {
                    ModelState.AddModelError("UploadImg", "Chỉ chấp nhận ảnh jpg, png, gif, webp hoặc svg.");
                }
                else if (upload.ContentLength > MaxUploadBytes)
                {
                    ModelState.AddModelError("UploadImg", "Ảnh quá lớn (tối đa 5 MB).");
                }
            }
        }

        /// <summary>Lưu ảnh vào ~/Content/Images với tên ngẫu nhiên, trả về đường dẫn dạng ~/Content/Images/xxx.png.</summary>
        private string SaveImage(HttpPostedFileBase file)
        {
            string ext = (Path.GetExtension(file.FileName) ?? "").ToLowerInvariant();
            string fileName = Guid.NewGuid().ToString("N") + ext;

            string folder = Server.MapPath(AppHelper.ImageFolder);
            Directory.CreateDirectory(folder);
            file.SaveAs(Path.Combine(folder, fileName));

            return AppHelper.ImageFolder + fileName;
        }
    }
}
