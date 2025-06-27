using System;

namespace TravelWeb.Models
{
    public class Tour
    {
        public int Id { get; set; }
        public string Name { get; set; }                 // Tên tour
        public string Description { get; set; }          // Mô tả ngắn
        public string Itinerary { get; set; }            // Lịch trình chi tiết
        public string Location { get; set; }             // Địa điểm đến
        public string DepartureLocation { get; set; }    // Nơi khởi hành
        public decimal Price { get; set; }               // Giá tour
        public int DurationDays { get; set; }            // Thời gian tour (số ngày)
        public DateTime StartDate { get; set; }          // Ngày khởi hành
        public string Transportation { get; set; }       // Phương tiện
        public string TourType { get; set; }             // Loại tour
        public bool IsHot { get; set; }                  // Tour nổi bật
        public double Rating { get; set; }               // Điểm đánh giá
        public int ReviewCount { get; set; }             // Số lượt đánh giá
        public string HotelName { get; set; }            // Tên khách sạn
        public string ImageUrl { get; set; }             // Ảnh đại diện
       
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
