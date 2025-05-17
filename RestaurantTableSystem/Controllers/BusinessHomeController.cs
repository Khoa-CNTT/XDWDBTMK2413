using System.IO;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RestaurantTableSystem.Models;
using RestaurantTableSystem.Models.ViewModels;

namespace RestaurantTableSystem.Controllers
{
    public class BusinessHomeController : Controller
    {
        private readonly RestaurantTableSystemEntities db = new RestaurantTableSystemEntities();

        public ActionResult Index()
        {
            if (Session["user"] == null)
            {
                TempData["Error"] = "Bạn cần đăng nhập để xem trang doanh nghiệp.";
                return RedirectToAction("Login", "Account");
            }

            var user = Session["user"] as User;
            if (user.role != "business")
            {
                TempData["Error"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Index", "Home");
            }

            var restaurant = db.Restaurants.FirstOrDefault(r => r.user_id == user.user_id);
            if (restaurant == null)
            {
                TempData["Error"] = "Không tìm thấy nhà hàng của bạn.";
                return RedirectToAction("Index", "Home");
            }

            var menuItems = db.MenuItems
                .Where(m => m.restaurant_id == restaurant.restaurant_id)
                .ToList();

            ViewBag.RestaurantId = restaurant.restaurant_id;
            ViewBag.RestaurantName = restaurant.name;
            ViewBag.RestaurantDescription = restaurant.description;
            ViewBag.RestaurantImage = restaurant.Image;

            return View(menuItems);
        }

        public ActionResult DaDatBan()
        {
            var userId = Session["user_id"] != null ? (int)Session["user_id"] : 0;
            if (userId == 0)
            {
                TempData["Error"] = "Bạn cần đăng nhập để xem trang này.";
                return RedirectToAction("Login", "Account");
            }

            var user = db.Users.FirstOrDefault(u => u.user_id == userId);
            if (user == null || user.role != "business")
            {
                TempData["Error"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Index", "Home");
            }

            var restaurant = db.Restaurants.FirstOrDefault(r => r.user_id == userId);
            if (restaurant == null)
            {
                TempData["Error"] = "Không tìm thấy nhà hàng của bạn.";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.RestaurantName = restaurant.name;

            var bookings = (from b in db.Bookings
                            join r in db.Restaurants on b.restaurant_id equals r.restaurant_id
                            join u in db.Users on b.user_id equals u.user_id
                            join p in db.Payments on b.booking_id equals p.booking_id into payments
                            from p in payments.DefaultIfEmpty()
                            where b.restaurant_id == restaurant.restaurant_id
                            select new BookingViewModel
                            {
                                BookingId = b.booking_id,
                                RestaurantName = r.name,
                                RestaurantAddress = r.address ?? "Không có địa chỉ",
                                CustomerName = u.full_name ?? "Unknown",
                                PhoneNumber = u.phone ?? "Unknown",
                                BookingTime = b.booking_time,
                                NumberOfGuests = b.number_of_guests,
                                AmountPaid = p != null ? p.amount : (decimal?)null,
                                SpecialRequest = b.special_request ?? "Không có",
                                PaymentStatus = p != null ? p.status : "Chưa thanh toán",
                                BookingStatus = b.status ?? "Không xác định"
                            }).ToList();

            System.Diagnostics.Debug.WriteLine($"Số lượng bookings trả về: {bookings.Count}");

            return View("DaDatBan", bookings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var userId = Session["user_id"] != null ? (int)Session["user_id"] : 0;
            if (userId == 0)
            {
                TempData["Error"] = "Bạn cần đăng nhập để hủy đặt bàn.";
                return RedirectToAction("Login", "Account");
            }

            var restaurant = db.Restaurants.FirstOrDefault(r => r.user_id == userId);
            if (restaurant == null)
            {
                TempData["Error"] = "Không tìm thấy nhà hàng của bạn.";
                return RedirectToAction("DaDatBan");
            }

            var booking = db.Bookings.Find(id);
            if (booking == null)
            {
                TempData["Error"] = "Không tìm thấy đặt bàn.";
                return RedirectToAction("DaDatBan");
            }

            if (booking.restaurant_id != restaurant.restaurant_id)
            {
                TempData["Error"] = "Bạn không có quyền hủy đặt bàn này.";
                return RedirectToAction("DaDatBan");
            }

            try
            {
                booking.status = "Đã hủy";
                db.SaveChanges();
                TempData["Success"] = "Hủy đặt bàn thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi hủy đặt bàn: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.ToString()}");
            }

            return RedirectToAction("DaDatBan");
        }

        public ActionResult CapNhatBusiness()
        {
            if (Session["user"] == null)
            {
                TempData["Error"] = "Bạn cần đăng nhập trước khi đăng ký doanh nghiệp.";
                return RedirectToAction("Login", "Account");
            }

            var user = Session["user"] as User;
            var existingRestaurant = db.Restaurants.FirstOrDefault(r => r.user_id == user.user_id);

            if (existingRestaurant != null)
            {
                if (existingRestaurant.is_approved == false || existingRestaurant.is_approved == null)
                {
                    TempData["Error"] = "Yêu cầu đăng ký nhà hàng của bạn đang được xử lý. Vui lòng đợi admin duyệt.";
                    return RedirectToAction("PendingNotice");
                }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CapNhatBusiness(Restaurant restaurant, HttpPostedFileBase ImageUpload)
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