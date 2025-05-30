﻿using RestaurantTableSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RestaurantTableSystem.Controllers
{
    public class HomeController : Controller
    {
        private RestaurantTableSystemEntities db = new RestaurantTableSystemEntities();

        public ActionResult Index(string search)
        {
            // Chỉ lấy nhà hàng đã được duyệt
            IQueryable<Restaurant> restaurants = db.Restaurants
                                                   .Where(r => r.is_approved == true);

            // Nếu có từ khóa tìm kiếm, lọc thêm theo tên hoặc địa chỉ
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                restaurants = restaurants.Where(r =>
                    r.name.ToLower().Contains(search) ||
                    r.address.ToLower().Contains(search));
            }

            var restaurantList = restaurants.ToList();
            return View(restaurantList);
        }


        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }
    }
}