using System;
using System.Security.Cryptography;
using System.Text;

namespace Web_PC_Builder.Helpers
{
    /// <summary>
    /// Băm mật khẩu để không lưu chữ thường trong bảng [User].
    /// Kết quả luôn dài 44 ký tự nên vẫn vừa cột Password (NCHAR(50) hoặc NVARCHAR(255)).
    /// </summary>
    public static class PasswordHelper
    {
        private const int Iterations = 10000;

        public static string Hash(string username, string password)
        {
            string u = (username ?? "").Trim().ToLowerInvariant();
            byte[] salt;
            using (var sha = SHA256.Create())
            {
                salt = sha.ComputeHash(Encoding.UTF8.GetBytes("My_Store|" + u));
            }
            using (var kdf = new Rfc2898DeriveBytes(password ?? "", salt, Iterations))
            {
                return Convert.ToBase64String(kdf.GetBytes(33));
            }
        }

        /// <summary>
        /// Kiểm tra mật khẩu. isLegacy = true nếu DB đang lưu mật khẩu dạng chữ thường (dữ liệu cũ),
        /// khi đó nơi gọi nên lưu lại bản băm.
        /// </summary>
        public static bool Verify(string username, string password, string stored, out bool isLegacy)
        {
            isLegacy = false;
            string s = (stored ?? "").Trim();
            if (s.Length == 0 || string.IsNullOrEmpty(password)) return false;

            if (s == Hash(username, password)) return true;

            if (s == password)
            {
                isLegacy = true;
                return true;
            }
            return false;
        }
    }
}
