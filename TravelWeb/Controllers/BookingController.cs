using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using TravelWeb.Data;
using TravelWeb.Models;
using Microsoft.AspNetCore.Identity;

namespace TravelWeb.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<BookingController> _logger;
        private const int MinGroupPeople = 10;

        public BookingController(
            ApplicationDbContext context,
            IEmailSender emailSender,
            ILogger<BookingController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Create(int tourId)
        {
            var tour = await _context.Tours.FindAsync(tourId);
            if (tour == null)
            {
                _logger.LogWarning("Tour không tồn tại: tourId={TourId}", tourId);
                return NotFound();
            }

            ViewBag.Tour = tour;
            return View(new Booking { TourId = tourId, DepartureDate = tour.StartDate });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            var tour = await _context.Tours.FindAsync(booking.TourId);
            if (tour == null)
            {
                _logger.LogWarning("Tour không tồn tại: tourId={TourId}", booking.TourId);
                ModelState.AddModelError("", "Tour không tồn tại.");
                return View(booking);
            }

            ViewBag.Tour = tour;

            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        _logger.LogWarning($"Model error: {state.Key} - {error.ErrorMessage}");
                    }
                }
                return View(booking);
            }

            if (booking.PeopleCount <= 0)
            {
                ModelState.AddModelError(nameof(booking.PeopleCount), "Số lượng khách phải lớn hơn 0.");
                return View(booking);
            }

            if (booking.DepartureDate.Date <= DateTime.Now.Date)
            {
                ModelState.AddModelError(nameof(booking.DepartureDate), "Ngày khởi hành phải sau ngày hiện tại.");
                return View(booking);
            }

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                booking.TotalAmount = tour.Price * booking.PeopleCount;
                booking.BookingDate = DateTime.Now;

                var group = await _context.GroupBookings
                    .FirstOrDefaultAsync(g => g.TourId == booking.TourId && g.DepartureDate.Date == booking.DepartureDate.Date);

                if (group == null)
                {
                    group = new GroupBooking
                    {
                        TourId = booking.TourId,
                        DepartureDate = booking.DepartureDate,
                        MinPeople = MinGroupPeople,
                        IsConfirmed = false
                    };

                    _context.GroupBookings.Add(group);
                    await _context.SaveChangesAsync();
                }

                booking.GroupBookingId = group.Id;

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Đã lưu booking ID: {BookingId}", booking.Id);

                await transaction.CommitAsync();

                await SendBookingConfirmationEmail(booking, tour);

                // Kiểm tra xác nhận đoàn nếu đủ người
                var fullGroup = await _context.GroupBookings
                    .Include(g => g.Bookings)
                    .FirstOrDefaultAsync(g => g.Id == booking.GroupBookingId);

                if (fullGroup != null)
                {
                    await CheckAndConfirmGroup(fullGroup);
                }

                return RedirectToAction("Success", new { id = booking.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo booking: tourId={TourId}", booking.TourId);
                ModelState.AddModelError("", $"Đã xảy ra lỗi: {ex.Message}");
                ViewBag.Tour = tour;
                return View(booking);
            }
        }

        public async Task<IActionResult> Success(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Tour)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                _logger.LogWarning("Booking không tồn tại: bookingId={BookingId}", id);
                TempData["Error"] = "Booking không tồn tại.";
                return RedirectToAction("Index", "Tour");
            }

            ViewBag.Tour = booking.Tour;
            return View(booking);
        }

        private async Task SendBookingConfirmationEmail(Booking booking, Tour tour)
        {
            var body = $@"
            <div style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: auto; border: 1px solid #eee; padding: 20px; border-radius: 8px;'>
                <h2 style='color: #007bff; text-align: center; margin-bottom: 20px;'>XÁC NHẬN ĐẶT TOUR THÀNH CÔNG</h2>
                <div style='text-align: center; margin-bottom: 20px;'>
                    <img src='https://placehold.co/150x50/F0F8FF/007bff?text=VIETLU+TRAVEL' alt='Việt Lữ Travel Logo' style='max-width: 150px; height: auto;'/>
                </div>
                <p>Kính gửi <strong>{booking.CustomerName}</strong>,</p>
                <p>
                    Cảm ơn quý khách đã tin tưởng và lựa chọn <strong>Việt Lữ Travel</strong> cho hành trình sắp tới. Chúng tôi đã tiếp nhận thông tin đặt tour của quý khách với chi tiết như sau:
                </p>
                <table style='width: 100%; border-collapse: collapse; margin: 20px 0; border: 1px solid #ddd;'>
                    <tr>
                        <td style='padding: 10px; border: 1px solid #ddd; background-color: #f8f8f8; font-weight: bold;'>Mã Booking:</td>
                        <td style='padding: 10px; border: 1px solid #ddd;'><strong>#{booking.Id}</strong></td>
                    </tr>
                    <tr>
                        <td style='padding: 10px; border: 1px solid #ddd; background-color: #f8f8f8; font-weight: bold;'>Tên Tour:</td>
                        <td style='padding: 10px; border: 1px solid #ddd;'>{tour.Name}</td>
                    </tr>
                    <tr>
                        <td style='padding: 10px; border: 1px solid #ddd; background-color: #f8f8f8; font-weight: bold;'>Ngày khởi hành:</td>
                        <td style='padding: 10px; border: 1px solid #ddd;'>{booking.DepartureDate:dd/MM/yyyy}</td>
                    </tr>
                    <tr>
                        <td style='padding: 10px; border: 1px solid #ddd; background-color: #f8f8f8; font-weight: bold;'>Số lượng khách:</td>
                        <td style='padding: 10px; border: 1px solid #ddd;'>{booking.PeopleCount} người</td>
                    </tr>
                    <tr>
                        <td style='padding: 10px; border: 1px solid #ddd; background-color: #f8f8f8; font-weight: bold;'>Tổng chi phí:</td>
                        <td style='padding: 10px; border: 1px solid #ddd; color: #007bff; font-weight: bold;'>{booking.TotalAmount:N0} VNĐ</td>
                    </tr>
                    <tr>
                        <td style='padding: 10px; border: 1px solid #ddd; background-color: #f8f8f8; font-weight: bold;'>Số tiền cọc (40%):</td>
                        <td style='padding: 10px; border: 1px solid #ddd; color: #007bff; font-weight: bold;'>{booking.TotalAmount * 0.4m:N0} VNĐ</td>
                    </tr>
                </table>
                <p style='margin-top: 25px;'>
                    Nếu quý khách có bất kỳ thắc mắc nào, xin vui lòng liên hệ với chúng tôi:
                </p>
                <ul style='list-style: none; padding: 0;'>
                    <li style='margin-bottom: 5px;'><strong style='color: #007bff;'>Hotline:</strong> 1900 1234</li>
                    <li><strong style='color: #007bff;'>Email:</strong> support@vietlutravel.com</li>
                </ul>
                <p style='margin-top: 30px;'>Trân trọng,<br/><strong>Đội ngũ Việt Lữ Travel</strong></p>
                <hr style='margin-top: 30px; border: 0; border-top: 1px solid #eee;'/>
                <small style='color: #888; display: block; text-align: center;'>Quý khách nhận được email này vì đã đặt tour tại website của Việt Lữ Travel.</small>
            </div>";

            await _emailSender.SendEmailAsync(
                booking.Email,
                "Xác nhận đặt tour thành công - Việt Lữ Travel",
                body
            );

            _logger.LogInformation("Đã gửi email xác nhận booking cho: {Email}, BookingId={BookingId}", booking.Email, booking.Id);
        }

        private async Task CheckAndConfirmGroup(GroupBooking group)
        {
            var total = await _context.Bookings
                .Where(b => b.GroupBookingId == group.Id)
                .SumAsync(b => b.PeopleCount);

            if (total >= group.MinPeople && !group.IsConfirmed)
            {
                group.IsConfirmed = true;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Xác nhận nhóm thành công: groupId={GroupId}, totalPeople={TotalPeople}", group.Id, total);
            }
        }
        

    }
}