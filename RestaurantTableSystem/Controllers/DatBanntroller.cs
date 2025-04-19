using RestaurantTableSystem.Models;
using RestaurantTableSystem.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RestaurantTableSystem.Controllers
{
    public class DatBanController : Controller
    {
        public ActionResult Index()
        {
            return View();

        }

        public ActionResult ThanhToan()
        {
            return View();
        }


       
        private RestaurantTableSystemEntities db = new RestaurantTableSystemEntities();
        // GET: Admin/RestaurantCategory
        public ActionResult DatBan(int id)
        {
            var restaurant = db.Restaurants.FirstOrDefault(r => r.restaurant_id == id);
            if (restaurant == null)
            {
                return HttpNotFound();
            }

            

            var viewModel = new RestaurantDetailViewModel
            {
                Restaurant = restaurant,
                
            };

            return View(viewModel);
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