using System.IO;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RestaurantTableSystem.Models;
using RestaurantTableSystem.Models.ViewModels;
using System.Configuration;
using System.Net.Mail;
using System.Net;

namespace RestaurantTableSystem.Controllers
{
    public class BusinessHomeController : Controller
    {
        private readonly RestaurantTableSystemEntities db = new RestaurantTableSystemEntities();

        public ActionResult Index()
        {
            if (Session["user"] == null)
            {
                TempData["Error"] = "Bạn cần đăng nhập để xem trang doanh nghiệp.";
                return RedirectToAction("Login", "Account");
            }

            var user = Session["user"] as User;
            if (user.role != "business")
            {
                TempData["Error"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Index", "Home");
            }

            var restaurant = db.Restaurants.FirstOrDefault(r => r.user_id == user.user_id);
            if (restaurant == null)
            {
                TempData["Error"] = "Không tìm thấy nhà hàng của bạn.";
                return RedirectToAction("Index", "Home");
            }

            var menuItems = db.MenuItems
                .Where(m => m.restaurant_id == restaurant.restaurant_id)
                .ToList();

            ViewBag.RestaurantId = restaurant.restaurant_id;
            ViewBag.RestaurantName = restaurant.name;
            ViewBag.RestaurantDescription = restaurant.description;
            ViewBag.RestaurantImage = restaurant.Image;

            return View(menuItems);
        }

        public ActionResult DaDatBan()
        {
            var userId = Session["user_id"] != null ? (int)Session["user_id"] : 0;
            if (userId == 0)
            {
                TempData["Error"] = "Bạn cần đăng nhập để xem trang này.";
                return RedirectToAction("Login", "Account");
            }

            var user = db.Users.FirstOrDefault(u => u.user_id == userId);
            if (user == null || user.role != "business")
            {
                TempData["Error"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Index", "Home");
            }

            var restaurant = db.Restaurants.FirstOrDefault(r => r.user_id == userId);
            if (restaurant == null)
            {
                TempData["Error"] = "Không tìm thấy nhà hàng của bạn.";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.RestaurantName = restaurant.name;

            var bookings = (from b in db.Bookings
                            join r in db.Restaurants on b.restaurant_id equals r.restaurant_id
                            join u in db.Users on b.user_id equals u.user_id
                            join p in db.Payments on b.booking_id equals p.booking_id into payments
                            from p in payments.DefaultIfEmpty()
                            where b.restaurant_id == restaurant.restaurant_id
                            select new BookingViewModel
                            {
                                BookingId = b.booking_id,
                                RestaurantName = r.name,
                                RestaurantAddress = r.address ?? "Không có địa chỉ",
                                CustomerName = u.full_name ?? "Unknown",
                                PhoneNumber = u.phone ?? "Unknown",
                                CustomerEmail = u.email ?? "Unknown",
                                BookingTime = b.booking_time,
                                NumberOfGuests = b.number_of_guests,
                                AmountPaid = p != null ? p.amount : (decimal?)null,
                                SpecialRequest = b.special_request ?? "Không có",
                                PaymentStatus = p != null ? p.status : "Chưa thanh toán",
                                BookingStatus = b.status ?? "Không xác định"
                            }).ToList();

            System.Diagnostics.Debug.WriteLine($"Số lượng bookings trả về: {bookings.Count}");

            return View("DaDatBan", bookings);
        }

       
        public ActionResult SendReminderEmail(int bookingId)
        {
            var userId = Session["user_id"] != null ? (int)Session["user_id"] : 0;
            if (userId == 0)
            {
                TempData["Error"] = "Bạn cần đăng nhập để thực hiện hành động này.";
                return RedirectToAction("Login", "Account");
            }

            var user = db.Users.FirstOrDefault(u => u.user_id == userId);
            if (user == null || user.role != "business")
            {
                TempData["Error"] = "Bạn không có quyền truy cập hành động này.";
                return RedirectToAction("Index", "Home");
            }

            var booking = (from b in db.Bookings
                           join r in db.Restaurants on b.restaurant_id equals r.restaurant_id
                           join u in db.Users on b.user_id equals u.user_id
                           where b.booking_id == bookingId && r.user_id == userId
                           select new
                           {
                               b.booking_id,
                               b.booking_time,
                               u.email,
                               u.full_name,
                               r.name,
                               b.number_of_guests,
                               b.special_request
                           }).FirstOrDefault();

            if (booking == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin đặt bàn.";
                return RedirectToAction("DaDatBan");
            }

            try
            {
                string subject = $"Nhắc nhở đặt bàn tại {booking.name}";
                string body = $@"Chào {booking.full_name},

Bạn có một đặt bàn tại nhà hàng {booking.name} vào lúc {booking.booking_time:dd/MM/yyyy HH:mm}.
Số lượng khách: {booking.number_of_guests}
Yêu cầu đặc biệt: {booking.special_request ?? "Không có"}

Vui lòng đến đúng giờ. Nếu có thay đổi, xin liên hệ với nhà hàng.

Trân trọng,
Restaurant Booking System";

                SendEmail(booking.email, subject, body);
                TempData["Success"] = "Email nhắc nhở đã được gửi thành công.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi gửi email: {ex.Message}";
            }

            return RedirectToAction("DaDatBan");
        }

        private void SendEmail(string toEmail, string subject, string body)
        {
            var fromEmail = ConfigurationManager.AppSettings["SmtpUser"];
            var fromPassword = ConfigurationManager.AppSettings["SmtpPass"];
            var smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
            var smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);

            var smtp = new SmtpClient
            {
                Host = smtpHost,
                Port = smtpPort,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail, fromPassword)
            };

            var fromAddress = new MailAddress(fromEmail, "Restaurant Booking System");
            var toAddress = new MailAddress(toEmail);

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            })
            {
                smtp.Send(message);
            }
        }
        public ActionResult CapNhatBusiness()
        {
            if (Session["user"] == null)
            {
                TempData["Error"] = "Bạn cần đăng nhập trước khi đăng ký doanh nghiệp.";
                return RedirectToAction("Login", "Account");
            }

            var user = Session["user"] as User;
            var existingRestaurant = db.Restaurants.FirstOrDefault(r => r.user_id == user.user_id);

            // Nếu nhà hàng đã tồn tại -> load dữ liệu cũ

            if (existingRestaurant != null)
            {
                // Nếu chưa được duyệt => thông báo và không cho vào form
                if (existingRestaurant.is_approved == false || existingRestaurant.is_approved == null)
                {
                    TempData["Error"] = "Yêu cầu đăng ký nhà hàng của bạn đang được xử lý. Vui lòng đợi admin duyệt.";
                    return RedirectToAction("PendingNotice");
                }

                // Đã được duyệt => cho chỉnh sửa
                ViewBag.IsEdit = true;
                return View(existingRestaurant);
            }

            ViewBag.IsEdit = false;
            return View(new Restaurant());
        }

        public ActionResult PendingNotice()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CapNhatBusiness(Restaurant restaurant, HttpPostedFileBase ImageUpload)
        {
            if (ModelState.IsValid)
            {
                if (Session["user"] == null)
                {
                    TempData["Error"] = "Bạn cần đăng nhập.";
                    return RedirectToAction("Login", "Account");
                }

                var user = Session["user"] as User;
                var existingRestaurant = db.Restaurants.FirstOrDefault(r => r.user_id == user.user_id);

                if (existingRestaurant != null)
                {
                    existingRestaurant.name = restaurant.name;
                    existingRestaurant.address = restaurant.address;
                    existingRestaurant.latitude = restaurant.latitude;
                    existingRestaurant.longitude = restaurant.longitude;
                    existingRestaurant.description = restaurant.description;
                    existingRestaurant.max_tables = restaurant.max_tables;
                    existingRestaurant.opening_hours = restaurant.opening_hours;
                    existingRestaurant.created_at = DateTime.Now;

                    // Chỉ đặt lại trạng thái nếu đang là chưa duyệt
                    if (existingRestaurant.is_approved == null || existingRestaurant.is_approved == false)
                    {
                        existingRestaurant.is_approved = false;
                    }

                    if (ImageUpload != null && ImageUpload.ContentLength > 0)
                    {
                        var uploadsFolder = Server.MapPath("~/Content/Uploadss/RestaurantImages/");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageUpload.FileName);
                        var fullPath = Path.Combine(uploadsFolder, fileName);
                        ImageUpload.SaveAs(fullPath);

                        existingRestaurant.Image = "/Content/Uploadss/RestaurantImages/" + fileName;
                    }

                    db.SaveChanges();
                    TempData["Success"] = "Thông tin đã được cập nhật.";

                    // Nếu nhà hàng đã được duyệt, chuyển tới Menu
                    if (existingRestaurant.is_approved == true)
                        return RedirectToAction("Index", "Menu", new { restaurantId = existingRestaurant.restaurant_id });

                    return RedirectToAction("PendingNotice");
                }

                else
                {
                    restaurant.user_id = user.user_id;
                    restaurant.created_at = DateTime.Now;
                    restaurant.is_approved = false;

                    if (ImageUpload != null && ImageUpload.ContentLength > 0)
                    {
                        var uploadsFolder = Server.MapPath("~/Content/Uploadss/RestaurantImages/");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageUpload.FileName);
                        var fullPath = Path.Combine(uploadsFolder, fileName);
                        ImageUpload.SaveAs(fullPath);

                        restaurant.Image = "/Content/Uploadss/RestaurantImages/" + fileName;
                    }
                    else
                    {
                        restaurant.Image = "/Content/Uploadss/RestaurantImages/default.jpg";
                    }

                    db.Restaurants.Add(restaurant);
                    db.SaveChanges();

                    TempData["Success"] = "Đăng ký thành công. Vui lòng đợi admin duyệt!";
                    return RedirectToAction("PendingNotice");
                }
            }

            return View(restaurant);
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
}