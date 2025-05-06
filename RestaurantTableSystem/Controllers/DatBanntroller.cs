using RestaurantTableSystem.Models;
using RestaurantTableSystem.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RestaurantTableSystem.Controllers
{
    public class DatBanController : Controller
    {
        public ActionResult Index()
        {
            return View();

        }





        private RestaurantTableSystemEntities db = new RestaurantTableSystemEntities();
        // GET: Admin/RestaurantCategory
        public ActionResult DatBan(int? id)
        {
            // Kiểm tra nếu id không có (null)
            if (id == null)
            {
                // Chuyển hướng về trang chủ hoặc trang lỗi tùy bạn
                return RedirectToAction("Index", "Home");
                // Hoặc: return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Tìm nhà hàng theo id
            var restaurant = db.Restaurants.FirstOrDefault(r => r.restaurant_id == id);
            if (restaurant == null)
            {
                return HttpNotFound();
            }

            // Tạo view model để truyền qua View
            var viewModel = new RestaurantDetailViewModel
            {
                Restaurant = restaurant,
            };

            return View(viewModel);
        }




        [HttpPost]
        public JsonResult LuuDatBan(Booking booking)
        {
            if (booking == null || booking.restaurant_id == null)
            {
                return Json(new { success = false, message = "Thông tin đặt bàn không hợp lệ." });
            }

            try
            {
                var userId = Session["user_id"] != null ? (int)Session["user_id"] : 0;
                if (userId == 0)
                {
                    return Json(new { success = false, message = "Chưa đăng nhập." });
                }

                // Kiểm tra bắt buộc
                if (booking.booking_time == DateTime.MinValue)
                    return Json(new { success = false, message = "Vui lòng chọn thời gian đặt bàn." });

                if (booking.number_of_guests <= 0)
                    return Json(new { success = false, message = "Số lượng khách không hợp lệ." });

                booking.user_id = userId;
                booking.status = "Đang xử lý  ";
                booking.special_request = string.IsNullOrEmpty(booking.special_request) ? "" : booking.special_request;

                db.Bookings.Add(booking);
                db.SaveChanges();

                return Json(new { success = true, bookingId = booking.booking_id });
            }
            catch (Exception ex)
            {
                string errorMessage = "";
                Exception currentEx = ex;

                // Lặp đến tận lỗi gốc
                while (currentEx.InnerException != null)
                {
                    currentEx = currentEx.InnerException;
                }

                errorMessage = currentEx.Message;

                return Json(new { success = false, message = "Lỗi thực tế: " + errorMessage });
            }

        }




        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}