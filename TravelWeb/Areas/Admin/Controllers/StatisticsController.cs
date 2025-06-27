using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using TravelWeb.Data;
using TravelWeb.Models;

namespace TravelWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class StatisticsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StatisticsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var data = _context.Bookings
                .Where(b => b.GroupBooking != null && b.GroupBooking.IsConfirmed)
                .GroupBy(b => b.BookingDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalRevenue = g.Sum(x => x.TotalAmount),
                    TotalBookings = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList();

            ViewBag.Dates = data.Select(x => x.Date.ToString("dd/MM/yyyy")).ToList();
            ViewBag.Revenues = data.Select(x => x.TotalRevenue).ToList();
            ViewBag.Bookings = data.Select(x => x.TotalBookings).ToList();
            ViewBag.TotalRevenue = data.Sum(x => x.TotalRevenue);
            ViewBag.TotalBookings = data.Sum(x => x.TotalBookings);

            return View(); // Không cần truyền Model
        }


        public IActionResult TourDistribution()
        {
    
            var tourRevenue = _context.Bookings
                .Include(b => b.Tour)
                .Include(b => b.GroupBooking)
                .Where(b => b.GroupBooking != null && b.GroupBooking.IsConfirmed) 
                .GroupBy(b => b.Tour.Name)
                .Select(g => new
                {
                    TourName = g.Key,
                    Revenue = g.Sum(b => b.TotalAmount)
                })
                .ToList();
    
   
            ViewBag.TourNames = tourRevenue.Select(x => x.TourName).ToList();
            ViewBag.Revenues = tourRevenue.Select(x => x.Revenue).ToList();
   
            return View();
        }
        public IActionResult GuideStats()
        {
            var hdvStats = _context.GroupBookings
                .Include(g => g.Guide)
                .Where(g => g.GuideId != null)
                .GroupBy(g => g.Guide.Name)
                .Select(g => new
                {
                    GuideName = g.Key,
                    TotalAssignedTours = g.Count()
                })
                .OrderByDescending(x => x.TotalAssignedTours)
                .ToList();

            ViewBag.GuideNames = hdvStats.Select(x => x.GuideName).ToList();
            ViewBag.AssignedCounts = hdvStats.Select(x => x.TotalAssignedTours).ToList();

            return View();
        }
        
    }
}
