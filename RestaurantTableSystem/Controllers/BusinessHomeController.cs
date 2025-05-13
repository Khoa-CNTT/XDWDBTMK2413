using System.Linq;
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

        public ActionResult BanDaDat()
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
                            where b.status == "Đã xác nhận" && b.restaurant_id == restaurant.restaurant_id
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
                                PaymentStatus = p != null ? p.status : "Chưa thanh toán"
                            }).ToList();

            System.Diagnostics.Debug.WriteLine($"Số lượng bookings trả về: {bookings.Count}");

            return View(bookings);
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