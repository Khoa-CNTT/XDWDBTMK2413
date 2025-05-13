using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RestaurantTableSystem.Models;

namespace RestaurantTableSystem.Areas.Admin.Controllers
{
    public class RoleController : Controller
    {
        private RestaurantTableSystemEntities db = new RestaurantTableSystemEntities();

        // GET: Admin/Role
        public ActionResult Index()
        {
            var users = db.Users.ToList(); // Lấy toàn bộ user
            return View(users);
        }

        // POST: Admin/Role/UpdateRole
        [HttpPost]
        public ActionResult UpdateRole(int userId, string role)
        {
            try
            {
                if (!new[] { "Admin", "business", "User" }.Contains(role))
                {
                    return Json(new { success = false, message = "Vai trò không hợp lệ." });
                }

                var user = db.Users.Find(userId);
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy người dùng." });
                }

                user.role = role;
                db.SaveChanges();

                return Json(new { success = true, message = "Cập nhật vai trò thành công." });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật vai trò." });
            }
        }
    }
}