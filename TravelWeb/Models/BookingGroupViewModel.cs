namespace TravelWeb.Models.ViewModels
{
    public class BookingGroupViewModel
    {
        public int? GroupBookingId { get; set; }
        public int TourId { get; set; }
        public string TourName { get; set; }
        public DateTime DepartureDate { get; set; }
        public int TotalPeople { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsRealGroup { get; set; }
        public string GuideName { get; set; }
        
    }
}
