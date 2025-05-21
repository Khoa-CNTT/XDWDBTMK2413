using RestaurantTableSystem.Models;
using RestaurantTableSystem.Models.ViewModels;
using System;
using System.Data.Entity;
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

            var reviews = db.Reviews
                           .Include(r => r.User)
                           .Where(r => r.restaurant_id == id)
                           .OrderByDescending(r => r.created_at)
                           .ToList();

            var viewModel = new RestaurantDetailViewModel
            {
                Restaurant = restaurant,
                MenuItems = menuItems,
                Reviews = reviews
            };

            return View(viewModel);
        }

        // GET: DetailNhaHang/AllReviews/5
        public ActionResult AllReviews(int id)
        {
            var restaurant = db.Restaurants.FirstOrDefault(r => r.restaurant_id == id);
            if (restaurant == null)
            {
                return HttpNotFound();
            }

            var menuItems = db.MenuItems
                             .Where(m => m.restaurant_id == id && m.is_available == true)
                             .ToList();

            var reviews = db.Reviews
                           .Include(r => r.User)
                           .Where(r => r.restaurant_id == id)
                           .OrderByDescending(r => r.created_at)
                           .ToList();

            var viewModel = new RestaurantDetailViewModel
            {
                Restaurant = restaurant,
                MenuItems = menuItems,
                Reviews = reviews
            };

            return RedirectToAction("Index", new { id = id, showAll = "true" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitReview(int restaurantId, byte rating, string comment)
        {
            if (rating < 1 || rating > 5)
            {
                ModelState.AddModelError("rating", "Điểm đánh giá phải từ 1 đến 5 sao.");
            }

            if (string.IsNullOrWhiteSpace(comment))
            {
                ModelState.AddModelError("comment", "Bình luận không được để trống.");
            }

            if (ModelState.IsValid)
            {
                var review = new Review
                {
                    restaurant_id = restaurantId,
                    user_id = null,
                    rating = rating,
                    comment = comment,
                    created_at = DateTime.Now
                };

                db.Reviews.Add(review);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Đánh giá của bạn đã được gửi thành công!";
                return RedirectToAction("Index", new { id = restaurantId });
            }

            var restaurant = db.Restaurants.FirstOrDefault(r => r.restaurant_id == restaurantId);
            if (restaurant == null)
            {
                return HttpNotFound();
            }

            var menuItems = db.MenuItems
                             .Where(m => m.restaurant_id == restaurantId && m.is_available == true)
                             .ToList();

            var reviews = db.Reviews
                           .Include(r => r.User)
                           .Where(r => r.restaurant_id == restaurantId)
                           .OrderByDescending(r => r.created_at)
                           .ToList();

            var viewModel = new RestaurantDetailViewModel
            {
                Restaurant = restaurant,
                MenuItems = menuItems,
                Reviews = reviews
            };

            TempData["ErrorMessage"] = "Vui lòng kiểm tra lại thông tin đánh giá.";
            return View("Index", viewModel);
        }

        public ActionResult Menu(int restaurantId)
        {
            var menuItems = db.MenuItems
                             .Where(m => m.restaurant_id == restaurantId)
                             .ToList();

            return View(menuItems);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}