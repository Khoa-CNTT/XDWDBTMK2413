using RestaurantTableSystem.Models;
using RestaurantTableSystem.Models.ViewModels;
using System.Linq;
using System.Web.Mvc;

namespace RestaurantTableSystem.Controllers
{
    public class DetailNhaHangController : Controller
    {
        private RestaurantTableSystemEntities db = new RestaurantTableSystemEntities();

        // GET: DetailNhaHang/Index/5
        public ActionResult Index(int id)
        {
            var restaurant = db.Restaurants.FirstOrDefault(r => r.restaurant_id == id);
            if (restaurant == null)
            {
                return HttpNotFound();
            }

            var menuItems = db.MenuItems
                              .Where(m => m.restaurant_id == id && m.is_available == true)
                              .ToList();

            var viewModel = new RestaurantDetailViewModel
            {
                Restaurant = restaurant,
                MenuItems = menuItems
            };

            return View(viewModel);
        }

        public ActionResult Menu(int restaurantId)
        {
            var menuItems = db.MenuItems
                .Where(m => m.restaurant_id == restaurantId)
                .ToList();

            return View(menuItems);
        }
    }
}