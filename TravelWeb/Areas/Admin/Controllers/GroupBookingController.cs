using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelWeb.Data;
using System.Linq;

namespace TravelWeb.Areas.Admin.Controllers  
{
    [Area("Admin")]
    public class GroupBookingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GroupBookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var groups = _context.GroupBookings
                .Include(g => g.Tour)
                .Include(g => g.Bookings)
                .Where(g => g.Tour != null)
                .ToList();

            return View(groups);
        }

        // Xác nhận thủ công đoàn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Confirm(int id)
        {
            var group = _context.GroupBookings.Find(id);
            if (group == null)
                return NotFound();

            group.IsConfirmed = true;
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // Huỷ đoàn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(int id)
        {
            var group = _context.GroupBookings
                .Include(g => g.Bookings)
                .FirstOrDefault(g => g.Id == id);

            if (group == null)
                return NotFound();

            // Xóa các Booking liên quan trước
            _context.Bookings.RemoveRange(group.Bookings);

            // Xóa đoàn
            _context.GroupBookings.Remove(group);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
