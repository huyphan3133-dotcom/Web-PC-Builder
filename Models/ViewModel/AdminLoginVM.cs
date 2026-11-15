using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Web_PC_Builder.Models.ViewModel
{
    public class AdminLoginVM
    {
        [Display(Name = "Tên đăng nhập")]
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
        public string Username { get; set; }

        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
