using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using PagedList;

namespace Web_PC_Builder.Models.ViewModel
{
    /// <summary>Một dòng trong danh sách khách hàng (kèm số đơn và tổng chi tiêu).</summary>
    public class CustomerRowVM
    {
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerEmail { get; set; }
        public string Username { get; set; }
        public int OrderCount { get; set; }
        public decimal? TotalSpent { get; set; }
    }

    public class CustomerSearchVM
    {
        public string SearchTerm { get; set; }
        public string SortOrder { get; set; }           // name_asc | name_desc | orders_desc | spent_desc
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public IPagedList<CustomerRowVM> Customers { get; set; }
    }

    /// <summary>Form tạo khách hàng mới: tạo đồng thời 1 dòng [User] (vai trò C) và 1 dòng [Customer].</summary>
    public class CustomerCreateVM
    {
        [Display(Name = "Tên đăng nhập")]
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
        [StringLength(255, ErrorMessage = "Tên đăng nhập tối đa 255 ký tự.")]
        public string Username { get; set; }

        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu từ 6 đến 100 ký tự.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Nhập lại mật khẩu")]
        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Hai mật khẩu không khớp.")]
        public string ConfirmPassword { get; set; }

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
    }
}
