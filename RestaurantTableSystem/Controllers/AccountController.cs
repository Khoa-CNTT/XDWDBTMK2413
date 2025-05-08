using RestaurantTableSystem.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace RestaurantTableSystem.Controllers
{
    public class AccountController : Controller
    {
        private RestaurantTableSystemEntities db = new RestaurantTableSystemEntities();

        [HttpGet]
        public ActionResult Register()
        {
            return View(new User());
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(User model)
        {
            if (ModelState.IsValid)
            {
                if (db.Users.Any(u => u.email == model.email))
                {
                    ModelState.AddModelError("email", "Email đã được sử dụng.");
                    return View(model);
                }

                model.role = "User";
                model.created_at = DateTime.Now;

                db.Users.Add(model);
                db.SaveChanges();

                TempData["RegisterSuccess"] = "Đăng ký thành công! Vui lòng đăng nhập.";

                return RedirectToAction("Login", "Account");
            }

            return View(model);
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View(new User());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(User model)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.FirstOrDefault(u => u.email == model.email && u.password_hash == model.password_hash);
                if (user != null)
                {
                    Session["user"] = user;
                    Session["user_id"] = user.user_id;

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
            }

            return View(model);
        }

        public ActionResult AccountInfo()
        {
            var userId = Session["user_id"] != null ? (int)Session["user_id"] : 0;
            if (userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = db.Users.FirstOrDefault(u => u.user_id == userId);
            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        [HttpGet]
        public ActionResult UpdateAccount()
        {
            var userId = Session["user_id"] != null ? (int)Session["user_id"] : 0;
            if (userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = db.Users.FirstOrDefault(u => u.user_id == userId);
            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateAccount(User model, string newPassword, string confirmPassword)
        {
            var userId = Session["user_id"] != null ? (int)Session["user_id"] : 0;
            if (userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = db.Users.FirstOrDefault(u => u.user_id == userId);
            if (user == null)
            {
                return HttpNotFound();
            }

            try
            {
                // Cập nhật họ và tên
                if (!string.IsNullOrEmpty(model.full_name) && model.full_name != user.full_name)
                {
                    if (model.full_name.Length > 100)
                    {
                        ViewBag.ErrorMessage = "Họ và tên không được vượt quá 100 ký tự.";
                        return View("UpdateAccount", user);
                    }
                    user.full_name = model.full_name;
                }

                // Cập nhật email
                if (!string.IsNullOrEmpty(model.email) && model.email != user.email)
                {
                    if (db.Users.Any(u => u.email == model.email && u.user_id != userId))
                    {
                        ViewBag.ErrorMessage = "Email đã được sử dụng.";
                        return View("UpdateAccount", user);
                    }
                    user.email = model.email;
                }

                // Cập nhật số điện thoại
                if (!string.IsNullOrEmpty(model.phone) && model.phone != user.phone)
                {
                    if (!System.Text.RegularExpressions.Regex.IsMatch(model.phone, @"^\+?\d{10,15}$"))
                    {
                        ViewBag.ErrorMessage = "Số điện thoại không hợp lệ.";
                        return View("UpdateAccount", user);
                    }
                    user.phone = model.phone;
                }

                // Cập nhật mật khẩu
                if (!string.IsNullOrEmpty(newPassword))
                {
                    if (newPassword != confirmPassword)
                    {
                        ViewBag.PasswordError = "Mật khẩu xác nhận không khớp.";
                        return View("UpdateAccount", user);
                    }
                    if (newPassword.Length < 6)
                    {
                        ViewBag.PasswordError = "Mật khẩu phải có ít nhất 6 ký tự.";
                        return View("UpdateAccount", user);
                    }
                    user.password_hash = newPassword;
                }

                db.SaveChanges();
                ViewBag.SuccessMessage = "Cập nhật thông tin tài khoản thành công.";
                return RedirectToAction("AccountInfo");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Lỗi khi cập nhật thông tin: " + ex.Message;
                return View("UpdateAccount", user);
            }
        }
    }
}


