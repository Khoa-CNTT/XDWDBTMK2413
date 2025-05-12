using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RestaurantTableSystem.Models;

namespace RestaurantTableSystem.Controllers
{
    public class BusinesssController : Controller
    {
        private RestaurantTableSystemEntities db = new RestaurantTableSystemEntities();

        // GET: Business
        public ActionResult Index()
        {
            if (Session["user"] == null)
            {
                TempData["Error"] = "Bạn cần đăng nhập trước khi đăng ký doanh nghiệp.";
                return RedirectToAction("Login", "Account");
            }

            var user = Session["user"] as User;
            var existingRestaurant = db.Restaurants.FirstOrDefault(r => r.user_id == user.user_id);

            // Nếu nhà hàng đã tồn tại -> load dữ liệu cũ
            if (existingRestaurant != null)
            {
                ViewBag.IsEdit = true;
                return View(existingRestaurant);
            }

            ViewBag.IsEdit = false;
            return View(new Restaurant());
        }

        // POST: Business
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(Restaurant restaurant, HttpPostedFileBase ImageUpload)
        {
            if (ModelState.IsValid)
            {
                if (Session["user"] == null)
                {
                    TempData["Error"] = "Bạn cần đăng nhập.";
                    return RedirectToAction("Login", "Account");
                }

                var user = Session["user"] as User;
                var existingRestaurant = db.Restaurants.FirstOrDefault(r => r.user_id == user.user_id);

                // Nếu nhà hàng đã tồn tại -> cập nhật
                if (existingRestaurant != null)
                {
                    existingRestaurant.name = restaurant.name;
                    existingRestaurant.address = restaurant.address;
                    existingRestaurant.latitude = restaurant.latitude;
                    existingRestaurant.longitude = restaurant.longitude;
                    existingRestaurant.description = restaurant.description;
                    existingRestaurant.max_tables = restaurant.max_tables;
                    existingRestaurant.opening_hours = restaurant.opening_hours;
                    existingRestaurant.created_at = DateTime.Now;
                    existingRestaurant.is_approved = false;

                    if (ImageUpload != null && ImageUpload.ContentLength > 0)
                    {
                        var uploadsFolder = Server.MapPath("~/Content/Uploadss/RestaurantImages/");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageUpload.FileName);
                        var fullPath = Path.Combine(uploadsFolder, fileName);
                        ImageUpload.SaveAs(fullPath);

                        existingRestaurant.Image = "/Content/Uploadss/RestaurantImages/" + fileName;
                    }

                    db.SaveChanges();
                    TempData["Success"] = "Cập nhật thông tin nhà hàng thành công!";
                    return RedirectToAction("Index", "Menu", new { restaurantId = existingRestaurant.restaurant_id });
                }
                else
                {
                    // Nếu chưa có nhà hàng, thì tạo mới
                    restaurant.user_id = user.user_id;
                    restaurant.created_at = DateTime.Now;
                    restaurant.is_approved = false;

                    if (ImageUpload != null && ImageUpload.ContentLength > 0)
                    {
                        var uploadsFolder = Server.MapPath("~/Content/Uploadss/RestaurantImages/");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageUpload.FileName);
                        var fullPath = Path.Combine(uploadsFolder, fileName);
                        ImageUpload.SaveAs(fullPath);

                        restaurant.Image = "/Content/Uploadss/RestaurantImages/" + fileName;
                    }
                    else
                    {
                        restaurant.Image = "/Content/Uploadss/RestaurantImages/default.jpg";
                    }

                    db.Restaurants.Add(restaurant);
                    db.SaveChanges();

                    TempData["Success"] = "Đăng ký nhà hàng thành công.";
                    return RedirectToAction("Index", "Menu", new { restaurantId = restaurant.restaurant_id });
                }
            }

            return View(restaurant);
        }
    }
}
