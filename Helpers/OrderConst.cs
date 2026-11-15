using System;
using System.Linq;

namespace Web_PC_Builder.Helpers
{
    /// <summary>
    /// Nguồn duy nhất cho các giá trị trạng thái đơn hàng / thanh toán.
    /// Dùng chung cho admin (UpdateStatus, Details, OrderAutoUpdater) và phần khách xem đơn.
    /// Không viết cứng chuỗi "Đã giao", "Đã hủy"... ở nơi khác, luôn tham chiếu về đây.
    /// </summary>
    public static class OrderConst
    {
        // ---- Trạng thái đơn ----
        public const string WaitingPayment = "Chờ thanh toán";
        public const string Processing = "Đang xử lý";
        public const string Shipping = "Đang vận chuyển";
        public const string Delivered = "Đã giao";
        public const string Cancelled = "Đã hủy";

        /// <summary>Toàn bộ trạng thái hợp lệ (dùng cho dropdown admin và kiểm tra dữ liệu gửi lên).</summary>
        public static readonly string[] Statuses =
        {
            WaitingPayment, Processing, Shipping, Delivered, Cancelled
        };

        /// <summary>Các bước của luồng giao hàng bình thường (không gồm "Đã hủy"), dùng cho thanh tiến trình phía khách.</summary>
        public static readonly string[] Steps =
        {
            WaitingPayment, Processing, Shipping, Delivered
        };

        // ---- Trạng thái thanh toán ----
        public const string PayPending = "Pending";
        public const string PayPaid = "Paid";
        public const string PayCancelled = "Cancelled";

        public static readonly string[] PaymentStatuses = { PayPending, PayPaid, PayCancelled };

        // ---- Chuẩn hóa & kiểm tra giá trị ----

        /// <summary>Cắt khoảng trắng, null thành chuỗi rỗng.</summary>
        public static string Normalize(string value)
        {
            return (value ?? "").Trim();
        }

        public static bool IsValidStatus(string status)
        {
            return Statuses.Contains(Normalize(status));
        }

        public static bool IsValidPaymentStatus(string status)
        {
            return PaymentStatuses.Contains(Normalize(status));
        }

        public static bool IsDelivered(string status)
        {
            return Normalize(status) == Delivered;
        }

        public static bool IsCancelled(string status)
        {
            return Normalize(status) == Cancelled;
        }

        public static bool IsPaid(string paymentStatus)
        {
            return Normalize(paymentStatus) == PayPaid;
        }

        /// <summary>Đơn đã giao hoặc đã hủy là trạng thái cuối: khóa, không cho chỉnh sửa nữa.</summary>
        public static bool IsLocked(string status)
        {
            string s = Normalize(status);
            return s == Delivered || s == Cancelled;
        }

        /// <summary>
        /// Vị trí của trạng thái trong luồng giao hàng (0..3), -1 nếu là "Đã hủy" hoặc không xác định.
        /// Phía khách dùng để tô các bước đã hoàn thành.
        /// </summary>
        public static int StepIndex(string status)
        {
            return Array.IndexOf(Steps, Normalize(status));
        }

        // ---- Hiển thị ----

        public static string PaymentText(string status)
        {
            switch (status)
            {
                case PayPending: return "Chưa thanh toán";
                case PayPaid: return "Đã thanh toán";
                case PayCancelled: return "Đã hủy";
                default: return string.IsNullOrEmpty(status) ? "-" : status;
            }
        }

        public static string StatusBadge(string status)
        {
            switch (status)
            {
                case WaitingPayment: return "bg-warning text-dark";
                case Processing: return "bg-info text-dark";
                case Shipping: return "bg-primary";
                case Delivered: return "bg-success";
                case Cancelled: return "bg-secondary";
                default: return "bg-light text-dark";
            }
        }

        public static string PaymentBadge(string status)
        {
            switch (status)
            {
                case PayPaid: return "bg-success";
                case PayPending: return "bg-warning text-dark";
                default: return "bg-secondary";
            }
        }
    }

    /// <summary>
    /// Quy tắc nghiệp vụ dùng chung cho đơn hàng: gọi ở server (UpdateStatus của admin)
    /// và để đối chiếu với JS trên form. Một chỗ duy nhất định nghĩa tổ hợp trạng thái hợp lệ.
    /// </summary>
    public static class OrderRules
    {
        // Thông báo lỗi dùng chung cho server và JS trên form.
        public static readonly string ErrCancelledPaid =
            "Không thể đánh dấu \"" + OrderConst.PaymentText(OrderConst.PayPaid)
            + "\" cho đơn \"" + OrderConst.Cancelled + "\".";

        public static readonly string ErrCodNotDelivered =
            "Đơn COD chỉ được đánh dấu \"" + OrderConst.PaymentText(OrderConst.PayPaid)
            + "\" sau khi đơn ở trạng thái \"" + OrderConst.Delivered + "\".";

        public const string ErrInvalidStatus = "Trạng thái đơn hàng không hợp lệ.";
        public const string ErrInvalidPayment = "Trạng thái thanh toán không hợp lệ.";

        /// <summary>Đơn COD (thanh toán khi nhận hàng)? Nhận cả "COD" lẫn chữ tiếng Việt tương đương.</summary>
        public static bool IsCod(string paymentMethod)
        {
            if (string.IsNullOrWhiteSpace(paymentMethod)) return false;
            string m = paymentMethod.Trim();
            return m.IndexOf("COD", StringComparison.OrdinalIgnoreCase) >= 0
                || m.IndexOf("khi nhận hàng", StringComparison.OrdinalIgnoreCase) >= 0
                || m.IndexOf("khi nhan hang", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>Trạng thái thanh toán này có phải "Đã thanh toán" không (dùng cho view).</summary>
        public static bool IsPaid(string paymentStatus)
        {
            return OrderConst.IsPaid(paymentStatus);
        }

        /// <summary>
        /// Kiểm tra tổ hợp (trạng thái đơn, trạng thái thanh toán, phương thức thanh toán).
        /// Trả về null nếu hợp lệ, ngược lại trả về thông báo lỗi tiếng Việt.
        /// </summary>
        public static string Validate(string orderStatus, string paymentStatus, string paymentMethod)
        {
            string os = OrderConst.Normalize(orderStatus);
            string ps = OrderConst.Normalize(paymentStatus);

            // 0) Giá trị gửi lên phải nằm trong danh sách của OrderConst (chống sửa tay form).
            if (!OrderConst.IsValidStatus(os)) return ErrInvalidStatus;
            if (!OrderConst.IsValidPaymentStatus(ps)) return ErrInvalidPayment;

            // 1) Đơn đã hủy thì không thể ở trạng thái "Đã thanh toán".
            if (os == OrderConst.Cancelled && ps == OrderConst.PayPaid)
                return ErrCancelledPaid;

            // 2) Đơn COD chỉ được đánh dấu "Đã thanh toán" sau khi đã giao hàng.
            if (IsCod(paymentMethod) && ps == OrderConst.PayPaid && os != OrderConst.Delivered)
                return ErrCodNotDelivered;

            return null;
        }
    }
}
