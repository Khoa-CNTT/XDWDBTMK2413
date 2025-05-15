using RestaurantTableSystem.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace RestaurantTableSystem.Controllers
{
    public class ChatbotController : Controller
    {
        private RestaurantTableSystemEntities db = new RestaurantTableSystemEntities();

        [HttpPost]
        public JsonResult Ask(string question)
        {
            try
            {
                // Chuyển câu hỏi thành truy vấn
                var result = ProcessQuestion(question);
                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        private object ProcessQuestion(string question)
        {
            question = question.ToLower().Trim();

            // Quy tắc đơn giản để ánh xạ câu hỏi sang truy vấn
            if (question.Contains("nhà hàng") && question.Contains("hà nội"))
            {
                return db.Restaurants
                    .Where(r => r.address.Contains("Hà Nội"))
                    .Select(r => new { r.restaurant_id, r.name, r.address })
                    .ToList();
            }
            else if (question.Contains("món ăn") && question.Contains("dưới 100000"))
            {
                return db.MenuItems
                    .Where(m => m.price < 100000 && m.is_available == true)
                    .Select(m => new { m.menu_item_id, m.name, m.price, m.category })
                    .ToList();
            }
            else if (question.Contains("đặt bàn") && question.Contains("hôm nay"))
            {
                var today = DateTime.Today;
                return db.Bookings
                    .Where(b => b.booking_time.Date == today && b.status == "Confirmed")
                    .Select(b => new { b.booking_id, b.restaurant_id, b.number_of_guests })
                    .ToList();
            }
            // Thêm các quy tắc khác cho các bảng User, Order, Payment, Table, Review...

            return "Xin lỗi, tôi chưa hiểu câu hỏi. Vui lòng thử lại!";
        }
    }
}