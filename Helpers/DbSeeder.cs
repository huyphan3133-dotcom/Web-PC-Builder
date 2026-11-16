using System;
using System.Collections.Generic;
using System.Linq;
using Web_PC_Builder.Models;

namespace Web_PC_Builder.Helpers
{
    /// <summary>
    /// Nếu trong bảng [User] chưa có tài khoản quản trị (UserRole = 'A') thì tự tạo
    /// tài khoản mặc định để đăng nhập lần đầu:  admin / Admin@123  (hãy đổi mật khẩu ngay).
    /// Ngoài ra seed các sản phẩm của web (lấy từ _ProductCore.cshtml) kèm tồn kho.
    /// </summary>
    public static class DbSeeder
    {
        public const string DefaultAdminUser = "admin";
        public const string DefaultAdminPassword = "Admin@123";

        /// <summary>Tồn kho mặc định cho sản phẩm seed (admin sửa sau trong form Sửa sản phẩm).</summary>
        public const int DefaultStock = 20;

        /// <summary>Tự thêm cột [Order].StatusUpdatedAt nếu database chưa có (chạy an toàn nhiều lần).</summary>
        public static void EnsureOrderColumns()
        {
            try
            {
                using (var db = new My_StoreEntities())
                {
                    db.Database.ExecuteSqlCommand(
                        "IF COL_LENGTH('dbo.[Order]', 'StatusUpdatedAt') IS NULL " +
                        "ALTER TABLE dbo.[Order] ADD StatusUpdatedAt datetime NULL");
                }
            }
            catch (Exception)
            {
                // Không đủ quyền ALTER -> hãy chạy tay câu lệnh SQL trong tài liệu hướng dẫn.
            }
        }

        /// <summary>
        /// Tự thêm cột [Product].Stock (tồn kho) nếu database chưa có (chạy an toàn nhiều lần).
        /// LƯU Ý: sau khi có cột phải "Update Model from Database" trong MyStoreModel.edmx
        /// để class Product có thuộc tính Stock.
        /// </summary>
        public static void EnsureProductColumns()
        {
            try
            {
                using (var db = new My_StoreEntities())
                {
                    db.Database.ExecuteSqlCommand(
                        "IF COL_LENGTH('dbo.[Product]', 'Stock') IS NULL " +
                        "ALTER TABLE dbo.[Product] ADD Stock int NOT NULL CONSTRAINT DF_Product_Stock DEFAULT 0");
                }
            }
            catch (Exception)
            {
                // Không đủ quyền ALTER -> chạy tay câu lệnh trên trong SSMS.
            }
        }

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

        // ------------------------------------------------------------------
        //  SEED SẢN PHẨM CỦA WEB (nguồn: mảng products trong _ProductCore.cshtml)
        //  Tên danh mục TRÙNG chữ với danh mục ở trang chủ.
        //  GIẢ ĐỊNH (đổi cho khớp model EF):
        //    Category { CategoryID, CategoryName }
        //    Product  { ProductName, Price (decimal), Stock (int), CategoryID }
        //  Nếu Product có các cột khác (Brand, OldPrice, Image, Description...) thì
        //  bỏ chú thích ở chỗ "THÊM CỘT" bên dưới.
        // ------------------------------------------------------------------

        private class SeedItem
        {
            public string Category, Brand, Name, Image, Specs;
            public decimal Price, OldPrice;
        }

        private static List<SeedItem> BuildSeedList()
        {
            var l = new List<SeedItem>();
            Action<string, string, string, decimal, decimal, string, string> A =
                (cat, brand, name, price, old, img, specs) => l.Add(new SeedItem
                {
                    Category = cat,
                    Brand = brand,
                    Name = name,
                    Price = price,
                    OldPrice = old,
                    Image = img,
                    Specs = specs
                });

            // PC GVN
            A("PC GVN", "GVN", "PC GVN Intel i5-12400F/ VGA RTX 3050 (Main H)", 18390000, 20280000, "pc1.png",
              "CPU: Intel Core i5-12400F, Mainboard: H610M, RAM: 16 GB, Ổ cứng: 512 GB, VGA: RTX 3050 6GB");
            A("PC GVN", "GVN", "PC GVN Intel i5-12400F/ VGA RX 6500XT (H610)", 16790000, 18680000, "pc5.png",
              "CPU: Intel Core i5-12400F, Mainboard: H610M, RAM: 16 GB, Ổ cứng: 512 GB, VGA: RX 6500XT 4GB");
            A("PC GVN", "GVN", "PC GVN x MSI LIGHTNING (Intel Core Ultra 9 285K/ VGA RTX 5090)", 315990000, 317330000, "pc3.png",
              "CPU: Core Ultra 9 285K, Mainboard: MSI MEG Z890, RAM: 64 GB, Ổ cứng: 1024 GB, VGA: RTX 5090 32GB");
            A("PC GVN", "GVN", "PC GVN Intel i7-14700F/ VGA RTX 5070Ti (DDR5)", 73080000, 74020000, "pc4.png",
              "CPU: Intel Core i7-14700F, Mainboard: Z790, RAM: 32 GB, Ổ cứng: 1024 GB, VGA: RTX 5070 Ti");

            // Laptop Gaming
            A("Laptop Gaming", "ACER", "Laptop gaming Acer Aspire 7 A715-59G-59RD (Core 5-210H/ RTX 3050 4GB/ 16GB/ 512GB)", 25990000, 27990000, "gm1.png",
              "CPU: Core 5-210H, RAM: 16 GB, Ổ cứng: 512 GB, VGA: RTX 3050 4GB, Màn hình: 15.6\" FHD");
            A("Laptop Gaming", "ACER", "Laptop gaming Acer Aspire 7 A715-59G-53RH (Core 5-210H/ RTX 4050 6GB/ 16GB/ 512GB)", 29790000, 29990000, "gm2.png",
              "CPU: Core 5-210H, RAM: 16 GB, Ổ cứng: 512 GB, VGA: RTX 4050 6GB, Màn hình: 15.6\" FHD IPS 144Hz");
            A("Laptop Gaming", "ACER", "Laptop gaming Acer Nitro ProPanel ANV15-52-556L (Core 5-210H/ RTX 5050 8GB/ 16GB/ 512GB)", 38990000, 38990000, "gm3.png",
              "CPU: Core 5-210H, RAM: 16 GB, Ổ cứng: 512 GB, VGA: RTX 5050 8GB, Màn hình: 15.6\" FHD");
            A("Laptop Gaming", "ASUS", "Laptop gaming ASUS V16 V3607VJ-TK189W (Core 5-210H/ RTX 3050 6GB/ 16GB/ 512GB)", 26990000, 29990000, "gm4.png",
              "CPU: Core 5-210H, RAM: 16 GB, Ổ cứng: 512 GB, VGA: RTX 3050 6GB, Màn hình: 16\" WUXGA 144Hz");

            // Laptop (văn phòng)
            A("Laptop", "ACER", "Laptop Acer Aspire Lite 15 AL15-53P-56QH (Core 5-120U/ 8GB/ 512GB/ 15.6\" FHD)", 18890000, 24990000, "vp1.png",
              "CPU: Core 5-120U, RAM: 8 GB, Ổ cứng: 512 GB, Đồ họa: Intel UHD Graphics, Màn hình: 15.6\" FHD IPS");
            A("Laptop", "ASUS", "Laptop Asus ExpertBook P1403CVA-C5H16-50W (Core 5-210H/ 16GB/ 512GB/ 14\" FHD)", 23390000, 23390000, "vp2.png",
              "CPU: Core 5-210H, RAM: 16 GB, Ổ cứng: 512 GB, Đồ họa: Intel UHD Graphics, Màn hình: 14\" FHD IPS");
            A("Laptop", "ACER", "Laptop Acer Swift Go 14 AI SFG14-75-5264 (Ultra 5-226V/ 16GB/ 512GB/ 14\" FHD+ OLED)", 28690000, 28890000, "vp3.png",
              "CPU: Core Ultra 5-226V, RAM: 16 GB, Ổ cứng: 512 GB, Đồ họa: Intel Arc, Màn hình: 14\" FHD+ OLED");
            A("Laptop", "ASUS", "Laptop Asus Vivobook 14 X1404V (Core 5-120U/ 8GB/ 256GB/ 14\" FHD)", 16590000, 18990000, "vp4.jpg",
              "CPU: Core 5-120U, RAM: 8 GB, Ổ cứng: 256 GB, Đồ họa: Intel UHD Graphics, Màn hình: 14\" FHD IPS");

            // Màn hình
            A("Màn hình", "VSP", "Màn hình VSP G2410QS 24\" IPS 2K 100Hz USBC", 3190000, 3590000, "mh1.png",
              "Kích thước: 24 inch, Tấm nền: IPS, Độ phân giải: 2K/QHD (2560 × 1440), Tần số quét: 100 Hz, Phản hồi: 5 ms");
            A("Màn hình", "VSP", "Màn hình VSP G2410Q1 24\" IPS 2K 100Hz", 2590000, 2790000, "mh2.png",
              "Kích thước: 24 inch, Tấm nền: IPS, Độ phân giải: 2K/QHD (2560 × 1440), Tần số quét: 100 Hz, Phản hồi: 5 ms");
            A("Màn hình", "VSP", "Màn hình VSP IP25Q180 25\" IPS 2K 180Hz chuyên game", 3290000, 3990000, "mh3.png",
              "Kích thước: 25 inch, Tấm nền: IPS, Độ phân giải: 2K/QHD (2560 × 1440), Tần số quét: 180 Hz, Phản hồi: 1 ms");
            A("Màn hình", "ViewSonic", "Màn hình ViewSonic VA24G1-H 24\" IPS 144Hz", 2190000, 2590000, "mh4.png",
              "Kích thước: 24 inch, Tấm nền: IPS, Tần số quét: 144 Hz, Phản hồi: 1 ms, Độ sáng: 400 cd/m²");

            // Bàn phím
            A("Bàn phím", "ASUS", "Bàn phím cơ ASUS ROG Falchion Ace 75 HE V2X White", 5890000, 5990000, "ph1.png",
              "Layout: 75%, Keycap: PBT, Đèn: RGB");
            A("Bàn phím", "ASUS", "Bàn phím cơ ASUS ROG Falchion Ace 75 HE V2X Black", 5890000, 5990000, "ph3.png",
              "Layout: 75%, Keycap: PBT Double-shot, Đèn: RGB");
            A("Bàn phím", "ASUS", "Bàn phím cơ ASUS ROG Falchion Ace 75 HE V2 White", 5890000, 5990000, "ph4.png",
              "Layout: 75%, Keycap: PBT, Kết nối: Có dây USB-C, Đèn: RGB");
            A("Bàn phím", "ASUS", "Bàn phím cơ ASUS ROG Falchion Ace 75 HE V2 Red", 5890000, 5990000, "ph5.png",
              "Layout: 75%, Keycap: PBT Double-shot, Đèn: RGB");

            // Chuột + Lót chuột
            A("Chuột + Lót chuột", "AKKO", "Chuột AKKO AG ONE 8K Joy of Life", 790000, 1590000, "ch1.png",
              "Cảm biến: PAW3395, Polling rate: 8000 Hz, Trọng lượng: 65 g, DPI: 26000");
            A("Chuột + Lót chuột", "Logitech", "Chuột gaming không dây Logitech G304 X Superlight Black", 1590000, 1790000, "ch2.png",
              "Cảm biến: HERO 44K, Polling rate: 1000 Hz, Trọng lượng: 59 g, DPI: 44000");
            A("Chuột + Lót chuột", "Razer", "Chuột Razer Basilisk V3 Pro 35K Phantom Green Edition", 3940000, 4290000, "ch3.png",
              "Kết nối: Đa kết nối, Cảm biến: Focus Pro 35K, Polling rate: 8000 Hz, DPI: 35000");
            A("Chuột + Lót chuột", "HyperWork", "Chuột không dây HyperWork Silentium MS01 Đen", 490000, 590000, "ch4.png",
              "Màu: Đen, Pin: Sạc, DPI: 2400");

            return l;
        }

        /// <summary>
        /// Seed sản phẩm của web: tự tạo danh mục còn thiếu, chỉ thêm sản phẩm chưa có (theo tên),
        /// nên chạy nhiều lần không bị trùng.
        /// </summary>
        public static void EnsureProducts()
        {
            try
            {
                using (var db = new My_StoreEntities())
                {
                    var seed = BuildSeedList();

                    foreach (var cname in seed.Select(s => s.Category).Distinct())
                    {
                        if (!db.Categories.Any(c => c.CategoryName == cname))
                            db.Categories.Add(new Category { CategoryName = cname });
                    }
                    db.SaveChanges();

                    var catIds = db.Categories.ToDictionary(c => c.CategoryName, c => c.CategoryID);
                    var existing = new HashSet<string>(db.Products.Select(p => p.ProductName));
                    var added = new List<Product>();

                    foreach (var s in seed)
                    {
                        if (existing.Contains(s.Name)) continue;

                        var newProduct = new Product
                        {
                            ProductName = s.Name,
                            ProductPrice = s.Price,
                            OldPrice = s.OldPrice,
                            Brand = s.Brand,
                            ProductImage = s.Image,           // file nằm ở Content/Images/
                            ProductDescription = s.Specs,
                            CategoryID = catIds[s.Category]
                        };
                        db.Products.Add(newProduct);
                        added.Add(newProduct);
                    }
                    db.SaveChanges();

                    // Gán tồn kho cho sản phẩm vừa thêm bằng SQL, để file build được kể cả khi
                    // class Product chưa có thuộc tính Stock. Chỉ chạy nếu cột Stock đã tồn tại.
                    // Sau khi "Update Model from Database" có thể đổi sang p.Stock = DefaultStock.
                    foreach (var item in added)
                    {
                        db.Database.ExecuteSqlCommand(
                            "IF COL_LENGTH('dbo.[Product]', 'Stock') IS NOT NULL " +
                            "EXEC('UPDATE dbo.[Product] SET Stock = ' + CAST(@p0 AS varchar(10)) + " +
                            "' WHERE ProductID = ' + CAST(@p1 AS varchar(10)))",
                            DefaultStock, item.ProductID);
                    }
                }
            }
            catch (Exception)
            {
                // DB chưa sẵn sàng / model chưa có cột Stock -> bỏ qua,
                // không để lỗi này làm sập ứng dụng khi khởi động.
            }
        }
    }
}
