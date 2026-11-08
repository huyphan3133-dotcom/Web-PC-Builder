using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Web_PC_Builder.Helpers
{
    /// <summary>
    /// Chỉ cho phép vào khi khách hàng đã đăng nhập (Session["CustomerUser"]).
    /// Nếu chưa đăng nhập sẽ chuyển về Account/Login và nhớ trang đang xem.
    /// </summary>
    public class CustomerAuthorizeAttribute : AuthorizeAttribute
    {
        public const string SessionKey = "CustomerUser";
        public const string SessionNameKey = "CustomerName";

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            return httpContext.Session != null && httpContext.Session[SessionKey] != null;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary(new
                {
                    area = "",
                    controller = "Account",
                    action = "Login",
                    returnUrl = filterContext.HttpContext.Request.RawUrl
                }));
        }
    }
}
