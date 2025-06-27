using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelWeb.Data;
using TravelWeb.Models;
using TravelWeb.Models.ViewModels; 
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace TravelWeb.Areas.Employee.Controllers
{
    [Area("Employee")]
    public class LichPhanCongController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LichPhanCongController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Employee/LichPhanCong
        public async Task<IActionResult> Index()
        {
            var assignments = await _context.GroupBookings
                .Include(g => g.Tour)
                .Include(g => g.Guide)
                .Where(g => g.GuideId != null)
                .OrderBy(g => g.DepartureDate)
                .Select(g => new BookingGroupViewModel
                {
                    GroupBookingId = g.Id,
                    TourId = g.TourId,
                    TourName = g.Tour.Name,
                    DepartureDate = g.DepartureDate,
                    GuideName = g.Guide.Name,                  
                })
                .ToListAsync();

            return View(assignments);
        }
        

    }
}
