using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using RestaurantTableSystem.Models;

namespace RestaurantTableSystem.Areas.Admin.Controllers
{
    public class RestaurantCategoryController : Controller
    {
        private RestaurantTableSystemEntities db = new RestaurantTableSystemEntities();
        // GET: Admin/RestaurantCategory
        public ActionResult Index()
        {
            var restaurant = db.Restaurants.ToList(); // Lấy toàn bộ restaurant
            return View(restaurant);
        }

        public ActionResult Detail()
        {
            var menuItem = db.MenuItems.ToList();
            return View(menuItem);
        }
        
    }
}