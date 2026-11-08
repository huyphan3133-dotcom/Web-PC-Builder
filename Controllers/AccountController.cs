using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web.Mvc;
using Web_PC_Builder.Helpers;
using Web_PC_Builder.Models;
using Web_PC_Builder.Models.ViewModel;
using AppUser = Web_PC_Builder.Models.User;

namespace Web_PC_Builder.Controllers
{
    /// <summary>Đăng ký / đăng nhập / đăng xuất / hồ sơ của khách hàng (UserRole = 'C').</summary>
    public class AccountController : Controller
    {
        private readonly My_StoreEntities db = new My_StoreEntities();

        // ---------------- ĐĂNG NHẬP ----------------

        // GET: Account/Login
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            if (IsLoggedIn()) return RedirectToAction("Profile");
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginVM());
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginVM model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (!ModelState.IsValid)
            {
                ViewBag.Error = FirstError();
                return View(model);
            }

            string username = NormalizeEmail(model.Email);
            AppUser user = db.Users.SingleOrDefault(u => u.Username == username);

            bool isLegacy = false;
            bool ok = user != null
                      && (user.UserRole ?? "").Trim() == "C"
                      && PasswordHelper.Verify(user.Username, model.Password, user.Password, out isLegacy);

            if (!ok)
            {
                ViewBag.Error = "Email hoặc mật khẩu không đúng.";
                return View(model);
            }

            // Mật khẩu cũ lưu chữ thường -> băm lại ở lần đăng nhập đầu tiên.
            if (isLegacy)
            {
                user.Password = PasswordHelper.Hash(user.Username, model.Password);
                db.SaveChanges();
            }

            Customer customer = db.Customers.FirstOrDefault(c => c.Username == user.Username);
            SignIn(user.Username, customer != null ? customer.CustomerName : user.Username);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        // ---------------- ĐĂNG KÝ ----------------

        // GET: Account/Register
        [HttpGet]
        public ActionResult Register()
        {
            if (IsLoggedIn()) return RedirectToAction("Profile");
            return View(new RegisterVM());
        }

        // POST: Account/Register  (tạo [User] vai trò C + [Customer], rồi tự đăng nhập)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterVM model)
        {
            // Cắt khoảng trắng đầu/cuối, gộp nhiều khoảng trắng liên tiếp trong tên thành 1,
            // rồi kiểm tra lại toàn bộ model trên dữ liệu đã chuẩn hóa.
            model.FullName = CleanSpaces(model.FullName);
            model.Email = (model.Email ?? "").Trim();
            model.Phone = (model.Phone ?? "").Trim();
            ModelState.Clear();
            TryValidateModel(model);

            string username = NormalizeEmail(model.Email);

            if (!string.IsNullOrEmpty(username) && db.Users.Any(u => u.Username == username))
            {
                ModelState.AddModelError("Email", "Email này đã được đăng ký.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Error = FirstError();
                return View(model);
            }

            string fullName = model.FullName.Trim();

            db.Users.Add(new AppUser
            {
                Username = username,
                Password = PasswordHelper.Hash(username, model.Password),
                UserRole = "C"
            });

            db.Customers.Add(new Customer
            {
                Username = username,
                CustomerName = fullName,
                CustomerPhone = model.Phone.Trim(),
                CustomerEmail = username
            });

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                ViewBag.Error = "Không thể tạo tài khoản lúc này. Vui lòng thử lại.";
                return View(model);
            }

            SignIn(username, fullName);
            return RedirectToAction("Profile");
        }

        // ---------------- ĐĂNG XUẤT ----------------

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            // Chỉ xóa phiên khách hàng, không đụng tới phiên admin (dùng chung Session).
            Session.Remove(CustomerAuthorizeAttribute.SessionKey);
            Session.Remove(CustomerAuthorizeAttribute.SessionNameKey);
            return RedirectToAction("Index", "Home");
        }

        // ---------------- HỒ SƠ ----------------

        // GET: Account/Profile
        [CustomerAuthorize]
        public ActionResult Profile()
        {
            OrderAutoUpdater.Run(db);

            string username = (string)Session[CustomerAuthorizeAttribute.SessionKey];
            Customer customer = db.Customers.FirstOrDefault(c => c.Username == username);

            var vm = new ProfileVM
            {
                Username = username,
                FullName = customer != null ? customer.CustomerName : username,
                Email = customer != null && !string.IsNullOrEmpty(customer.CustomerEmail) ? customer.CustomerEmail : username,
                Phone = customer != null ? customer.CustomerPhone : null,
                Address = customer != null ? customer.CustomerAddress : null
            };

            if (customer != null)
            {
                int customerId = customer.CustomerID;
                vm.Orders = db.Orders
                    .Where(o => o.CustomerID == customerId)
                    .OrderByDescending(o => o.OrderDate)
                    .Select(o => new OrderRowVM2
                    {
                        OrderID = o.OrderID,
                        OrderDate = o.OrderDate,
                        TotalAmount = o.TotalAmount,
                        OrderStatus = o.OrderStatus
                    })
                    .ToList();
            }

            return View(vm);
        }

        // ---------------- HỖ TRỢ ----------------

        private bool IsLoggedIn()
        {
            return Session[CustomerAuthorizeAttribute.SessionKey] != null;
        }

        private void SignIn(string username, string displayName)
        {
            Session[CustomerAuthorizeAttribute.SessionKey] = username;
            Session[CustomerAuthorizeAttribute.SessionNameKey] = displayName;
        }

        private static string CleanSpaces(string s)
        {
            return System.Text.RegularExpressions.Regex.Replace((s ?? "").Trim(), @"\s+", " ");
        }

        private static string NormalizeEmail(string email)
        {
            return (email ?? "").Trim().ToLowerInvariant();
        }

        private string FirstError()
        {
            return ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault(m => !string.IsNullOrEmpty(m));
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
