using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using RestaurantTableSystem.Models;

namespace RestaurantTableSystem.Areas.Admin.Controllers
{
    public class RestaurantCategoryController : System.Web.Mvc.Controller
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
        [HttpGet]
        public ActionResult Delete(int id)
        {
            var restaurant = db.Restaurants.Find(id);
            if (restaurant != null)
            {
                db.Restaurants.Remove(restaurant);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

    }
}