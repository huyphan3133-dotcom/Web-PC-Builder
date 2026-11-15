using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using Web_PC_Builder.Models;

namespace Web_PC_Builder.Helpers
{
    /// <summary>
    /// Tự động chuyển trạng thái đơn hàng, tính từ lần đổi trạng thái gần nhất (cột StatusUpdatedAt):
    ///   Đang xử lý       -> Đang vận chuyển : sau ShippingAfterMinutes phút
    ///   Đang vận chuyển  -> Đã giao         : sau DeliveredAfterMinutes phút (đơn chưa thanh toán được đánh dấu Đã thanh toán, vì COD thu tiền khi giao)
    ///   Chờ thanh toán   -> Đã hủy          : sau UnpaidCancelAfterMinutes phút nếu chưa thanh toán
    /// Mỗi lần chạy chỉ đi 1 bước để dễ theo dõi. Đơn Đã giao / Đã hủy không bao giờ bị đổi nữa.
    /// Các mốc thời gian chỉnh trong Web.config (appSettings, đơn vị phút).
    /// </summary>
    public static class OrderAutoUpdater
    {
        public static int ShippingAfterMinutes { get { return ReadMinutes("OrderAuto.ShippingMinutes", 1440); } }
        public static int DeliveredAfterMinutes { get { return ReadMinutes("OrderAuto.DeliveredMinutes", 2880); } }
        public static int UnpaidCancelAfterMinutes { get { return ReadMinutes("OrderAuto.UnpaidCancelMinutes", 4320); } }

        private static readonly object Sync = new object();
        private static DateTime _lastRun = DateTime.MinValue;
        private static Timer _timer;

        private static int ReadMinutes(string key, int fallback)
        {
            int v;
            return int.TryParse(ConfigurationManager.AppSettings[key], out v) && v >= 0 ? v : fallback;
        }

        /// <summary>Hiển thị số phút dễ đọc: 90 -> "1 giờ 30 phút", 1440 -> "1 ngày".</summary>
        public static string Format(int minutes)
        {
            if (minutes % 1440 == 0 && minutes > 0) return (minutes / 1440) + " ngày";
            if (minutes >= 60)
            {
                int h = minutes / 60, m = minutes % 60;
                return m == 0 ? h + " giờ" : h + " giờ " + m + " phút";
            }
            return minutes + " phút";
        }

        /// <summary>Chạy nền định kỳ để đơn tự chuyển trạng thái kể cả khi không có ai mở trang.</summary>
        public static void StartBackground()
        {
            if (_timer != null) return;
            _timer = new Timer(delegate
            {
                try
                {
                    using (var db = new My_StoreEntities())
                    {
                        RunCore(db);
                    }
                }
                catch (Exception) { }
            }, null, TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(15));
        }

        /// <summary>Gọi khi vào trang (tối đa 1 lần / 10 giây).</summary>
        public static void Run(My_StoreEntities db)
        {
            lock (Sync)
            {
                if ((DateTime.Now - _lastRun).TotalSeconds < 10) return;
            }
            RunCore(db);
        }

        private static void RunCore(My_StoreEntities db)
        {
            if (!Monitor.TryEnter(Sync)) return;   // đang có luồng khác chạy
            try
            {
                _lastRun = DateTime.Now;

                int shipMin = ShippingAfterMinutes;
                int deliverMin = DeliveredAfterMinutes;
                int cancelMin = UnpaidCancelAfterMinutes;
                DateTime now = DateTime.Now;

                var open = db.Orders
                    .Where(o => o.OrderStatus == OrderConst.WaitingPayment
                                || o.OrderStatus == OrderConst.Processing
                                || o.OrderStatus == OrderConst.Shipping)
                    .ToList();

                bool changed = false;
                foreach (Order o in open)
                {
                    string status = (o.OrderStatus ?? "").Trim();
                    // Đơn cũ chưa có StatusUpdatedAt thì lấy ngày đặt (0h) làm mốc.
                    DateTime since = o.StatusUpdatedAt ?? o.OrderDate;
                    double minutes = (now - since).TotalMinutes;

                    if (status == OrderConst.WaitingPayment)
                    {
                        if (minutes >= cancelMin && o.PaymentStatus != OrderConst.PayPaid)
                        {
                            o.OrderStatus = OrderConst.Cancelled;
                            o.PaymentStatus = OrderConst.PayCancelled;
                            o.StatusUpdatedAt = now;

                            // Đơn tự hủy thì hoàn lại tồn kho (giống khi admin hủy).
                            foreach (OrderDetail d in o.OrderDetails)
                            {
                                Product product = d.Product ?? db.Products.Find(d.ProductID);
                                if (product != null) product.Stock += d.Quantity;
                            }
                            changed = true;
                        }
                    }
                    else if (status == OrderConst.Processing)
                    {
                        if (minutes >= shipMin)
                        {
                            o.OrderStatus = OrderConst.Shipping;
                            o.StatusUpdatedAt = now;
                            changed = true;
                        }
                    }
                    else if (status == OrderConst.Shipping)
                    {
                        if (minutes >= deliverMin)
                        {
                            o.OrderStatus = OrderConst.Delivered;
                            if (o.PaymentStatus != OrderConst.PayCancelled) o.PaymentStatus = OrderConst.PayPaid;
                            o.StatusUpdatedAt = now;
                            changed = true;
                        }
                    }
                }

                if (changed) db.SaveChanges();
            }
            catch (Exception)
            {
                // Không để lỗi cập nhật tự động làm hỏng trang đang xem.
            }
            finally
            {
                Monitor.Exit(Sync);
            }
        }
    }
}
