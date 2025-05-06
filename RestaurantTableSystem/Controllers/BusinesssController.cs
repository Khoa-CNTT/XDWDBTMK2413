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
                    TempData["Error"] = "Bạn cần đăng nhập trước khi đăng ký doanh nghiệp.";
                    return RedirectToAction("Login", "Account");
                }

                var user = Session["user"] as User;

                if (user != null)
                {
                    var existingRestaurant = db.Restaurants
                        .FirstOrDefault(r => r.latitude == restaurant.latitude && r.longitude == restaurant.longitude);

                    if (existingRestaurant != null)
                    {
                        ModelState.AddModelError("", "Vị trí vĩ độ và kinh độ này đã được đăng ký trước đó. Vui lòng kiểm tra lại.");
                        return View(restaurant);
                    }

                    // Gán thông tin thêm
                    restaurant.user_id = user.user_id;
                    restaurant.created_at = DateTime.Now;
                    restaurant.is_approved = false;

                    // Xử lý hình ảnh
                    if (ImageUpload != null && ImageUpload.ContentLength > 0)
                    {
                        var uploadsFolder = Server.MapPath("~/Content/Uploadss/RestaurantImages/");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageUpload.FileName);
                        var fullPath = Path.Combine(uploadsFolder, fileName);

                        ImageUpload.SaveAs(fullPath);

                        restaurant.Image = "/Content/Uploadss/RestaurantImages/" + fileName;
                    }
                    else
                    {
                        restaurant.Image = "/Content/Uploadss/RestaurantImages/default.jpg"; // default nếu không upload ảnh
                    }

                    // ⚡ THÊM DÒNG NÀY
                    ModelState.Clear();

                    // Bây giờ mới lưu
                    db.Restaurants.Add(restaurant);
                    db.SaveChanges();


                    TempData["Success"] = "Đăng ký nhà hàng thành công. Hãy đăng ký món ăn cho nhà hàng của bạn!";

                    // Chuyển qua Menu Controller để thêm món
                    return RedirectToAction("Index", "Menu", new { restaurantId = restaurant.restaurant_id });
                }
                else
                {
                    TempData["Error"] = "Không tìm thấy người dùng trong session.";
                    return RedirectToAction("Login", "Account");
                }
            }

            // Nếu dữ liệu không hợp lệ
            return View(restaurant);
        }
    }
}
