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
                // Nếu chưa được duyệt => thông báo và không cho vào form
                if (existingRestaurant.is_approved == false || existingRestaurant.is_approved == null)
                {
                    TempData["Error"] = "Yêu cầu đăng ký nhà hàng của bạn đang được xử lý. Vui lòng đợi admin duyệt.";
                    return RedirectToAction("PendingNotice");
                }

                // Đã được duyệt => cho chỉnh sửa
                ViewBag.IsEdit = true;
                return View(existingRestaurant);
            }

            ViewBag.IsEdit = false;
            return View(new Restaurant());
        }
        public ActionResult PendingNotice()
        {
            return View();
        }

        // POST: Business
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

                    // Chỉ đặt lại trạng thái nếu đang là chưa duyệt
                    if (existingRestaurant.is_approved == null || existingRestaurant.is_approved == false)
                    {
                        existingRestaurant.is_approved = false;
                    }

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
                    TempData["Success"] = "Thông tin đã được cập nhật.";

                    // Nếu nhà hàng đã được duyệt, chuyển tới Menu
                    if (existingRestaurant.is_approved == true)
                        return RedirectToAction("Index", "Menu", new { restaurantId = existingRestaurant.restaurant_id });

                    return RedirectToAction("PendingNotice");
                }

                else
                {
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

                    TempData["Success"] = "Đăng ký thành công. Vui lòng đợi admin duyệt!";
                    return RedirectToAction("PendingNotice");
                }
            }

            return View(restaurant);
        }

    }
}
