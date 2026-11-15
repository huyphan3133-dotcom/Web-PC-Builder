using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Globalization;
namespace Web_PC_Builder.Controllers
{
    public class ProductController : Controller
    {
        public ActionResult Home() { return View(); }
        public ActionResult Details(int id) { return View(); }
    }
}