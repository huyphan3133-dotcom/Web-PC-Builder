using System.Web.Mvc;
using Web_PC_Builder.Helpers;
using Web_PC_Builder.Models;

namespace Web_PC_Builder.Areas.Admin.Controllers
{
    /// <summary>
    /// Lớp cha của mọi controller trong khu quản trị (trừ Account):
    /// bắt buộc đã đăng nhập admin và dùng chung một DbContext cho mỗi request.
    /// </summary>
    [AdminAuthorize]
    public abstract class AdminBaseController : Controller
    {
        protected const int DefaultPageSize = 10;

        protected readonly My_StoreEntities db = new My_StoreEntities();

        /// <summary>Tên đăng nhập của admin hiện tại (lấy từ Session).</summary>
        protected string CurrentAdmin
        {
            get { return Session[AdminAuthorizeAttribute.SessionKey] as string; }
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            OrderAutoUpdater.Run(db);
            base.OnActionExecuting(filterContext);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
