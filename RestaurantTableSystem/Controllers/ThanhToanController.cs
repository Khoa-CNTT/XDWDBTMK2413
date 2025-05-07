using RestaurantTableSystem.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Diagnostics;

namespace RestaurantTableSystem.Controllers
{
    public class ThanhToanController : Controller
    {
        public ActionResult ThanhToan()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LuuThanhToan(string paymentMethod, int? bookingId)
        {
            try
            {
                Debug.WriteLine($"Nhận dữ liệu: paymentMethod={paymentMethod}, bookingId={bookingId}");

                if (string.IsNullOrEmpty(paymentMethod) || !bookingId.HasValue)
                {
                    Debug.WriteLine("Dữ liệu đầu vào không hợp lệ.");
                    return Json(new { success = false, message = "Dữ liệu đầu vào không hợp lệ." });
                }

                paymentMethod = paymentMethod.ToLower();
                if (paymentMethod != "momo" && paymentMethod != "vnpay")
                {
                    Debug.WriteLine($"Phương thức thanh toán không hợp lệ: {paymentMethod}");
                    return Json(new { success = false, message = "Phương thức thanh toán không được hỗ trợ." });
                }

                if (bookingId <= 0)
                {
                    Debug.WriteLine($"BookingId không hợp lệ: {bookingId}");
                    return Json(new { success = false, message = "Mã đặt bàn không hợp lệ." });
                }

                using (var db = new RestaurantTableSystemEntities())
                {
                    var booking = db.Bookings.FirstOrDefault(b => b.booking_id == bookingId);
                    if (booking == null)
                    {
                        Debug.WriteLine($"Không tìm thấy booking với booking_id: {bookingId}");
                        return Json(new { success = false, message = "Không tìm thấy thông tin đặt bàn." });
                    }

                    var payment = new Payment
                    {
                        booking_id = bookingId.Value,
                        amount = 200000M, // Sử dụng DECIMAL để khớp với SQL
                        payment_method = paymentMethod,
                        status = "completed",
                        transaction_id = Guid.NewGuid().ToString()
                    };

                    db.Payments.Add(payment);
                    booking.status = "Đã xác nhận";

                    int changes = db.SaveChanges();
                    Debug.WriteLine($"Đã lưu {changes} thay đổi vào database.");

                    string bookingCode = $"BK{bookingId.Value.ToString("D6")}";
                    return Json(new { success = true, bookingId = bookingId, bookingCode = bookingCode });
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.InnerException?.Message ?? ex.Message;
                Debug.WriteLine($"Lỗi trong LuuThanhToan: {errorMessage}\nStackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = "Có lỗi xảy ra: " + errorMessage });
            }
        }
    }
}