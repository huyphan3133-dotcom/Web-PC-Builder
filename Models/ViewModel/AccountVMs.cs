using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Web_PC_Builder.Models.ViewModel
{
    public class LoginVM
    {
        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    public class RegisterVM
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2 đến 200 ký tự.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [StringLength(255, ErrorMessage = "Email tối đa 255 ký tự.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại gồm 10 số, bắt đầu bằng 0 (VD: 0901234567).")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu từ 6 đến 100 ký tự.")]
        [RegularExpression(@"^\S+$", ErrorMessage = "Mật khẩu không được chứa khoảng trắng.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Hai mật khẩu không khớp.")]
        public string ConfirmPassword { get; set; }
    }

    /// <summary>Một dòng trong bảng "Lịch sử đơn hàng" ở trang Profile.</summary>
    public class OrderRowVM2
    {
        public int OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public int ItemCount { get; set; }          // tổng số lượng sản phẩm trong đơn
    }

    /// <summary>Một sản phẩm trong trang chi tiết đơn hàng.</summary>
    public class OrderItemVM
    {
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public decimal LineTotal
        {
            get { return UnitPrice * Quantity; }
        }
    }

    /// <summary>Trang chi tiết + theo dõi trạng thái một đơn hàng của khách.</summary>
    public class OrderDetailVM
    {
        public int OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string OrderStatus { get; set; }
        public string PaymentStatus { get; set; }
        public string PaymentMethod { get; set; }
        public string DeliveryMethod { get; set; }
        public string ShippingAddress { get; set; }
        public string Note { get; set; }
        public List<OrderItemVM> Items { get; set; }

        public OrderDetailVM()
        {
            Items = new List<OrderItemVM>();
        }
    }

    /// <summary>Form sửa thông tin cá nhân ở trang Profile (email là tên đăng nhập nên không cho sửa).</summary>
    public class UpdateProfileVM
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2 đến 200 ký tự.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại gồm 10 số, bắt đầu bằng 0 (VD: 0901234567).")]
        public string Phone { get; set; }

        [StringLength(500, ErrorMessage = "Địa chỉ tối đa 500 ký tự.")]
        public string Address { get; set; }
    }

    public class ProfileVM
    {
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public List<OrderRowVM2> Orders { get; set; }

        public ProfileVM()
        {
            Orders = new List<OrderRowVM2>();
        }
    }
}
