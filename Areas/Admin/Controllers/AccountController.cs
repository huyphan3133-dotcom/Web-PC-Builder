using System.Linq;
using System.Web.Mvc;
using Web_PC_Builder.Helpers;
using Web_PC_Builder.Models;
using Web_PC_Builder.Models.ViewModel;

namespace Web_PC_Builder.Areas.Admin.Controllers
{
    /// <summary>Đăng nhập / đăng xuất trang quản trị. Chỉ tài khoản có UserRole = 'A' mới vào được.</summary>
    public class AccountController : Controller
    {
        private My_StoreEntities db = new My_StoreEntities();

        // GET: Admin/Account/Login
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            if (Session[AdminAuthorizeAttribute.SessionKey] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View(new AdminLoginVM());
        }

        // POST: Admin/Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(AdminLoginVM model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string username = model.Username.Trim();
            Models.User user = db.Users.SingleOrDefault(u => u.Username == username);

            bool isLegacy = false;
            bool ok = user != null
                      && user.UserRole.Trim() == "A"
                      && PasswordHelper.Verify(user.Username, model.Password, user.Password, out isLegacy);

            if (!ok)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng, hoặc tài khoản không có quyền quản trị.");
                return View(model);
            }

            // Mật khẩu cũ đang lưu chữ thường -> băm lại ngay lần đăng nhập đầu tiên.
            if (isLegacy)
            {
                user.Password = PasswordHelper.Hash(user.Username, model.Password);
                db.SaveChanges();
            }

            Session[AdminAuthorizeAttribute.SessionKey] = user.Username;

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        // POST: Admin/Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            Session.Remove(AdminAuthorizeAttribute.SessionKey);
            Session.Abandon();
            return RedirectToAction("Login");
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
