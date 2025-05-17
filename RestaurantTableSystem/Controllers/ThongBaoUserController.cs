using RestaurantTableSystem.Models;
using System.Web.Mvc;
using System;
using System.Linq;

public class ThongBaoUserController : Controller
{
    private RestaurantTableSystemEntities db = new RestaurantTableSystemEntities();

    private int GetCurrentUserId()
    {
        if (Session["user_id"] == null)
        {
            // Có thể redirect hoặc throw exception
            throw new UnauthorizedAccessException("User chưa đăng nhập");
        }
        return Convert.ToInt32(Session["user_id"]);
    }

    // Danh sách thông báo
    public ActionResult Index()
    {
        int userId = GetCurrentUserId();

        var confirmedBookings = db.Bookings
                                  .Include("Restaurant")
                                  .Where(b => b.user_id == userId && (b.status == "Đã xác nhận" || b.status == "Đã huỷ"))
                                  .OrderByDescending(b => b.booking_time)
                                  .ToList();

        return View(confirmedBookings);
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