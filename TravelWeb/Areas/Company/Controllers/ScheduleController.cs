using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelWeb.Data;
using TravelWeb.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

namespace TravelWeb.Areas.Company.Controllers
{
    [Area("Company")]
    [Authorize(Roles = "Company")]
    public class ScheduleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ScheduleController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy email của HDV đăng nhập
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized(); // hoặc xử lý khác nếu không lấy được email
            }

            // Tìm hướng dẫn viên theo email
            var guide = await _context.Guides.FirstOrDefaultAsync(g => g.Email == email);
            if (guide == null)
            {
                return NotFound("Không tìm thấy hướng dẫn viên.");
            }

            // Lấy danh sách GroupBooking của HDV đó, đã được xác nhận
            var schedule = await _context.GroupBookings
                .Include(g => g.Tour)
                .Include(g => g.Guide)
                .Where(g => g.GuideId == guide.Id && g.IsConfirmed == true)
                .OrderBy(g => g.DepartureDate)
                .ToListAsync();

            return View(schedule);
        }
    }
}
