using RestaurantTableSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RestaurantTableSystem.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var list = db.Restaurants.ToList(); // hoặc lấy theo điều kiện
            return View(list);
        }

        private RestaurantTableSystemEntities db = new RestaurantTableSystemEntities();
        // GET: Admin/RestaurantCategory

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}