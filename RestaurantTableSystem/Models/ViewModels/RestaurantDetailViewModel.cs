using System.Collections.Generic;

namespace RestaurantTableSystem.Models.ViewModels
{
    public class RestaurantDetailViewModel
    {
        public Restaurant Restaurant { get; set; }
        public List<MenuItem> MenuItems { get; set; }
        public List<Review> Reviews { get; set; }
    }
}
