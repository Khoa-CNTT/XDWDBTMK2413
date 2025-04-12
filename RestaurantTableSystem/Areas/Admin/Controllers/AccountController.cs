using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using RestaurantTableSystem.Models;

namespace RestaurantTableSystem.Areas.Admin.Controllers
{
    public class AccountController : Controller
    {
        private RestaurantTableSystemEntities db = new RestaurantTableSystemEntities();

        //public ActionResult UserList()
        //{
        //    var users = db.Users.ToList(); // Lấy toàn bộ user
        //    return View(users);
        //}
        // GET: Admin/Account
        public ActionResult Index()
        {
            var users = db.Users.ToList(); // Lấy toàn bộ user
            return View(users);
        }
    }
}