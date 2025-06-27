using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace TravelWeb.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly IEmailSender _emailSender;

        public LoginModel(SignInManager<IdentityUser> signInManager,
                          UserManager<IdentityUser> userManager,
                          ILogger<LoginModel> logger,
                          IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user != null)
                {
                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {
                        // Tạo mã OTP
                        var otpCode = new Random().Next(100000, 999999).ToString();

                        // Lưu OTP vào User Token
                        await _userManager.RemoveAuthenticationTokenAsync(user, "Default", "OTP");
                        await _userManager.SetAuthenticationTokenAsync(user, "Default", "OTP", otpCode);

                        // Gửi OTP qua email
                        await _emailSender.SendEmailAsync(user.Email, "Xác nhận tài khoản - OTP",
                            $"Mã xác nhận của bạn là: <strong>{otpCode}</strong>. Mã này sẽ hết hạn sau 1 phút.");

                        // Lưu thời gian gửi OTP (Unix timestamp)
                        var otpUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                        // Hiển thị thông báo và redirect sau 10 giây
                        TempData["OtpMessage"] = "Tài khoản của bạn chưa được xác nhận. Đang chuyển đến trang xác nhận OTP trong 10 giây...";

                        Response.Headers.Add("Refresh", $"10;url={Url.Page("/Account/ConfirmOtp", new
                        {
                            area = "Identity",
                            userId = user.Id,
                            email = user.Email,
                            returnUrl,
                            otpTime = otpUnixTime
                        })}");

                        return Page();
                    }
                }


                // Đăng nhập nếu đã xác nhận email
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Đăng nhập không hợp lệ.");
                    return Page();
                }
            }

            return Page();
        }
    }
}
