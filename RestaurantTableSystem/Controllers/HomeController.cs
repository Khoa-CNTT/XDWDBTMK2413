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
            var restaurant = db.Restaurants.ToList(); // Lấy toàn bộ restaurant
            return View(restaurant);
        }
        private RestaurantTableSystemEntities db = new RestaurantTableSystemEntities();
        // GET: Admin/RestaurantCategory
        public ActionResult detailnhahang()
        {
            var restaurant = db.Restaurants.ToList(); // Lấy toàn bộ restaurant
            return View(restaurant);
        }
        
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