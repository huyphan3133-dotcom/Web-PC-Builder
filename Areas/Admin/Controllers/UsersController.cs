using System;
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
    /// <summary>Quản lý tài khoản quản trị (UserRole = 'A'). Tài khoản khách hàng được quản lý ở mục Khách hàng.</summary>
    public class UsersController : AdminBaseController
    {
        // GET: Admin/Users?searchTerm=&page=
        public ActionResult Index(string searchTerm, int page = 1)
        {
            if (page < 1) page = 1;

            IQueryable<AppUser> query = db.Users.Where(u => u.UserRole == "A");

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string t = searchTerm.Trim();
                query = query.Where(u => u.Username.Contains(t));
            }

            var vm = new UserSearchVM
            {
                SearchTerm = searchTerm,
                PageNumber = page,
                PageSize = DefaultPageSize,
                Users = query.OrderBy(u => u.Username).ToPagedList(page, DefaultPageSize)
            };
            return View(vm);
        }

        // GET: Admin/Users/Details?username=admin
        public ActionResult Details(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AppUser user = FindAdmin(username);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Admin/Users/Create
        public ActionResult Create()
        {
            return View(new UserCreateVM());
        }

        // POST: Admin/Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserCreateVM model)
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
                UserRole = "A"
            });
            db.SaveChanges();

            TempData["Success"] = "Đã tạo tài khoản quản trị \"" + username + "\".";
            return RedirectToAction("Index");
        }

        // GET: Admin/Users/Edit?username=admin
        public ActionResult Edit(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AppUser user = FindAdmin(username);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(new UserEditVM { Username = user.Username });
        }

        // POST: Admin/Users/Edit  (đổi mật khẩu)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserEditVM model)
        {
            AppUser user = string.IsNullOrEmpty(model.Username) ? null : FindAdmin(model.Username);
            if (user == null)
            {
                return HttpNotFound();
            }

            if (!ModelState.IsValid)
            {
                model.Username = user.Username;
                return View(model);
            }

            user.Password = PasswordHelper.Hash(user.Username, model.NewPassword);
            db.SaveChanges();

            TempData["Success"] = "Đã đổi mật khẩu cho tài khoản \"" + user.Username + "\".";
            return RedirectToAction("Index");
        }

        // GET: Admin/Users/Delete?username=abc
        public ActionResult Delete(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AppUser user = FindAdmin(username);
            if (user == null)
            {
                return HttpNotFound();
            }
            ViewBag.CannotDeleteReason = GetCannotDeleteReason(user);
            return View(user);
        }

        // POST: Admin/Users/Delete?username=abc
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string username)
        {
            AppUser user = string.IsNullOrEmpty(username) ? null : FindAdmin(username);
            if (user == null)
            {
                return HttpNotFound();
            }

            string reason = GetCannotDeleteReason(user);
            if (reason != null)
            {
                ViewBag.CannotDeleteReason = reason;
                ModelState.AddModelError("", reason);
                return View("Delete", user);
            }

            db.Users.Remove(user);
            db.SaveChanges();

            TempData["Success"] = "Đã xóa tài khoản \"" + username + "\".";
            return RedirectToAction("Index");
        }

        // ------------------------------------------------------------------ helpers

        private AppUser FindAdmin(string username)
        {
            string name = username.Trim();
            return db.Users.SingleOrDefault(u => u.Username == name && u.UserRole == "A");
        }

        /// <summary>Trả về lý do không được xóa tài khoản, hoặc null nếu xóa được.</summary>
        private string GetCannotDeleteReason(AppUser user)
        {
            if (string.Equals(user.Username, CurrentAdmin, StringComparison.OrdinalIgnoreCase))
            {
                return "Không thể xóa tài khoản đang đăng nhập.";
            }
            if (db.Users.Count(u => u.UserRole == "A") <= 1)
            {
                return "Không thể xóa tài khoản quản trị cuối cùng.";
            }
            string name = user.Username;
            if (db.Customers.Any(c => c.Username == name))
            {
                return "Tài khoản này đang được gắn với khách hàng nên không xóa được.";
            }
            return null;
        }
    }
}
