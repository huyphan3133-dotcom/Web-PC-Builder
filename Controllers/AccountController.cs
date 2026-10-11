
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Web_PC_Builder.Controllers
{
    public class AccountController : Controller
    {
        public class DemoUser
        {
            public string FullName { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public ActionResult Index()
        {
            return View();
        }

        // Mở trang đăng nhập
        [HttpGet]
        public ActionResult Login()
        {
            return View("~/Views/Account/Login.cshtml");
        }

        // Xử lý đăng nhập
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password)
        {
            var users = Session["DemoUsers"] as List<DemoUser>;

            if (users == null)
            {
                ViewBag.Error = "Bạn chưa có tài khoản. Hãy đăng ký trước.";
                return View("~/Views/Account/Login.cshtml");
            }

            string enteredEmail = (email ?? "").Trim();

            var user = users.FirstOrDefault(u =>
                u.Email.Equals(
                    enteredEmail,
                    StringComparison.OrdinalIgnoreCase)
                && u.Password == password);

            if (user == null)
            {
                ViewBag.Error = "Email hoặc mật khẩu không đúng.";
                return View("~/Views/Account/Login.cshtml");
            }

            Session["CurrentUser"] = user;

            return RedirectToAction("MyProfile");
        }

        // Mở trang đăng ký
        [HttpGet]
        public ActionResult Register()
        {
            return View("~/Views/Account/Register.cshtml");
        }

        // Xử lý đăng ký
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(
            string fullName,
            string email,
            string password,
            string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(confirmPassword))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
                return View("~/Views/Account/Register.cshtml");
            }

            if (password.Length < 6)
            {
                ViewBag.Error = "Mật khẩu phải có ít nhất 6 ký tự.";
                return View("~/Views/Account/Register.cshtml");
            }

            if (password != confirmPassword)
            {
                ViewBag.Error = "Hai mật khẩu không khớp.";
                return View("~/Views/Account/Register.cshtml");
            }

            var users = Session["DemoUsers"] as List<DemoUser>;

            if (users == null)
            {
                users = new List<DemoUser>();
            }

            string enteredEmail = email.Trim();

            if (users.Any(u =>
                u.Email.Equals(
                    enteredEmail,
                    StringComparison.OrdinalIgnoreCase)))
            {
                ViewBag.Error = "Email này đã được đăng ký.";
                return View("~/Views/Account/Register.cshtml");
            }

            users.Add(new DemoUser
            {
                FullName = fullName.Trim(),
                Email = enteredEmail,
                Password = password
            });

            Session["DemoUsers"] = users;

            TempData["Success"] =
                "Đăng ký thành công! Hãy đăng nhập.";

            return RedirectToAction("Login");
        }

        // Trang hồ sơ
        [HttpGet]
        public ActionResult MyProfile()
        {
            if (Session["CurrentUser"] == null)
            {
                return RedirectToAction("Login");
            }

            var user = Session["CurrentUser"] as DemoUser;

            ViewBag.FullName = user.FullName;
            ViewBag.Email = user.Email;

            return View("~/Views/Account/Profile.cshtml");
        }

        // Đăng xuất
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            Session.Remove("CurrentUser");
            return RedirectToAction("Login");
        }
    }
}
