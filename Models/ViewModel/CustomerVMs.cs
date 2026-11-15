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
        [StringLength(100, ErrorMessage = "Từ khóa tìm kiếm tối đa 100 ký tự.")]
        public string SearchTerm { get; set; }

        public string SortOrder { get; set; }           // name_asc | name_desc | orders_desc | spent_desc
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public IPagedList<CustomerRowVM> Customers { get; set; }
    }

    /// <summary>Form tạo khách hàng mới: tạo đồng thời 1 dòng [User] (vai trò C) và 1 dòng [Customer].</summary>
    public class CustomerCreateVM
    {
        private string _customerPhone;

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

        /// <summary>
        /// SĐT di động VN: 10 số, đầu số 03/05/07/08/09.
        /// Khi gán giá trị, tự bỏ khoảng trắng/dấu chấm/gạch và đổi +84 / 84 ở đầu thành 0,
        /// nên giá trị lưu vào DB luôn có dạng 0xxxxxxxxx.
        /// </summary>
        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [StringLength(20, ErrorMessage = "Số điện thoại quá dài.")]
        [RegularExpression(@"^0(3|5|7|8|9)\d{8}$",
            ErrorMessage = "Số điện thoại không hợp lệ (10 số, đầu số 03/05/07/08/09, VD: 0901234567).")]
        public string CustomerPhone
        {
            get { return _customerPhone; }
            set { _customerPhone = NormalizePhone(value); }
        }

        [Display(Name = "Email")]
        [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string CustomerEmail { get; set; }

        [Display(Name = "Địa chỉ")]
        [StringLength(200, ErrorMessage = "Địa chỉ tối đa 200 ký tự.")]
        public string CustomerAddress { get; set; }

        /// <summary>Bỏ ký tự phân cách; +84xxxxxxxxx hoặc 84xxxxxxxxx (11 số) đổi thành 0xxxxxxxxx.</summary>
        private static string NormalizePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return phone;

            string p = new string(phone.Where(c => !char.IsWhiteSpace(c) && c != '.' && c != '-' && c != '(' && c != ')').ToArray());

            if (p.StartsWith("+84")) p = "0" + p.Substring(3);
            else if (p.StartsWith("84") && p.Length == 11) p = "0" + p.Substring(2);

            return p;
        }
    }
}