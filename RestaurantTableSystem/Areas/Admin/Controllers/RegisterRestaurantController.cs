using System.Linq;
using System.Web.Mvc;
using RestaurantTableSystem.Models;

namespace RestaurantTableSystem.Areas.Admin.Controllers
{
    public class RegisterRestaurantController : Controller
    {
        private RestaurantTableSystemEntities db = new RestaurantTableSystemEntities();

        // GET: Admin/RegisterRestaurant
        public ActionResult Index()
        {
            // Lấy danh sách nhà hàng chưa được duyệt
            var pendingRestaurants = db.Restaurants
                                       .Where(r => r.is_approved == false || r.is_approved == null)
                                       .OrderByDescending(r => r.created_at)
                                       .ToList();

            return View(pendingRestaurants);
        }

        // Duyệt nhà hàng
        public ActionResult Approve(int id)
        {
            var restaurant = db.Restaurants.Find(id);
            if (restaurant != null)
            {
                restaurant.is_approved = true;
                db.SaveChanges();
                TempData["Success"] = "Nhà hàng đã được duyệt.";
            }
            return RedirectToAction("Index");
        }

        // Từ chối và xoá
        public ActionResult Reject(int id)
        {
            var restaurant = db.Restaurants.Find(id);
            if (restaurant != null)
            {
                db.Restaurants.Remove(restaurant);
                db.SaveChanges();
                TempData["Success"] = "Nhà hàng đã bị từ chối và xoá.";
            }
            return RedirectToAction("Index");
        }
    }
}
