using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;
namespace Web_PC_Builder.Models
{
    // ------------------------------------------------------------ Chuẩn hóa SĐT (dùng chung)
    public static class PhoneNormalizer
    {
        /// <summary>Bỏ khoảng trắng, dấu chấm, gạch; +84 / 84 (11 số) ở đầu đổi thành 0.</summary>
        public static string Normalize(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return phone;

            string p = new string(phone.Where(c => !char.IsWhiteSpace(c) && c != '.' && c != '-').ToArray());

            if (p.StartsWith("+84")) p = "0" + p.Substring(3);
            else if (p.StartsWith("84") && p.Length == 11) p = "0" + p.Substring(2);

            return p;
        }
    }

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
        [StringLength(2000, ErrorMessage = "Mô tả sản phẩm tối đa 2000 ký tự.")]
        [DataType(DataType.MultilineText)]
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

        [Display(Name = "Tồn kho")]
        [Range(0, 1000000, ErrorMessage = "Tồn kho phải là số nguyên từ 0 trở lên.")]
        public int Stock { get; set; }
    }

    // ------------------------------------------------------------ Customer
    [MetadataType(typeof(CustomerMetadata))]
    public partial class Customer
    {
        /// <summary>Đổi CustomerPhone về dạng 0xxxxxxxxx (gọi tự động trong SaveChanges).</summary>
        public void NormalizePhone()
        {
            CustomerPhone = PhoneNormalizer.Normalize(CustomerPhone);
        }
    }

    public class CustomerMetadata
    {
        [Display(Name = "Mã KH")]
        public int CustomerID { get; set; }

        [Display(Name = "Họ và tên")]
        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        [StringLength(200, ErrorMessage = "Họ và tên tối đa 200 ký tự.")]
        public string CustomerName { get; set; }

        // SĐT di động VN: 10 số, đầu số 03/05/07/08/09. Cho phép nhập +84 / 84 ở đầu;
        // khi lưu, SaveChanges tự đổi về dạng 0xxxxxxxxx.
        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [StringLength(15, ErrorMessage = "Số điện thoại quá dài.")]
        [RegularExpression(@"^(\+84|84|0)(3|5|7|8|9)\d{8}$",
            ErrorMessage = "Số điện thoại không hợp lệ (10 số, đầu số 03/05/07/08/09, VD: 0901234567).")]
        public string CustomerPhone { get; set; }

        [Display(Name = "Email")]
        [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string CustomerEmail { get; set; }

        [Display(Name = "Địa chỉ")]
        [StringLength(200, ErrorMessage = "Địa chỉ tối đa 200 ký tự.")]
        [DataType(DataType.MultilineText)]
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
        [StringLength(200, ErrorMessage = "Địa chỉ giao hàng tối đa 200 ký tự.")]
        [DataType(DataType.MultilineText)]
        public string ShippingAddress { get; set; }

        [Display(Name = "Hình thức giao hàng")]
        public string DeliveryMethod { get; set; }

        [Display(Name = "Hình thức thanh toán")]
        public string PaymentMethod { get; set; }

        [Display(Name = "Trạng thái đơn")]
        public string OrderStatus { get; set; }

        [Display(Name = "Ghi chú")]
        [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự.")]
        [DataType(DataType.MultilineText)]
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

    // ------------------------------------------------------------ DbContext
    // Tự chuẩn hóa SĐT khách hàng ngay trước khi lưu, nên mọi nơi (Create, Edit, đăng ký, đặt hàng...)
    // đều không cần tự xử lý. Tên class phải trùng với DbContext (My_StoreEntities).
    public partial class My_StoreEntities
    {
        public override int SaveChanges()
        {
            foreach (var entry in ChangeTracker.Entries<Customer>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
            {
                entry.Entity.NormalizePhone();
            }
            return base.SaveChanges();
        }
    }
}
