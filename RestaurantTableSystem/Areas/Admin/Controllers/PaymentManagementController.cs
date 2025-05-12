using RestaurantTableSystem.Models;
using RestaurantTableSystem.Models.ViewModels;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Diagnostics;

namespace RestaurantTableSystem.Areas.Admin.Controllers
{
    public class PaymentManagementController : System.Web.Mvc.Controller
    {
        private RestaurantTableSystemEntities db = new RestaurantTableSystemEntities();

        public ActionResult Index()
        {
            var pendingPayments = (from b in db.Bookings
                                   join r in db.Restaurants on b.restaurant_id equals r.restaurant_id
                                   join u in db.Users on b.user_id equals u.user_id
                                   join p in db.Payments on b.booking_id equals p.booking_id
                                   where p.status == "pending"
                                   select new BookingViewModel
                                   {
                                       BookingId = b.booking_id,
                                       RestaurantName = r.name,
                                       RestaurantAddress = r.address ?? "Không có địa chỉ",
                                       CustomerName = u.full_name ?? "Unknown",
                                       PhoneNumber = u.phone ?? "Unknown",
                                       BookingTime = b.booking_time,
                                       NumberOfGuests = b.number_of_guests,
                                       AmountPaid = p.amount,
                                       SpecialRequest = b.special_request ?? "Không có",
                                       PaymentStatus = p.status
                                   }).ToList();

            return View(pendingPayments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmPayment(int bookingId)
        {
            try
            {
                Debug.WriteLine($"Nhận yêu cầu ConfirmPayment với bookingId: {bookingId}");

                using (var db = new RestaurantTableSystemEntities())
                {
                    var payment = db.Payments.FirstOrDefault(p => p.booking_id == bookingId);
                    if (payment == null)
                    {
                        Debug.WriteLine($"Không tìm thấy payment cho booking_id: {bookingId}");
                        return Json(new { success = false, message = "Không tìm thấy thông tin thanh toán." });
                    }

                    Debug.WriteLine($"Tìm thấy payment: booking_id={payment.booking_id}, status hiện tại={payment.status}");

                    if (payment.status != "pending")
                    {
                        Debug.WriteLine($"Trạng thái hiện tại không phải pending cho booking_id: {bookingId}, trạng thái: {payment.status}");
                        return Json(new { success = false, message = "Chỉ có thể xác nhận thanh toán khi trạng thái là 'pending'." });
                    }

                    payment.status = "completed";
                    Debug.WriteLine($"Đã cập nhật trạng thái thành 'completed' cho booking_id: {bookingId}");

                    db.Entry(payment).State = System.Data.Entity.EntityState.Modified;

                    int changes = db.SaveChanges();
                    Debug.WriteLine($"Đã lưu {changes} thay đổi vào database cho booking_id: {bookingId}");

                    if (changes == 0)
                    {
                        Debug.WriteLine($"Không có thay đổi nào được lưu cho booking_id: {bookingId}");
                        return Json(new { success = false, message = "Không thể cập nhật trạng thái thanh toán." });
                    }

                    return Json(new { success = true, message = "Xác nhận thanh toán thành công." });
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.InnerException?.Message ?? ex.Message;
                Debug.WriteLine($"Lỗi trong ConfirmPayment cho booking_id {bookingId}: {errorMessage}\nStackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = "Có lỗi xảy ra: " + errorMessage });
            }
        }
    }
}