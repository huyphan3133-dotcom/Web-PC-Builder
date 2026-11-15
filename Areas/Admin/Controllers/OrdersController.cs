using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using PagedList;
using Web_PC_Builder.Helpers;
using Web_PC_Builder.Models;
using Web_PC_Builder.Models.ViewModel;

namespace Web_PC_Builder.Areas.Admin.Controllers
{
    public class OrdersController : AdminBaseController
    {
        // GET: Admin/Orders?searchTerm=&status=&customerId=&sortOrder=&page=
        public ActionResult Index(string searchTerm, string status, int? customerId, string sortOrder, int page = 1)
        {
            if (page < 1) page = 1;

            var vm = new OrderSearchVM
            {
                SearchTerm = searchTerm,
                Status = status,
                CustomerID = customerId,
                SortOrder = sortOrder,
                PageNumber = page,
                PageSize = DefaultPageSize
            };

            IQueryable<Order> query = db.Orders.Include(o => o.Customer);

            if (customerId.HasValue)
            {
                int cid = customerId.Value;
                query = query.Where(o => o.CustomerID == cid);
                vm.Customer = db.Customers.Find(cid);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                string st = status.Trim();
                query = query.Where(o => o.OrderStatus == st);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string t = searchTerm.Trim();
                int orderId;
                if (int.TryParse(t.TrimStart('#'), out orderId))
                {
                    query = query.Where(o => o.OrderID == orderId
                                             || o.Customer.CustomerName.Contains(t)
                                             || o.Customer.CustomerPhone.Contains(t));
                }
                else
                {
                    query = query.Where(o => o.Customer.CustomerName.Contains(t)
                                             || o.Customer.CustomerPhone.Contains(t));
                }
            }

            switch (sortOrder)
            {
                case "date_asc":
                    query = query.OrderBy(o => o.OrderDate).ThenBy(o => o.OrderID);
                    break;
                case "amount_desc":
                    query = query.OrderByDescending(o => o.TotalAmount).ThenByDescending(o => o.OrderID);
                    break;
                case "amount_asc":
                    query = query.OrderBy(o => o.TotalAmount).ThenByDescending(o => o.OrderID);
                    break;
                default:
                    query = query.OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.OrderID);
                    break;
            }

            vm.Orders = query.ToPagedList(page, DefaultPageSize);
            return View(vm);
        }

        // GET: Admin/Orders/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            int orderId = id.Value;
            Order order = db.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails.Select(d => d.Product))
                .SingleOrDefault(o => o.OrderID == orderId);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // POST: Admin/Orders/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string orderStatus, string paymentStatus)
        {
            // Nạp kèm chi tiết đơn + sản phẩm để hoàn kho khi hủy.
            Order order = db.Orders
                .Include(o => o.OrderDetails.Select(d => d.Product))
                .SingleOrDefault(o => o.OrderID == id);
            if (order == null)
            {
                return HttpNotFound();
            }

            if (OrderConst.IsLocked(order.OrderStatus))
            {
                TempData["Error"] = "Đơn hàng #" + id + " đã ở trạng thái \"" + order.OrderStatus.Trim() + "\" nên không thể chỉnh sửa.";
                return RedirectToAction("Details", new { id = id });
            }

            if (!OrderConst.Statuses.Contains(orderStatus) || !OrderConst.PaymentStatuses.Contains(paymentStatus))
            {
                TempData["Error"] = "Trạng thái không hợp lệ.";
                return RedirectToAction("Details", new { id = id });
            }

            // Quy tắc nghiệp vụ dùng chung (cùng luật với JS trên form): kiểm tra ở SERVER, không tin client.
            string error = OrderRules.Validate(orderStatus, paymentStatus, order.PaymentMethod);
            if (error != null)
            {
                TempData["Error"] = error;
                return RedirectToAction("Details", new { id = id });
            }

            // Chỉ hoàn kho một lần: khi đơn CHUYỂN sang "Đã hủy" (đơn đã hủy sẽ bị IsLocked chặn ở trên).
            bool becomingCancelled = orderStatus == OrderConst.Cancelled
                                     && order.OrderStatus != OrderConst.Cancelled;

            if (order.OrderStatus != orderStatus)
            {
                order.StatusUpdatedAt = DateTime.Now;   // đếm lại thời gian từ lúc đổi trạng thái
            }
            order.OrderStatus = orderStatus;
            order.PaymentStatus = paymentStatus;

            if (becomingCancelled)
            {
                RestoreStock(order);   // hoàn tồn kho, cùng lần SaveChanges bên dưới
            }

            db.SaveChanges();

            TempData["Success"] = becomingCancelled
                ? "Đã hủy đơn hàng #" + id + " và hoàn lại tồn kho."
                : "Đã cập nhật đơn hàng #" + id + ".";
            return RedirectToAction("Details", new { id = id });
        }

        /// <summary>
        /// Hoàn lại tồn kho khi đơn bị hủy: cộng Quantity của từng dòng chi tiết trả về Product.Stock.
        /// Không gọi SaveChanges ở đây: UpdateStatus lưu cùng lúc với việc đổi trạng thái đơn.
        /// Nếu TV1 đã có hàm RestoreStock riêng thì thay thân hàm này bằng lời gọi hàm đó.
        /// </summary>
        private void RestoreStock(Order order)
        {
            foreach (var d in order.OrderDetails)
            {
                Product product = d.Product ?? db.Products.Find(d.ProductID);
                if (product != null)
                {
                    product.Stock += d.Quantity;
                }
            }
        }
    }
}
