using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Web_PC_Builder.Helpers;
using Web_PC_Builder.Models;

namespace Web_PC_Builder.Controllers
{
    public class CartController : Controller
    {
        private readonly My_StoreEntities db = new My_StoreEntities();

        public const string PayCod = "Thanh toán khi nhận hàng (COD)";
        public const string PayBank = "Chuyển khoản ngân hàng";

        /// <summary>Một dòng trong giỏ hàng do trình duyệt gửi lên (giỏ hàng lưu ở localStorage).</summary>
        public class CartLine
        {
            public string name { get; set; }
            public decimal price { get; set; }
            public int qty { get; set; }
            public string cat { get; set; }
            public string brand { get; set; }
            public string file { get; set; }
            public string desc { get; set; }
        }

        // GET: Cart/Home
        public ActionResult Home()
        {
            return View();
        }

        // GET: Cart/Checkout  (bắt buộc đăng nhập để đơn hàng gắn với tài khoản)
        [CustomerAuthorize]
        public ActionResult Checkout()
        {
            string username = (string)Session[CustomerAuthorizeAttribute.SessionKey];
            Customer c = db.Customers.FirstOrDefault(x => x.Username == username);
            if (c != null)
            {
                ViewBag.FullName = c.CustomerName;
                ViewBag.Phone = c.CustomerPhone;
                ViewBag.Email = c.CustomerEmail;
                ViewBag.Address = c.CustomerAddress;
            }
            else
            {
                ViewBag.Email = username;
            }
            return View();
        }

        // POST: Cart/PlaceOrder  -> lưu [Order] + [OrderDetail] vào database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PlaceOrder(string fullName, string phone, string email, string address,
                                       string note, string pay, string items)
        {
            string username = Session[CustomerAuthorizeAttribute.SessionKey] as string;
            if (username == null)
            {
                return Fail("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", true);
            }

            fullName = Regex.Replace((fullName ?? "").Trim(), @"\s+", " ");
            phone = (phone ?? "").Trim();
            email = (email ?? "").Trim();
            address = Regex.Replace((address ?? "").Trim(), @"\s+", " ");
            note = (note ?? "").Trim();

            if (fullName.Length < 2) return Fail("Vui lòng nhập họ và tên.");
            if (!Regex.IsMatch(phone, @"^(0|\+84)\d{9,10}$")) return Fail("Số điện thoại không hợp lệ.");
            if (email.Length > 0 && !Regex.IsMatch(email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$")) return Fail("Email không hợp lệ.");
            if (address.Length < 8) return Fail("Vui lòng nhập địa chỉ đầy đủ.");
            if (pay != PayCod && pay != PayBank) return Fail("Phương thức thanh toán không hợp lệ.");

            // ---- đọc giỏ hàng ----
            List<CartLine> lines;
            try
            {
                lines = new JavaScriptSerializer().Deserialize<List<CartLine>>(items ?? "[]");
            }
            catch (Exception)
            {
                return Fail("Dữ liệu giỏ hàng không hợp lệ.");
            }
            if (lines == null || lines.Count == 0) return Fail("Giỏ hàng đang trống.");
            if (lines.Count > 50) return Fail("Giỏ hàng có quá nhiều sản phẩm.");

            // Gộp các dòng trùng tên sản phẩm
            var merged = new Dictionary<string, CartLine>(StringComparer.OrdinalIgnoreCase);
            foreach (CartLine l in lines)
            {
                string n = (l == null ? "" : (l.name ?? "")).Trim();
                if (n.Length == 0 || l.qty < 1 || l.qty > 99 || l.price <= 0 || l.price > 10000000000m)
                {
                    return Fail("Giỏ hàng có sản phẩm không hợp lệ. Vui lòng kiểm tra lại.");
                }
                CartLine existing;
                if (merged.TryGetValue(n, out existing))
                {
                    existing.qty = Math.Min(99, existing.qty + l.qty);
                }
                else
                {
                    l.name = n;
                    merged[n] = l;
                }
            }

            // ---- khách hàng ----
            Customer customer = db.Customers.FirstOrDefault(c => c.Username == username);
            if (customer == null)
            {
                customer = new Customer
                {
                    Username = username,
                    CustomerName = fullName,
                    CustomerPhone = phone,
                    CustomerEmail = email.Length > 0 ? email : username,
                    CustomerAddress = address
                };
                db.Customers.Add(customer);
            }

            // ---- sản phẩm + chi tiết đơn ----
            var order = new Order
            {
                Customer = customer,
                OrderDate = DateTime.Today,
                StatusUpdatedAt = DateTime.Now,
                PaymentStatus = OrderConst.PayPending,
                ShippingAddress = address,
                DeliveryMethod = "Giao hàng tận nơi",
                PaymentMethod = pay,
                OrderStatus = pay == PayCod ? OrderConst.Processing : OrderConst.WaitingPayment
            };

            string noteText = "Người nhận: " + fullName + " - SĐT: " + phone;
            if (email.Length > 0) noteText += " - Email: " + email;
            if (note.Length > 0) noteText += " - Ghi chú: " + note;
            order.Note = noteText;

            // Danh sách cần trừ kho: (ProductID, số lượng, tên). Việc trừ thật sự làm ở transaction bên dưới.
            var toDeduct = new List<Tuple<int, int, string>>();

            decimal total = 0m;
            foreach (CartLine l in merged.Values)
            {
                string productName = l.name;
                Product product = db.Products.FirstOrDefault(p => p.ProductName == productName);
                if (product == null)
                {
                    // Chỉ cho đặt sản phẩm đã có trong CSDL. Không tạo sản phẩm mới theo dữ liệu
                    // trình duyệt gửi lên (tên/giá do client tự khai có thể bị sửa tay trong localStorage).
                    return Fail("Sản phẩm \"" + productName + "\" không còn bán. Vui lòng xóa khỏi giỏ hàng và thử lại.");
                }

                // Kiểm tra sớm để báo lỗi dễ hiểu. Việc trừ kho (nguyên tử) nằm ở transaction bên dưới.
                if (product.Stock < l.qty)
                {
                    return Fail(product.Stock <= 0
                        ? "Sản phẩm \"" + product.ProductName + "\" đã hết hàng."
                        : "Sản phẩm \"" + product.ProductName + "\" chỉ còn " + product.Stock + " trong kho.");
                }
                toDeduct.Add(Tuple.Create(product.ProductID, l.qty, product.ProductName));

                order.OrderDetails.Add(new OrderDetail
                {
                    Product = product,
                    Quantity = l.qty,
                    UnitPrice = product.ProductPrice
                });
                total += product.ProductPrice * l.qty;
            }
            order.TotalAmount = total;
            db.Orders.Add(order);

            // ---- trừ kho + lưu đơn trong CÙNG một transaction ----
            // UPDATE ... WHERE Stock >= @qty là một câu lệnh nguyên tử: hai khách đặt cùng lúc
            // không thể cùng qua kiểm tra rồi trừ vượt tồn kho. Hết hàng giữa chừng -> rollback cả đơn.
            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    foreach (var d in toDeduct)
                    {
                        int affected = db.Database.ExecuteSqlCommand(
                            "UPDATE dbo.[Product] SET Stock = Stock - @p0 WHERE ProductID = @p1 AND Stock >= @p0",
                            d.Item2, d.Item1);

                        if (affected == 0)
                        {
                            tx.Rollback();
                            return Fail("Sản phẩm \"" + d.Item3 + "\" vừa hết hàng hoặc không đủ số lượng. Vui lòng kiểm tra lại giỏ hàng.");
                        }
                    }

                    db.SaveChanges();
                    tx.Commit();
                }
                catch (DbEntityValidationException)
                {
                    tx.Rollback();
                    return Fail("Dữ liệu đơn hàng không hợp lệ. Vui lòng kiểm tra lại thông tin.");
                }
                catch (DbUpdateException)
                {
                    tx.Rollback();
                    return Fail("Không thể lưu đơn hàng lúc này. Vui lòng thử lại.");
                }
            }

            return Json(new { ok = true, code = "#" + order.OrderID, total = order.TotalAmount });
        }

        private JsonResult Fail(string message, bool needLogin = false)
        {
            return Json(new { ok = false, login = needLogin, error = message });
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
