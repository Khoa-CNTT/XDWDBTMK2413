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

        // Danh sách các danh mục hợp lệ (khớp với ràng buộc CHECK)
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
            if (restaurant_id.HasValue && !db.Restaurants.Any(r => r.restaurant_id == restaurant_id.Value))
            {
                return HttpNotFound("Nhà hàng không tồn tại.");
            }

            var menuItems = restaurant_id.HasValue
                ? db.MenuItems.Where(m => m.restaurant_id == restaurant_id.Value).ToList()
                : db.MenuItems.ToList();

            ViewBag.RestaurantId = restaurant_id;
            return View(menuItems);
        }

        // GET: Menu/Create
        public ActionResult Create(int? restaurant_id)
        {
            if (!restaurant_id.HasValue || !db.Restaurants.Any(r => r.restaurant_id == restaurant_id.Value))
            {
                return HttpNotFound("Nhà hàng không tồn tại.");
            }

            ViewBag.RestaurantId = restaurant_id;
            ViewBag.Categories = validCategories;
            return View();
        }

        // POST: Menu/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MenuItem menuItem, HttpPostedFileBase Image, int? restaurant_id)
        {
            // Kiểm tra restaurant_id
            const int fixedRestaurantId = 22; // Giá trị cố định theo yêu cầu
            if (!db.Restaurants.Any(r => r.restaurant_id == fixedRestaurantId))
            {
                ModelState.AddModelError("restaurant_id", "Nhà hàng với ID 22 không tồn tại.");
            }

            // Kiểm tra category
            if (!validCategories.Contains(menuItem.category))
            {
                ModelState.AddModelError("category", "Danh mục không hợp lệ. Vui lòng chọn một danh mục hợp lệ.");
            }

            if (ModelState.IsValid)
            {
                menuItem.restaurant_id = fixedRestaurantId; // Gán cố định restaurant_id = 22

                if (Image != null && Image.ContentLength > 0)
                {
                    var uploadDir = "~/Content/Uploadss/MenuImages/";
                    var physicalPath = Server.MapPath(uploadDir);

                    if (!Directory.Exists(physicalPath))
                    {
                        Directory.CreateDirectory(physicalPath);
                    }

                    var fileName = Path.GetFileName(Image.FileName);
                    var path = Path.Combine(physicalPath, fileName);
                    Image.SaveAs(path);
                    menuItem.Image = "/Content/Uploadss/MenuImages/" + fileName;
                }

                db.MenuItems.Add(menuItem);
                db.SaveChanges();

                // Chuyển hướng về /Menu?restaurantId=22
                return RedirectToAction("Index", new { restaurant_id = fixedRestaurantId });
            }

            ViewBag.RestaurantId = restaurant_id;
            ViewBag.Categories = validCategories;
            return View(menuItem);
        }

        // GET: Menu/Edit/5
        public ActionResult Edit(int id)
        {
            var menuItem = db.MenuItems.Find(id);
            if (menuItem == null)
            {
                return HttpNotFound();
            }
            if (!db.Restaurants.Any(r => r.restaurant_id == menuItem.restaurant_id))
            {
                return HttpNotFound("Nhà hàng không tồn tại.");
            }

            ViewBag.RestaurantId = menuItem.restaurant_id;
            ViewBag.Categories = validCategories;
            return View(menuItem);
        }

        // POST: Menu/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MenuItem menuItem, HttpPostedFileBase Image)
        {
            // Kiểm tra restaurant_id
            if (!db.Restaurants.Any(r => r.restaurant_id == menuItem.restaurant_id))
            {
                ModelState.AddModelError("restaurant_id", "Nhà hàng không hợp lệ.");
            }

            // Kiểm tra category
            if (!validCategories.Contains(menuItem.category))
            {
                ModelState.AddModelError("category", "Danh mục không hợp lệ. Vui lòng chọn một danh mục hợp lệ.");
            }

            if (ModelState.IsValid)
            {
                var existingItem = db.MenuItems.Find(menuItem.menu_item_id);
                if (existingItem == null)
                {
                    return HttpNotFound();
                }

                existingItem.name = menuItem.name;
                existingItem.category = menuItem.category;
                existingItem.price = menuItem.price;
                existingItem.description = menuItem.description;
                existingItem.is_available = menuItem.is_available;
                existingItem.restaurant_id = menuItem.restaurant_id;

                if (Image != null && Image.ContentLength > 0)
                {
                    var uploadDir = "~/Content/Uploadss/MenuImages/";
                    var physicalPath = Server.MapPath(uploadDir);

                    if (!Directory.Exists(physicalPath))
                    {
                        Directory.CreateDirectory(physicalPath);
                    }

                    var fileName = Path.GetFileName(Image.FileName);
                    var path = Path.Combine(physicalPath, fileName);
                    Image.SaveAs(path);
                    existingItem.Image = "/Content/Uploadss/MenuImages/" + fileName;
                }

                db.SaveChanges();
                return RedirectToAction("Index", new { restaurant_id = existingItem.restaurant_id });
            }

            ViewBag.RestaurantId = menuItem.restaurant_id;
            ViewBag.Categories = validCategories;
            return View(menuItem);
        }

        // GET: Menu/Delete/5
        public ActionResult Delete(int id)
        {
            var menuItem = db.MenuItems.Find(id);
            if (menuItem == null)
            {
                return HttpNotFound();
            }
            if (!db.Restaurants.Any(r => r.restaurant_id == menuItem.restaurant_id))
            {
                return HttpNotFound("Nhà hàng không tồn tại.");
            }

            ViewBag.RestaurantId = menuItem.restaurant_id;
            return View(menuItem);
        }

        // POST: Menu/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var menuItem = db.MenuItems.Find(id);
            if (menuItem == null)
            {
                return HttpNotFound();
            }
            if (!db.Restaurants.Any(r => r.restaurant_id == menuItem.restaurant_id))
            {
                return HttpNotFound("Nhà hàng không tồn tại.");
            }

            var restaurant_id = menuItem.restaurant_id;
            db.MenuItems.Remove(menuItem);
            db.SaveChanges();
            return RedirectToAction("Index", new { restaurant_id });
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