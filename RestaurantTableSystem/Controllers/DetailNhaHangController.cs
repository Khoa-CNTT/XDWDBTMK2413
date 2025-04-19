using RestaurantTableSystem.Models;
using System.Linq;
using System.Web.Mvc;

namespace RestaurantTableSystem.Controllers
{
    public class DetailNhaHangController : Controller
    {
        private RestaurantTableSystemEntities db = new RestaurantTableSystemEntities();

        // GET: DetailNhaHang/Index/5
        public ActionResult Index (int id)
        {
            var restaurant = db.Restaurants.FirstOrDefault(r => r.restaurant_id == id);
            if (restaurant == null)
            {
                return HttpNotFound();
            }
            return View(restaurant);
        }

    }
}
