using System;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Web.Mvc;
using RestaurantTableSystem.Models;
using RestaurantTableSystem.Models.ViewModels;

namespace RestaurantTableSystem.Areas.Admin.Controllers
{
    public class BanDatController : System.Web.Mvc.Controller
    {
        private RestaurantTableSystemEntities db = new RestaurantTableSystemEntities();

        public ActionResult ThongKe()
        {
            // Tính tổng số bàn đã đặt và tổng tiền cọc
            var totalBookings = db.Bookings.Count(b => b.status == "Đã xác nhận");
            var totalDeposit = (from b in db.Bookings
                                join p in db.Payments on b.booking_id equals p.booking_id into payments
                                from p in payments.DefaultIfEmpty()
                                where b.status == "Đã xác nhận"
                                select p != null ? p.amount : 0).Sum();

            // Thống kê theo nhà hàng
            var restaurantStats = (from b in db.Bookings
                                   join r in db.Restaurants on b.restaurant_id equals r.restaurant_id
                                   join p in db.Payments on b.booking_id equals p.booking_id into payments
                                   from p in payments.DefaultIfEmpty()
                                   where b.status == "Đã xác nhận"
                                   group new { b, p, r } by new { r.restaurant_id, r.name } into g
                                   select new RestaurantStatistics
                                   {
                                       RestaurantId = g.Key.restaurant_id,
                                       RestaurantName = g.Key.name,
                                       BookingCount = g.Count(),
                                       DepositAmount = g.Sum(x => x.p != null ? x.p.amount : 0)
                                   }).ToList();

            var model = new StatisticsViewModel
            {
                TotalBookings = totalBookings,
                TotalDeposit = totalDeposit,
                RestaurantStats = restaurantStats
            };

            System.Diagnostics.Debug.WriteLine($"Tổng số bookings: {totalBookings}, Tổng tiền cọc: {totalDeposit}, Số nhà hàng: {restaurantStats.Count}");

            return View(model);
        }

        // Action để lấy thống kê chi tiết theo ngày cho một nhà hàng (toàn bộ lịch sử)
        [HttpGet]
        public ActionResult GetRestaurantDailyStats(int restaurantId)
        {
            var dailyStats = (from b in db.Bookings
                              join p in db.Payments on b.booking_id equals p.booking_id into payments
                              from p in payments.DefaultIfEmpty()
                              where b.status == "Đã xác nhận" && b.restaurant_id == restaurantId && b.booking_time != null
                              group new { b, p } by DbFunctions.TruncateTime(b.booking_time) into g
                              select new DailyStatistics
                              {
                                  Date = g.Key.Value,
                                  BookingCount = g.Count(),
                                  DepositAmount = g.Sum(x => x.p != null ? x.p.amount : 0)
                              }).OrderBy(x => x.Date).ToList();

            System.Diagnostics.Debug.WriteLine($"RestaurantId: {restaurantId}, Số ngày thống kê: {dailyStats.Count}");
            if (!dailyStats.Any())
            {
                System.Diagnostics.Debug.WriteLine("Không có dữ liệu thống kê cho nhà hàng này.");
            }
            else
            {
                foreach (var stat in dailyStats)
                {
                    System.Diagnostics.Debug.WriteLine($"Ngày: {stat.Date:dd/MM/yyyy}, Số bàn: {stat.BookingCount}, Tiền cọc: {stat.DepositAmount}");
                }
            }

            return Json(dailyStats.Select(s => new
            {
                Date = s.Date.ToString("dd/MM/yyyy"),
                BookingCount = s.BookingCount,
                DepositAmount = s.DepositAmount
            }), JsonRequestBehavior.AllowGet);
        }

        // Action ListBanDat giữ nguyên
        public ActionResult ListBanDat()
        {
            var bookings = (from b in db.Bookings
                            join r in db.Restaurants on b.restaurant_id equals r.restaurant_id
                            join u in db.Users on b.user_id equals u.user_id
                            join p in db.Payments on b.booking_id equals p.booking_id into payments
                            from p in payments.DefaultIfEmpty()
                            where b.status == "Đã xác nhận"
                            select new BookingViewModel
                            {
                                BookingId = b.booking_id,
                                RestaurantName = r.name,
                                CustomerName = u.full_name ?? "Unknown",
                                PhoneNumber = u.phone ?? "Unknown",
                                BookingTime = b.booking_time,
                                NumberOfGuests = b.number_of_guests,
                                AmountPaid = p != null ? p.amount : (decimal?)null,
                                PaymentStatus = p != null ? p.status : null
                            }).ToList();

            System.Diagnostics.Debug.WriteLine($"Số lượng bookings admin trả về: {bookings.Count}");

            return View(bookings);
        }
    }
}