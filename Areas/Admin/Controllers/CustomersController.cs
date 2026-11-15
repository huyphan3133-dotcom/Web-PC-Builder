using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using PagedList;
using Web_PC_Builder.Helpers;
using Web_PC_Builder.Models;
using Web_PC_Builder.Models.ViewModel;
using AppUser = Web_PC_Builder.Models.User;

namespace Web_PC_Builder.Areas.Admin.Controllers
{
    public class CustomersController : AdminBaseController
    {
        // GET: Admin/Customers?searchTerm=&sortOrder=&page=
        public ActionResult Index(string searchTerm, string sortOrder, int page = 1)
        {
            if (page < 1) page = 1;

            IQueryable<Customer> query = db.Customers;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string t = searchTerm.Trim();
                query = query.Where(c => c.CustomerName.Contains(t)
                                         || c.CustomerPhone.Contains(t)
                                         || c.CustomerEmail.Contains(t)
                                         || c.Username.Contains(t));
            }

            IQueryable<CustomerRowVM> rows = query.Select(c => new CustomerRowVM
            {
                CustomerID = c.CustomerID,
                CustomerName = c.CustomerName,
                CustomerPhone = c.CustomerPhone,
                CustomerEmail = c.CustomerEmail,
                Username = c.Username,
                OrderCount = c.Orders.Count(),
                TotalSpent = c.Orders
                    .Where(o => o.OrderStatus != OrderConst.Cancelled)
                    .Sum(o => (decimal?)o.TotalAmount)
            });

            switch (sortOrder)
            {
                case "name_asc":
                    rows = rows.OrderBy(r => r.CustomerName).ThenBy(r => r.CustomerID);
                    break;
                case "name_desc":
                    rows = rows.OrderByDescending(r => r.CustomerName).ThenBy(r => r.CustomerID);
                    break;
                case "orders_desc":
                    rows = rows.OrderByDescending(r => r.OrderCount).ThenBy(r => r.CustomerID);
                    break;
                case "spent_desc":
                    rows = rows.OrderByDescending(r => r.TotalSpent).ThenBy(r => r.CustomerID);
                    break;
                default:
                    rows = rows.OrderByDescending(r => r.CustomerID);
                    break;
            }

            var vm = new CustomerSearchVM
            {
                SearchTerm = searchTerm,
                SortOrder = sortOrder,
                PageNumber = page,
                PageSize = DefaultPageSize,
                Customers = rows.ToPagedList(page, DefaultPageSize)
            };
            return View(vm);
        }

        // GET: Admin/Customers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            int customerId = id.Value;
            Customer customer = db.Customers
                .Include(c => c.Orders)
                .SingleOrDefault(c => c.CustomerID == customerId);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        // GET: Admin/Customers/Create
        public ActionResult Create()
        {
            return View(new CustomerCreateVM());
        }

        // POST: Admin/Customers/Create  (tạo [User] vai trò C + [Customer])
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CustomerCreateVM model)
        {
            string username = (model.Username ?? "").Trim();

            if (!string.IsNullOrEmpty(username) && db.Users.Any(u => u.Username == username))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            db.Users.Add(new AppUser
            {
                Username = username,
                Password = PasswordHelper.Hash(username, model.Password),
                UserRole = "C"
            });

            db.Customers.Add(new Customer
            {
                Username = username,
                CustomerName = model.CustomerName.Trim(),
                CustomerPhone = model.CustomerPhone.Trim(),
                CustomerEmail = string.IsNullOrWhiteSpace(model.CustomerEmail) ? null : model.CustomerEmail.Trim(),
                CustomerAddress = string.IsNullOrWhiteSpace(model.CustomerAddress) ? null : model.CustomerAddress.Trim()
            });

            db.SaveChanges();
            TempData["Success"] = "Đã thêm khách hàng \"" + model.CustomerName.Trim() + "\".";
            return RedirectToAction("Index");
        }

        // GET: Admin/Customers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = db.Customers.Find(id.Value);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        // POST: Admin/Customers/Edit/5  (không cho đổi Username vì là khóa ngoại tới [User])
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CustomerID,CustomerName,CustomerPhone,CustomerEmail,CustomerAddress,Username")] Customer customer)
        {
            Customer existing = db.Customers.Find(customer.CustomerID);
            if (existing == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                existing.CustomerName = customer.CustomerName.Trim();
                existing.CustomerPhone = customer.CustomerPhone.Trim();
                existing.CustomerEmail = string.IsNullOrWhiteSpace(customer.CustomerEmail) ? null : customer.CustomerEmail.Trim();
                existing.CustomerAddress = string.IsNullOrWhiteSpace(customer.CustomerAddress) ? null : customer.CustomerAddress.Trim();
                db.SaveChanges();
                TempData["Success"] = "Đã cập nhật khách hàng.";
                return RedirectToAction("Index");
            }

            // Hiển thị lại form: Username luôn lấy từ DB, không tin giá trị client gửi lên.
            customer.Username = existing.Username;
            return View(customer);
        }

        // GET: Admin/Customers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            int customerId = id.Value;
            Customer customer = db.Customers
                .Include(c => c.Orders)
                .SingleOrDefault(c => c.CustomerID == customerId);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        // POST: Admin/Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Customer customer = db.Customers
                .Include(c => c.Orders)
                .SingleOrDefault(c => c.CustomerID == id);
            if (customer == null)
            {
                return HttpNotFound();
            }

            if (customer.Orders.Count > 0)
            {
                ModelState.AddModelError("", "Không thể xóa: khách hàng này đã có " + customer.Orders.Count + " đơn hàng.");
                return View("Delete", customer);
            }

            string username = customer.Username;
            string name = customer.CustomerName;

            db.Customers.Remove(customer);
            db.SaveChanges();

            // Xóa luôn tài khoản đăng nhập (vai trò C) nếu không còn khách hàng nào dùng nó.
            AppUser account = db.Users.Find(username);
            if (account != null
                && (account.UserRole ?? "").Trim() == "C"
                && !db.Customers.Any(c => c.Username == username))
            {
                db.Users.Remove(account);
                db.SaveChanges();
            }

            TempData["Success"] = "Đã xóa khách hàng \"" + name + "\".";
            return RedirectToAction("Index");
        }
    }
}
