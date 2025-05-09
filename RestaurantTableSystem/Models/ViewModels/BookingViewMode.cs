using System;

namespace RestaurantTableSystem.Models.ViewModels
{
    public class BookingViewModel
    {
        public int BookingId { get; set; }
        public string RestaurantName { get; set; }
        public string RestaurantAddress { get; set; } // Thêm thuộc tính địa chỉ nhà hàng
        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime BookingTime { get; set; }
        public int NumberOfGuests { get; set; }
        public decimal? AmountPaid { get; set; }
        public string SpecialRequest { get; set; }
        public string PaymentStatus { get; set; }
    }
}