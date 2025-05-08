using System.Linq;
using System.Web.Mvc;
using RestaurantTableSystem.Models;
using RestaurantTableSystem.Models.ViewModels;

namespace RestaurantTableSystem.Areas.Admin.Controllers
{
    
    public class BanDatController : Controller
    {
        private RestaurantTableSystemEntities db = new RestaurantTableSystemEntities();

        // GET: Admin/BanDat/ListBanDat
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
                                AmountPaid = p != null ? p.amount : (decimal?)null
                            }).ToList();

            System.Diagnostics.Debug.WriteLine($"Số lượng bookings admin trả về: {bookings.Count}");

            return View(bookings);
        }
    }
}