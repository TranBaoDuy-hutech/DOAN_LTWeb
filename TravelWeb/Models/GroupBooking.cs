using System;
using System.Collections.Generic;

namespace TravelWeb.Models
{
    public class GroupBooking
    {
        public int Id { get; set; }

        public int TourId { get; set; }
        public Tour Tour { get; set; }

        public DateTime DepartureDate { get; set; }

        public int MinPeople { get; set; } = 10;

        public bool IsConfirmed { get; set; } = false;
        public int? GuideId { get; set; }  
        public Guides Guide { get; set; }   
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
