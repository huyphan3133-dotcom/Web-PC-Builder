using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Web_PC_Builder.Helpers
{
    /// <summary>
    /// Chỉ cho phép vào trang quản trị khi đã đăng nhập (Session["AdminUser"]).
    /// Nếu chưa đăng nhập sẽ chuyển về Admin/Account/Login.
    /// </summary>
    public class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        public const string SessionKey = "AdminUser";

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            return httpContext.Session != null && httpContext.Session[SessionKey] != null;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary(new
                {
                    area = "Admin",
                    controller = "Account",
                    action = "Login",
                    returnUrl = filterContext.HttpContext.Request.RawUrl
                }));
        }
    }
}
