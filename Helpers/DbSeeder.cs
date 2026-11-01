using System;
using System.Linq;
using Web_PC_Builder.Models;

namespace Web_PC_Builder.Helpers
{
    /// <summary>
    /// Nếu trong bảng [User] chưa có tài khoản quản trị (UserRole = 'A') thì tự tạo
    /// tài khoản mặc định để đăng nhập lần đầu:  admin / Admin@123  (hãy đổi mật khẩu ngay).
    /// </summary>
    public static class DbSeeder
    {
        public const string DefaultAdminUser = "admin";
        public const string DefaultAdminPassword = "Admin@123";

        public static void EnsureAdminAccount()
        {
            try
            {
                using (var db = new My_StoreEntities())
                {
                    if (db.Users.Any(u => u.UserRole == "A")) return;

                    db.Users.Add(new Models.User
                    {
                        Username = DefaultAdminUser,
                        Password = PasswordHelper.Hash(DefaultAdminUser, DefaultAdminPassword),
                        UserRole = "A"
                    });
                    db.SaveChanges();
                }
            }
            catch (Exception)
            {
                // DB chưa sẵn sàng hoặc username 'admin' đã tồn tại với vai trò khác -> bỏ qua,
                // không để lỗi này làm sập ứng dụng khi khởi động.
            }
        }
    }
}
