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
                    var payment = new Payment
                    {
                        booking_id = bookingId,
                        amount = 200000,
                        payment_method = paymentMethod.ToLower(), // "momo", "vnpay", v.v.
                        status = "completed",
                        transaction_id = Guid.NewGuid().ToString()
                    };

                    db.Payments.Add(payment);
                    db.SaveChanges();

                    return Json(new { success = true, bookingId = bookingId });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }


    }
}