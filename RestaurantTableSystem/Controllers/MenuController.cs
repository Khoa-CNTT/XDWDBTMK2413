using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RestaurantTableSystem.Models;

namespace RestaurantTableSystem.Controllers
{
    public class MenuController : Controller
    {
        private readonly RestaurantTableSystemEntities db = new RestaurantTableSystemEntities();

        private readonly List<string> validCategories = new List<string>
        {
            "main",
            "appetizer",
            "dessert",
            "drink"
        };

        // GET: Menu
        public ActionResult Index(int? restaurant_id)
        {
            if (Session["user"] == null)
            {
                TempData["Error"] = "Bạn cần đăng nhập để xem thực đơn.";
                return RedirectToAction("Login", "Account");
            }

            var user = Session["user"] as User;
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
            return View(menuItems);
        }

        // POST: Menu/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MenuItem menuItem, HttpPostedFileBase Image)
        {
            if (Session["user"] == null)
            {
                TempData["Error"] = "Bạn cần đăng nhập để thêm món ăn.";
                return RedirectToAction("Login", "Account");
            }

            var user = Session["user"] as User;
            var restaurant = db.Restaurants.FirstOrDefault(r => r.user_id == user.user_id);

            if (restaurant == null)
            {
                TempData["Error"] = "Không tìm thấy nhà hàng của bạn.";
                return RedirectToAction("Index", "Home");
            }

            // Assign restaurant_id to the menu item
            menuItem.restaurant_id = restaurant.restaurant_id;

            // Handle is_available checkbox
            if (Image != null && Image.ContentLength > 0)
            {
                var uploadsFolder = Server.MapPath("~/Content/Uploadss/MenuImages/");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Image.FileName);
                var fullPath = Path.Combine(uploadsFolder, fileName);
                Image.SaveAs(fullPath);

                menuItem.Image = "/Content/Uploadss/MenuImages/" + fileName;
            }
            else
            {
                menuItem.Image = "/Content/Uploadss/MenuImages/default.jpg";
            }

            db.MenuItems.Add(menuItem);
            db.SaveChanges();

            TempData["Success"] = "Thêm món ăn thành công.";
            return View("Index", db.MenuItems.Where(m => m.restaurant_id == restaurant.restaurant_id).ToList());


        }
        // POST: Menu/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MenuItem menuItem, HttpPostedFileBase Image)
        {
            if (Session["user"] == null)
            {
                TempData["Error"] = "Bạn cần đăng nhập để chỉnh sửa món ăn.";
                return RedirectToAction("Login", "Account");
            }

            var user = Session["user"] as User;
            var restaurant = db.Restaurants.FirstOrDefault(r => r.user_id == user.user_id);

            if (restaurant == null)
            {
                TempData["Error"] = "Không tìm thấy nhà hàng của bạn.";
                return RedirectToAction("Index", "Home");
            }

            // Handle is_available checkbox (unchecked sends null, set to false)
            if (!Request.Form.AllKeys.Contains("is_available"))
            {
                menuItem.is_available = false;
            }

            if (!validCategories.Contains(menuItem.category))
            {
                ModelState.AddModelError("category", "Danh mục không hợp lệ.");
            }

            if (ModelState.IsValid)
            {
                var existingItem = db.MenuItems.Find(menuItem.menu_item_id);
                if (existingItem == null)
                {
                    TempData["Error"] = "Không tìm thấy món ăn.";
                    return RedirectToAction("Index");
                }

                if (existingItem.restaurant_id != restaurant.restaurant_id)
                {
                    TempData["Error"] = "Bạn không có quyền chỉnh sửa món ăn này.";
                    return RedirectToAction("Index");
                }

                try
                {
                    existingItem.name = menuItem.name;
                    existingItem.category = menuItem.category;
                    existingItem.price = menuItem.price;
                    existingItem.description = menuItem.description;
                    existingItem.is_available = menuItem.is_available;

                    if (Image != null && Image.ContentLength > 0)
                    {
                        var validImageTypes = new[] { "image/jpeg", "image/png", "image/gif" };
                        if (!validImageTypes.Contains(Image.ContentType))
                        {
                            ModelState.AddModelError("Image", "Vui lòng chọn file ảnh (JPEG, PNG, GIF).");
                        }
                        else if (Image.ContentLength > 2 * 1024 * 1024) // 2MB
                        {
                            ModelState.AddModelError("Image", "Kích thước ảnh không được vượt quá 2MB.");
                        }
                        else
                        {
                            var uploadDir = Server.MapPath("~/Content/Uploadss/MenuImages/");
                            if (!Directory.Exists(uploadDir))
                            {
                                Directory.CreateDirectory(uploadDir);
                            }

                            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Image.FileName);
                            var path = Path.Combine(uploadDir, fileName);
                            Image.SaveAs(path);
                            existingItem.Image = "/Content/Uploadss/MenuImages/" + fileName;
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        db.SaveChanges();
                        TempData["Success"] = "Cập nhật món ăn thành công!";
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi khi cập nhật món ăn: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Exception: {ex.ToString()}");
                }
            }

            TempData["Error"] = "Có lỗi khi cập nhật món ăn. Vui lòng kiểm tra lại.";
            ViewBag.RestaurantId = restaurant.restaurant_id;
            ViewBag.Categories = validCategories;
            ViewBag.InvalidMenuItem = menuItem; // Pass the invalid model to preserve input
            return View("Index", db.MenuItems.Where(m => m.restaurant_id == restaurant.restaurant_id).ToList());
        }

        // POST: Menu/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, int restaurant_id)
        {
            if (Session["user"] == null)
            {
                TempData["Error"] = "Bạn cần đăng nhập để xóa món ăn.";
                return RedirectToAction("Login", "Account");
            }

            var user = Session["user"] as User;
            var restaurant = db.Restaurants.FirstOrDefault(r => r.user_id == user.user_id);

            if (restaurant == null || restaurant.restaurant_id != restaurant_id)
            {
                TempData["Error"] = "Bạn không có quyền xóa món ăn này.";
                return RedirectToAction("Index");
            }

            var menuItem = db.MenuItems.Find(id);
            if (menuItem == null)
            {
                TempData["Error"] = "Không tìm thấy món ăn.";
                return RedirectToAction("Index");
            }

            if (menuItem.restaurant_id != restaurant.restaurant_id)
            {
                TempData["Error"] = "Bạn không có quyền xóa món ăn này.";
                return RedirectToAction("Index");
            }

            try
            {
                db.MenuItems.Remove(menuItem);
                db.SaveChanges();
                TempData["Success"] = "Xóa món ăn thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi xóa món ăn: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.ToString()}");
            }

            return RedirectToAction("Index");
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