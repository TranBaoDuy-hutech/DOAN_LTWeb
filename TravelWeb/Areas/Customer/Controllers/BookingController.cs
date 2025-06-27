using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelWeb.Data;
using TravelWeb.Models;

namespace TravelWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "Customer")]
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;

        public BookingController(ApplicationDbContext context, IEmailSender emailSender, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _emailSender = emailSender;
            _userManager = userManager;
        }

        // GET: /Customer/Booking/MyBookings
        public async Task<IActionResult> MyBookings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var email = user.Email;

            var bookings = await _context.Bookings
                .Include(b => b.Tour)
                .Include(b => b.GroupBooking)
                    .ThenInclude(gb => gb.Guide) // Đây mới là đúng!
                .Where(b => b.Email == email)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();



            ViewBag.Email = email;
            return View("BookingHistory", bookings);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var booking = await _context.Bookings
                .Include(b => b.Tour)
                .FirstOrDefaultAsync(b => b.Id == id && b.Email == user.Email);

            if (booking == null) return NotFound();

            // Kiểm tra nếu tour đã cận ngày khởi hành (trước 3 ngày)
            if ((booking.DepartureDate - DateTime.Now).TotalDays < 3)
            {
                return BadRequest("Bạn chỉ có thể hủy tour trước ngày khởi hành ít nhất 3 ngày.");
            }

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            return Ok();
        }


    }
}
