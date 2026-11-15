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

            DbSeeder.EnsureOrderColumns();
            DbSeeder.EnsureProductColumns();   // thêm cột Product.Stock nếu DB chưa có
            DbSeeder.EnsureAdminAccount();
            DbSeeder.EnsureProducts();

            // Chạy nền: tự chuyển trạng thái đơn hàng theo thời gian.
            OrderAutoUpdater.StartBackground();
        }
    }
}
