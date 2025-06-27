using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TravelWeb.Data;
using TravelWeb.Models;

namespace TravelWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var bookings = _context.Bookings
                .Include(b => b.Tour)
                .Include(b => b.GroupBooking)
                .Where(b => b.GroupBookingId != null && b.GroupBooking.IsConfirmed)  // chỉ tour đoàn đã xác nhận
                .ToList();

            return View(bookings);
        }

        public IActionResult Details(int id)
        {
            var booking = _context.Bookings
                .Include(b => b.Tour)
                .FirstOrDefault(b => b.Id == id);

            if (booking == null)
                return NotFound();

            return View(booking);
        }

        public IActionResult Delete(int id)
        {
            var booking = _context.Bookings
                .Include(b => b.Tour)
                .FirstOrDefault(b => b.Id == id);

            if (booking == null)
                return NotFound();

            return View(booking);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var booking = _context.Bookings.FirstOrDefault(b => b.Id == id);
            if (booking == null)
                return NotFound();

            _context.Bookings.Remove(booking);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var booking = _context.Bookings.Find(id);
            if (booking == null)
                return NotFound();

            ViewBag.Tours = new SelectList(_context.Tours, "Id", "Name", booking.TourId);
            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Booking booking)
        {
            if (id != booking.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy thông tin tour để lấy giá
                    var tour = _context.Tours.Find(booking.TourId);
                    if (tour == null)
                    {
                        ModelState.AddModelError("", "Tour không tồn tại.");
                        ViewBag.Tours = new SelectList(_context.Tours, "Id", "Name", booking.TourId);
                        return View(booking);
                    }

                    // Cập nhật tổng tiền dựa trên số người và giá tour
                    booking.TotalAmount = booking.PeopleCount * tour.Price;

                    _context.Update(booking);
                    _context.SaveChanges();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Bookings.Any(e => e.Id == id))
                        return NotFound();
                    else
                        throw;
                }
            }

            ViewBag.Tours = new SelectList(_context.Tours, "Id", "Name", booking.TourId);
            return View(booking);
        }
        // GET: Hiển thị form phân công HDV cho tour
        public IActionResult AssignGuide(int groupBookingId)
        {
            var group = _context.GroupBookings
                .Include(g => g.Tour)
                .Include(g => g.Guide)
                .FirstOrDefault(g => g.Id == groupBookingId);

            if (group == null)
            {
                return NotFound();
            }

            ViewData["StartDate"] = group.DepartureDate.ToString("dd/MM/yyyy");
            ViewData["GroupBookingId"] = group.Id;
            ViewData["CurrentGuideId"] = group.GuideId;
            ViewData["CurrentGuideName"] = group.Guide?.Name;

            var availableGuides = _context.Guides.ToList();

            return View(availableGuides);
        }



        // POST: Nhận phân công HDV
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignGuide(int groupBookingId, int guideId)
        {
            var groupBooking = _context.GroupBookings.Find(groupBookingId);
            if (groupBooking == null)
                return NotFound();

            var targetDate = groupBooking.DepartureDate;

            bool isGuideBusy = _context.GroupBookings
                .Any(gb => gb.GuideId == guideId && gb.DepartureDate.Date == targetDate.Date && gb.Id != groupBookingId);

            if (isGuideBusy)
            {
                TempData["GuideAssignmentMessage"] = "Hướng dẫn viên này đã được phân công cho một tour khác cùng ngày. Vui lòng chọn HDV khác.";
                return RedirectToAction("AssignGuide", new { groupBookingId });
            }

            groupBooking.GuideId = guideId;
            _context.SaveChanges();

            TempData["GuideAssignmentMessage"] = "Phân công hướng dẫn viên thành công.";
            return RedirectToAction("Index");
        }



        public class BookingGroupViewModel
        {
            public int? GroupBookingId { get; set; }  // null nếu là nhóm gom khách lẻ
            public int TourId { get; set; }
            public string TourName { get; set; }
            public DateTime DepartureDate { get; set; }
            public int TotalPeople { get; set; }
            public decimal TotalAmount { get; set; }
            public bool IsRealGroup { get; set; }  // true nếu đoàn thật, false nếu gom khách lẻ
        }

    }
}
