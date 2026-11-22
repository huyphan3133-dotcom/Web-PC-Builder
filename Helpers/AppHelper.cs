using System;
using System.Globalization;
using System.Web.Mvc;

namespace Web_PC_Builder.Helpers
{
    public static class AppHelper
    {
        private static readonly CultureInfo Vi = new CultureInfo("vi-VN");

        public const string NoImage = "~/Content/Images/no-image.svg";
        public const string ImageFolder = "~/Content/Images/";

        /// <summary>Định dạng tiền: 18.390.000đ</summary>
        public static string Vnd(decimal? value)
        {
            return (value ?? 0m).ToString("N0", Vi) + "đ";
        }

        /// <summary>Định dạng ngày: 31/10/2026</summary>
        public static string Date(DateTime value)
        {
            return value.ToString("dd/MM/yyyy", Vi);
        }

        /// <summary>
        /// Trả về URL ảnh sản phẩm. Hỗ trợ: rỗng (ảnh mặc định), "~/Content/Images/a.png",
        /// "/Content/Images/a.png", "http://...", "https://...", hoặc chỉ tên file "a.png".
        /// </summary>
        public static string ProductImage(this UrlHelper url, string image)
        {
            if (string.IsNullOrWhiteSpace(image)) return url.Content(NoImage);

            string img = image.Trim();
            if (img.StartsWith("~/")) return url.Content(img);

            // URL tuyệt đối (http://, https://, //cdn...) giữ nguyên. Không dùng StartsWith("http")
            // vì tên file như "http-logo.png" sẽ bị hiểu nhầm là URL.
            if (img.StartsWith("//") ||
                img.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                img.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) return img;

            // "/Content/Images/a.png": đi qua url.Content để đúng cả khi web chạy trong thư mục ảo (IIS Application).
            if (img.StartsWith("/")) return url.Content("~" + img);

            return url.Content(ImageFolder + img);
        }
    }
}