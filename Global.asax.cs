using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Web_PC_Builder.Helpers;

namespace Web_PC_Builder
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Tạo sẵn tài khoản quản trị (admin / Admin@123) nếu bảng [User] chưa có tài khoản nào có UserRole = 'A'.
            DbSeeder.EnsureOrderColumns();
            DbSeeder.EnsureAdminAccount();

            // Chạy nền: tự chuyển trạng thái đơn hàng theo thời gian.
            OrderAutoUpdater.StartBackground();
        }
    }
}
