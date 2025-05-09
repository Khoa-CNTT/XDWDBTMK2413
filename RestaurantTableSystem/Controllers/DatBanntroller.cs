using RestaurantTableSystem.Models;
using RestaurantTableSystem.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;


namespace RestaurantTableSystem.Controllers
{
    public class DatBanController : Controller
    {
        private RestaurantTableSystemEntities db = new RestaurantTableSystemEntities();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DatBan(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Home");
            }

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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult LuuDatBan(Booking booking)
        {
            if (booking == null || booking.restaurant_id == null)
            {
                return Json(new { success = false, message = "Thông tin đặt bàn không hợp lệ." });
            }

            var userId = Session["user_id"] as int? ?? 0;
            if (userId == 0)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để đặt bàn." });
            }

            // Kiểm tra restaurant
            var restaurant = db.Restaurants.FirstOrDefault(r => r.restaurant_id == booking.restaurant_id);
            if (restaurant == null)
            {
                return Json(new { success = false, message = "Nhà hàng không tồn tại." });
            }

            // Kiểm tra user
            var user = db.Users.FirstOrDefault(u => u.user_id == userId);
            if (user == null)
            {
                return Json(new { success = false, message = "Thông tin người dùng không hợp lệ." });
            }

            try
            {
                if (booking.booking_time == DateTime.MinValue || booking.booking_time < DateTime.UtcNow)
                {
                    return Json(new { success = false, message = "Vui lòng chọn thời gian đặt bàn hợp lệ (sau thời gian hiện tại)." });
                }

                if (booking.number_of_guests <= 0 || booking.number_of_guests > 100)
                {
                    return Json(new { success = false, message = "Số lượng khách không hợp lệ." });
                }

                booking.user_id = userId;
                booking.status = "Đang xử lý";
                booking.special_request = string.IsNullOrEmpty(booking.special_request) ? "" : booking.special_request;

                db.Bookings.Add(booking);
                db.SaveChanges();

                return Json(new { success = true, bookingId = booking.booking_id });
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessage = ex.EntityValidationErrors
                    .SelectMany(e => e.ValidationErrors)
                    .Aggregate("", (current, validationError) => current + validationError.ErrorMessage + "\n");
                System.Diagnostics.Debug.WriteLine($"Validation Error in LuuDatBan: {errorMessage}");
                return Json(new { success = false, message = "Lỗi xác thực dữ liệu: " + errorMessage });
            }
            catch (Exception ex)
            {
                string errorMessage = ex.InnerException?.Message ?? ex.Message;
                System.Diagnostics.Debug.WriteLine($"Lỗi trong LuuDatBan: {errorMessage}\nStackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = "Lỗi thực tế: " + errorMessage });
            }
        }
        public ActionResult DanhSachBanDaDat()
        {
            var userId = Session["user_id"] != null ? (int)Session["user_id"] : 0;
            if (userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            System.Diagnostics.Debug.WriteLine($"User ID từ Session: {userId}");

            var bookings = (from b in db.Bookings
                            join r in db.Restaurants on b.restaurant_id equals r.restaurant_id
                            join u in db.Users on b.user_id equals u.user_id
                            join p in db.Payments on b.booking_id equals p.booking_id into payments
                            from p in payments.DefaultIfEmpty()
                            where b.status == "Đã xác nhận" && b.user_id == userId
                            select new BookingViewModel
                            {
                                BookingId = b.booking_id,
                                RestaurantName = r.name,
                                RestaurantAddress = r.address ?? "Không có địa chỉ", // Lấy địa chỉ nhà hàng
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
    }
}