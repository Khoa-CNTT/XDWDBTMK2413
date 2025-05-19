using RestaurantTableSystem.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

public class ThongBaoController : Controller
{
    private RestaurantTableSystemEntities db = new RestaurantTableSystemEntities();

    // Giả sử đã lưu user_id người quản lý trong session
    private int GetCurrentBusinessUserId()
    {
        return Convert.ToInt32(Session["user_id"]);
    }

    public ActionResult Index()
    {
        DeleteOldNotifications();
        int businessUserId = GetCurrentBusinessUserId();

        // Lấy danh sách các nhà hàng của business
        var restaurantIds = db.Restaurants
                              .Where(r => r.User.user_id == businessUserId)
                              .Select(r => r.restaurant_id)
                              .ToList();
        // Nạp dữ liệu bookings cùng với thông tin người dùng (User), loại bỏ các booking không có user_id
        var bookings = db.Bookings
                         .Include(b => b.User) // Nạp thông tin User
                         .Where(b => b.restaurant_id != null &&
                                     restaurantIds.Contains(b.restaurant_id.Value) &&
                                     b.user_id != null && b.status == "Đang xử lý")
                         .OrderByDescending(b => b.booking_time)
                         .ToList();

        // Kiểm tra dữ liệu (tùy chọn, để debug)
        foreach (var booking in bookings)
        {
            if (booking.User == null)
            {
                System.Diagnostics.Debug.WriteLine($"Booking {booking.booking_id} không có User liên kết.");
            }
        }

        return View(bookings);
    }

    [HttpPost]
    public ActionResult Confirm(int id)
    {
        var booking = db.Bookings.Include(b => b.Restaurant).FirstOrDefault(b => b.booking_id == id);
        if (booking == null)
            return HttpNotFound();

        booking.status = "Đã xác nhận";
        booking.booking_time = DateTime.Now;
        db.SaveChanges();
        return RedirectToAction("Index");
    }

    [HttpPost]
    public ActionResult Cancel(int id)
    {
        var booking = db.Bookings.Include(b => b.Restaurant).FirstOrDefault(b => b.booking_id == id);
        if (booking == null)
            return HttpNotFound();
        booking.status = "Đã huỷ";
        booking.booking_time = DateTime.Now;
        db.SaveChanges();

        return RedirectToAction("Index");
    }
    [ChildActionOnly]
    public ActionResult NotificationCount()
    {
        int businessUserId = Convert.ToInt32(Session["user_id"]);

        var restaurantIds = db.Restaurants
                              .Where(r => r.User.user_id == businessUserId)
                              .Select(r => r.restaurant_id)
                              .ToList();

        int count = db.Bookings
                      .Count(b => b.restaurant_id != null &&
                                  restaurantIds.Contains(b.restaurant_id.Value) &&
                                  b.user_id != null &&
                                  b.status == "Đang xử lý");

        ViewBag.NotificationCount = count;

        return PartialView("_NotificationCount");
    }
    private void DeleteOldNotifications()
    {
        DateTime limit = DateTime.Now.AddDays(-7);

        var expiredBookings = db.Bookings
            .Where(b => (b.status == "Đã xác nhận" || b.status == "Đã huỷ") &&
                        b.booking_time != null &&
                        b.booking_time < limit)
            .ToList();

        if (expiredBookings.Any())
        {
            db.Bookings.RemoveRange(expiredBookings);
            db.SaveChanges();
        }
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