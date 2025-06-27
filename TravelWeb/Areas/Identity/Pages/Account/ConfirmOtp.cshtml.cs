using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace TravelWeb.Areas.Identity.Pages.Account
{
    public class ConfirmOtpModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<ConfirmOtpModel> _logger;
        private readonly IEmailSender _emailSender;

        private const int OtpExpiryMinutes = 1;

        public ConfirmOtpModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<ConfirmOtpModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public string ReturnUrl { get; set; }
        public string UserId { get; set; }
        public string UserEmail { get; set; }
        public long OtpCreationUnixTimestamp { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập mã OTP.")]
            [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải có 6 chữ số.")]
            [Display(Name = "Mã OTP")]
            public string OtpCode { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string userId, string email, string returnUrl = null)
        {
            if (!TryGetOtpData(userId, out var unixTimestamp))
            {
                await LogAndDeleteUnconfirmedUser(userId, email, "Thiếu hoặc sai dữ liệu OTP khi tải trang.");
                StatusMessage = "Mã OTP đã hết hạn hoặc không hợp lệ. Vui lòng đăng ký lại.";
                return RedirectToPage("Register");
            }


            if (IsOtpExpired(unixTimestamp))
            {
                await LogAndDeleteUnconfirmedUser(userId, email, "OTP hết hạn khi tải trang.");
                StatusMessage = "Mã OTP đã hết hạn. Vui lòng đăng ký lại.";
                return RedirectToPage("Register");
            }

            UserId = userId;
            UserEmail = email;
            ReturnUrl = returnUrl;
            OtpCreationUnixTimestamp = unixTimestamp;

            TempData.Keep();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string userId, string email, string returnUrl = null)
        {
            UserId = userId;
            UserEmail = email;
            ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                TryGetOtpData(userId, out var timestamp);
                OtpCreationUnixTimestamp = timestamp;
                TempData.Keep();
                return Page();
            }

            if (!TryGetOtpData(userId, out var unixTimestamp))
            {
                await LogAndDeleteUnconfirmedUser(userId, email, "Thiếu dữ liệu OTP khi gửi.");
                StatusMessage = "Mã OTP đã hết hạn hoặc không hợp lệ. Vui lòng đăng ký lại.";
                return RedirectToPage("Register");
            }

            if (IsOtpExpired(unixTimestamp))
            {
                await LogAndDeleteUnconfirmedUser(userId, email, "OTP hết hạn khi gửi.");
                StatusMessage = "Mã OTP đã hết hạn. Vui lòng đăng ký lại.";
                return RedirectToPage("Register");
            }

            OtpCreationUnixTimestamp = unixTimestamp;

            var storedOtp = TempData.Peek($"OtpCode_{userId}")?.ToString();
            if (Input.OtpCode != storedOtp)
            {
                _logger.LogWarning("Người dùng nhập sai mã OTP. userId: {UserId}, email: {Email}", userId, email);
                StatusMessage = "Mã OTP không đúng. Vui lòng thử lại.";
                TempData.Keep();
                return Page();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Tài khoản không tồn tại. userId: {UserId}, email: {Email}", userId, email);
                StatusMessage = "Tài khoản không tồn tại hoặc đã bị hủy.";
                return Page();
            }

            var token = TempData.Peek($"EmailConfirmToken_{userId}")?.ToString();
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    await LogAndDeleteUnconfirmedUser(userId, email, $"Xác nhận email thất bại: {error.Description}");
                    StatusMessage = $"Lỗi xác nhận email: {error.Description}";
                    return Page();
                }
            }

            _logger.LogInformation("Email xác nhận thành công cho userId: {UserId}", userId);
            ClearTempData(userId);
            await _signInManager.SignInAsync(user, isPersistent: false);
            return LocalRedirect(returnUrl ?? "~/");
        }

        public async Task<IActionResult> OnPostResendOtpAsync(string userId, string email, string returnUrl = null)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
            {
                StatusMessage = "Không thể gửi lại mã OTP. Vui lòng đăng ký lại.";
                return RedirectToPage("Register");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || await _userManager.IsEmailConfirmedAsync(user))
            {
                StatusMessage = "Tài khoản không tồn tại hoặc đã xác nhận.";
                return RedirectToPage("Login");
            }

            var otpCode = new Random().Next(100000, 999999).ToString();
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            TempData[$"OtpCode_{userId}"] = otpCode;
            TempData[$"EmailConfirmToken_{userId}"] = token;
            TempData[$"OtpCreationTime_{userId}"] = timestamp.ToString();
            TempData["UserIdForOtp"] = userId;

            var message = $@"
                <p>Xin chào,</p>
                <p>Mã OTP mới của bạn là: <strong>{otpCode}</strong></p>
                <p>Mã này sẽ hết hạn sau {OtpExpiryMinutes} phút.</p>
                <p>Trân trọng,<br/> Việt Lữ Travel </p>";

            try
            {
                await _emailSender.SendEmailAsync(email, "Mã xác thực OTP mới", message);
                _logger.LogInformation("Gửi lại OTP thành công cho {Email}", email);
                StatusMessage = "Mã OTP mới đã được gửi đến email của bạn.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi lại OTP");
                StatusMessage = "Gửi lại OTP thất bại. Vui lòng thử lại.";
                TempData.Keep();
                return Page();
            }

            TempData.Keep();
            return RedirectToPage(new { userId, email, returnUrl });
        }

        private bool TryGetOtpData(string userId, out long unixTimestamp)
        {
            unixTimestamp = 0;

            if (string.IsNullOrEmpty(userId)) return false;

            var hasOtp = TempData.Peek($"OtpCode_{userId}") != null;
            var hasToken = TempData.Peek($"EmailConfirmToken_{userId}") != null;
            var timestampStr = TempData.Peek($"OtpCreationTime_{userId}")?.ToString();

            if (!hasOtp || !hasToken || string.IsNullOrEmpty(timestampStr)) return false;

            return long.TryParse(timestampStr, out unixTimestamp);
        }

        private bool IsOtpExpired(long unixTimestamp)
        {
            var otpTime = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
            return (DateTimeOffset.UtcNow - otpTime).TotalMinutes > OtpExpiryMinutes;
        }

        private void ClearTempData(string userId)
        {
            TempData.Remove($"OtpCode_{userId}");
            TempData.Remove($"EmailConfirmToken_{userId}");
            TempData.Remove($"OtpCreationTime_{userId}");
            TempData.Remove("UserIdForOtp");
        }

        private async Task LogAndDeleteUnconfirmedUser(string userId, string email, string reason)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && !await _userManager.IsEmailConfirmedAsync(user))
            {
                _logger.LogWarning("Xóa user chưa xác nhận. userId: {UserId}, lý do: {Reason}", userId, reason);
                await _userManager.DeleteAsync(user);
                ClearTempData(userId);
            }
        }
    }
}
