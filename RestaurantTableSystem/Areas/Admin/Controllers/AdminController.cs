using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RestaurantTableSystem.Areas.Admin.Controllers
{
    public class AdminController : System.Web.Mvc.Controller
    {
        // GET: Admin/Home
        public ActionResult Index()
        {
            // Đảm bảo chỉ Admin mới truy cập được trang này
            if (Session["role"]?.ToString() != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }
    }
}