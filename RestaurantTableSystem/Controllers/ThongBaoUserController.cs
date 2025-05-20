using RestaurantTableSystem.Models;
using System.Web.Mvc;
using System;
using System.Linq;

public class ThongBaoUserController : Controller
{
    private RestaurantTableSystemEntities db = new RestaurantTableSystemEntities();

    private int GetCurrentUserId()
    {
        return Convert.ToInt32(Session["user_id"]);
    }

    // Danh sách thông báo
    public ActionResult Index()
    {
        DeleteOldNotifications();
        int userId = GetCurrentUserId();

        var confirmedBookings = db.Bookings
                                  .Include("Restaurant")
                                  .Where(b => b.user_id == userId && (b.status == "Đã xác nhận" || b.status == "Đã huỷ"))
                                  .OrderByDescending(b => b.booking_time)
                                  .ToList();

        return View(confirmedBookings);
    }
    [ChildActionOnly]
    public ActionResult NotificationCount()
    {
        int userId = GetCurrentUserId();

        int count = db.Bookings
                      .Count(b => b.user_id == userId && (b.status == "Đã xác nhận" || b.status == "Đã huỷ"));

        ViewBag.UserNotificationCount = count;
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

    // Chi tiết thông báo
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            db.Dispose();
        }
        base.Dispose(disposing);
    }
}