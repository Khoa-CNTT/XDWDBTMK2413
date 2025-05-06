using RestaurantTableSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RestaurantTableSystem.Controllers
{
    public class ThanhToanController : Controller
    {
        // GET: ThanhToan
        public ActionResult ThanhToan()
        {
            return View();
        }


        [HttpPost]
        public ActionResult LuuThanhToan(string paymentMethod, int? bookingId)
        {
            try
            {
                using (var db = new RestaurantTableSystemEntities())
                {
                    if (!bookingId.HasValue)
                    {
                        return Json(new { success = false, message = "Mã đặt bàn không hợp lệ." });
                    }

                    var booking = db.Bookings.FirstOrDefault(b => b.booking_id == bookingId);
                    if (booking == null)
                    {
                        return Json(new { success = false, message = "Không tìm thấy thông tin đặt bàn." });
                    }

                    var payment = new Payment
                    {
                        booking_id = bookingId,
                        amount = 200000,
                        payment_method = paymentMethod.ToLower(),
                        status = "completed",
                        transaction_id = Guid.NewGuid().ToString()
                    };

                    db.Payments.Add(payment);

                    // Cập nhật trạng thái booking với giá trị hợp lệ
                    booking.status = "Đã xác nhận"; // Sử dụng giá trị hợp lệ thay vì "Đã thanh toán"
                    db.SaveChanges();

                    string bookingCode = $"BK{bookingId.Value.ToString("D6")}";
                    return Json(new { success = true, bookingId = bookingId, bookingCode = bookingCode });
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += " | Inner Exception: " + ex.InnerException.Message;
                    if (ex.InnerException.InnerException != null)
                    {
                        errorMessage += " | Inner Inner Exception: " + ex.InnerException.InnerException.Message;
                    }
                }
                return Json(new { success = false, message = "Có lỗi xảy ra: " + errorMessage });
            }
        }


    }
}