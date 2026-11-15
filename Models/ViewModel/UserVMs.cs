using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using PagedList;

namespace Web_PC_Builder.Models.ViewModel
{
    public class UserSearchVM
    {
        public string SearchTerm { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public IPagedList<User> Users { get; set; }
    }

    public class UserCreateVM
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
    }

    /// <summary>Đổi mật khẩu của một tài khoản quản trị (tên đăng nhập là khóa chính nên không cho sửa).</summary>
    public class UserEditVM
    {
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; }

        [Display(Name = "Mật khẩu mới")]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu từ 6 đến 100 ký tự.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Display(Name = "Nhập lại mật khẩu mới")]
        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu mới.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Hai mật khẩu không khớp.")]
        public string ConfirmPassword { get; set; }
    }
}
