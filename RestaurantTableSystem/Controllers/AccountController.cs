using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RestaurantTableSystem.Models;

namespace RestaurantTableSystem.Controllers
{
    public class AccountController : Controller
    {
        private RestaurantTableSystemEntities db = new RestaurantTableSystemEntities();
        // GET: Account
        //public ActionResult Index()
        //{
        //    return View();
        //}
        [HttpGet]
        public ActionResult Register()
        {
            return View(new User());
        }
        public ActionResult Logout()
        {
            Session.Clear(); // hoặc chỉ Session.Remove("user") và Session.Remove("UserId")
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

                model.password_hash = model.password_hash;
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
                    // Lưu session
                    Session["user"] = user;

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
            }

            return View(model);
        }


    }
}