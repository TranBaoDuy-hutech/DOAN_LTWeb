namespace TravelWeb.Models
{
    public class TourAssignment
    {
        public int Id { get; set; }
        public int TourId { get; set; }
        public int GuideId { get; set; }
        public DateTime AssignedDate { get; set; }
        public Tour Tour { get; set; }
        public Guides Guide { get; set; }
        
    }
}
