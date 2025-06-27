using System.ComponentModel.DataAnnotations;
using TravelWeb.Models;

namespace TravelWeb.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required]
        public string CustomerName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public int PeopleCount { get; set; }

        public DateTime DepartureDate { get; set; }

        public DateTime BookingDate { get; set; } = DateTime.Now;

        public int TourId { get; set; }
        public Tour? Tour { get; set; }

        public decimal TotalAmount { get; set; }

        public int? GroupBookingId { get; set; }
        public GroupBooking? GroupBooking { get; set; }
      
    }

}
