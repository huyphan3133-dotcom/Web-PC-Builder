using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;

// Các file Category.cs, Product.cs... do EF (T4) sinh ra và sẽ bị ghi đè khi "Update Model from Database".
// Vì vậy phần nhãn tiếng Việt + validation + trường UploadImg được đặt ở file riêng này (partial class + MetadataType).
namespace Web_PC_Builder.Models
{
    // ------------------------------------------------------------ Category
    [MetadataType(typeof(CategoryMetadata))]
    public partial class Category
    {
    }

    public class CategoryMetadata
    {
        [Display(Name = "Mã danh mục")]
        public int CategoryID { get; set; }

        [Display(Name = "Tên danh mục")]
        [Required(ErrorMessage = "Vui lòng nhập tên danh mục.")]
        [StringLength(200, ErrorMessage = "Tên danh mục tối đa 200 ký tự.")]
        public string CategoryName { get; set; }
    }

    // ------------------------------------------------------------ Product
    [MetadataType(typeof(ProductMetadata))]
    public partial class Product
    {
        /// <summary>File ảnh upload từ form (không lưu trong DB).</summary>
        [NotMapped]
        [Display(Name = "Tải ảnh lên")]
        public HttpPostedFileBase UploadImg { get; set; }
    }

    public class ProductMetadata
    {
        [Display(Name = "Mã SP")]
        public int ProductID { get; set; }

        [Display(Name = "Danh mục")]
        [Required(ErrorMessage = "Vui lòng chọn danh mục.")]
        public int CategoryID { get; set; }

        [Display(Name = "Tên sản phẩm")]
        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm.")]
        [StringLength(300, ErrorMessage = "Tên sản phẩm tối đa 300 ký tự.")]
        public string ProductName { get; set; }

        [Display(Name = "Mô tả / thông số")]
        [Required(ErrorMessage = "Vui lòng nhập mô tả sản phẩm.")]
        public string ProductDescription { get; set; }

        [Display(Name = "Giá bán")]
        [Required(ErrorMessage = "Vui lòng nhập giá bán.")]
        [Range(typeof(decimal), "0", "9999999999999", ErrorMessage = "Giá bán không hợp lệ.")]
        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true)]
        public decimal ProductPrice { get; set; }

        [Display(Name = "Ảnh")]
        public string ProductImage { get; set; }

        [Display(Name = "Giá cũ")]
        [Range(typeof(decimal), "0", "9999999999999", ErrorMessage = "Giá cũ không hợp lệ.")]
        [DisplayFormat(DataFormatString = "{0:0.##}", ApplyFormatInEditMode = true, NullDisplayText = "")]
        public Nullable<decimal> OldPrice { get; set; }

        [Display(Name = "Thương hiệu")]
        [StringLength(100, ErrorMessage = "Thương hiệu tối đa 100 ký tự.")]
        public string Brand { get; set; }
    }

    // ------------------------------------------------------------ Customer
    [MetadataType(typeof(CustomerMetadata))]
    public partial class Customer
    {
    }

    public class CustomerMetadata
    {
        [Display(Name = "Mã KH")]
        public int CustomerID { get; set; }

        [Display(Name = "Họ và tên")]
        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        [StringLength(200, ErrorMessage = "Họ và tên tối đa 200 ký tự.")]
        public string CustomerName { get; set; }

        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [RegularExpression(@"^(0|\+84)\d{9,10}$", ErrorMessage = "Số điện thoại không hợp lệ (VD: 0901234567).")]
        public string CustomerPhone { get; set; }

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string CustomerEmail { get; set; }

        [Display(Name = "Địa chỉ")]
        public string CustomerAddress { get; set; }

        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; }
    }

    // ------------------------------------------------------------ User
    [MetadataType(typeof(UserMetadata))]
    public partial class User
    {
    }

    public class UserMetadata
    {
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; }

        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [Display(Name = "Vai trò")]
        public string UserRole { get; set; }
    }

    // ------------------------------------------------------------ Order
    [MetadataType(typeof(OrderMetadata))]
    public partial class Order
    {
    }

    public class OrderMetadata
    {
        [Display(Name = "Mã đơn")]
        public int OrderID { get; set; }

        [Display(Name = "Khách hàng")]
        public int CustomerID { get; set; }

        [Display(Name = "Ngày đặt")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime OrderDate { get; set; }

        [Display(Name = "Tổng tiền")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Thanh toán")]
        public string PaymentStatus { get; set; }

        [Display(Name = "Địa chỉ giao hàng")]
        public string ShippingAddress { get; set; }

        [Display(Name = "Hình thức giao hàng")]
        public string DeliveryMethod { get; set; }

        [Display(Name = "Hình thức thanh toán")]
        public string PaymentMethod { get; set; }

        [Display(Name = "Trạng thái đơn")]
        public string OrderStatus { get; set; }

        [Display(Name = "Ghi chú")]
        public string Note { get; set; }
    }

    // ------------------------------------------------------------ OrderDetail
    [MetadataType(typeof(OrderDetailMetadata))]
    public partial class OrderDetail
    {
    }

    public class OrderDetailMetadata
    {
        [Display(Name = "Sản phẩm")]
        public int ProductID { get; set; }

        [Display(Name = "Số lượng")]
        public int Quantity { get; set; }

        [Display(Name = "Đơn giá")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Thành tiền")]
        public Nullable<decimal> TotalPrice { get; set; }
    }
}
