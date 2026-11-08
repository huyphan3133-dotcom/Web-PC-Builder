namespace Web_PC_Builder.Helpers
{
    /// <summary>Các giá trị trạng thái đơn hàng / thanh toán dùng thống nhất trong trang quản trị.</summary>
    public static class OrderConst
    {
        public const string WaitingPayment = "Chờ thanh toán";
        public const string Processing = "Đang xử lý";
        public const string Shipping = "Đang vận chuyển";
        public const string Delivered = "Đã giao";
        public const string Cancelled = "Đã hủy";

        /// <summary>Đơn đã giao hoặc đã hủy là trạng thái cuối: khóa, không cho chỉnh sửa nữa.</summary>
        public static bool IsLocked(string status)
        {
            string s = (status ?? "").Trim();
            return s == Delivered || s == Cancelled;
        }

        public static readonly string[] Statuses =
        {
            WaitingPayment, Processing, Shipping, Delivered, Cancelled
        };

        public const string PayPending = "Pending";
        public const string PayPaid = "Paid";
        public const string PayCancelled = "Cancelled";

        public static readonly string[] PaymentStatuses = { PayPending, PayPaid, PayCancelled };

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
}
