using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using TravelWeb.Models; 
namespace TravelWeb.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender; 

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required(ErrorMessage = "Email không được để trống.")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required(ErrorMessage = "Mật khẩu không được để trống.")]
            [StringLength(100, ErrorMessage = "{0} phải dài ít nhất {2} và tối đa {1} ký tự.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Xác nhận mật khẩu")]
            [Compare("Password", ErrorMessage = "Mật khẩu và mật khẩu xác nhận không khớp.")]
            public string ConfirmPassword { get; set; }
            

        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!_roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();
            }

        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(Input.Email);
                if (existingUser != null)
                {
                    // Nếu đã tồn tại và chưa xác nhận email, thì gửi lại OTP
                    if (!await _userManager.IsEmailConfirmedAsync(existingUser))
                    {
                        var userId = await _userManager.GetUserIdAsync(existingUser);
                        var otpCode = new Random().Next(100000, 999999).ToString();
                        var emailConfirmToken = await _userManager.GenerateEmailConfirmationTokenAsync(existingUser);
                        var otpCreationTime = DateTimeOffset.UtcNow;

                        TempData[$"OtpCode_{userId}"] = otpCode;
                        TempData[$"EmailConfirmToken_{userId}"] = emailConfirmToken;
                        TempData[$"OtpCreationTime_{userId}"] = otpCreationTime.ToUnixTimeSeconds().ToString();
                        TempData["UserIdForOtp"] = userId;

                        await _emailSender.SendEmailAsync(
                            Input.Email,
                            "Mã OTP xác nhận tài khoản của bạn",
                            $"Mã OTP của bạn để xác nhận tài khoản là: <strong>{otpCode}</strong>. Mã này sẽ hết hạn trong 1 phút. Vui lòng nhập mã này vào trang xác nhận."
                        );

                        _logger.LogInformation($"OTP (resend): {otpCode} gửi đến email {Input.Email} lúc {otpCreationTime.ToLocalTime()}.");

                        return RedirectToPage("ConfirmOtp", new { userId = userId, email = Input.Email, returnUrl = returnUrl });
                    }

                    // Nếu đã xác nhận email thì không cho đăng ký lại
                    ModelState.AddModelError(string.Empty, "Tài khoản với email này đã tồn tại và đã được xác nhận.");
                    return Page();
                }

                // Nếu chưa có, tiến hành tạo user
                var user = CreateUser();
                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    await _userManager.AddToRoleAsync(user, SD.Role_Customer);
                    var userId = await _userManager.GetUserIdAsync(user);
                    var otpCode = new Random().Next(100000, 999999).ToString();
                    var emailConfirmToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var otpCreationTime = DateTimeOffset.UtcNow;

                    TempData[$"OtpCode_{userId}"] = otpCode;
                    TempData[$"EmailConfirmToken_{userId}"] = emailConfirmToken;
                    TempData[$"OtpCreationTime_{userId}"] = otpCreationTime.ToUnixTimeSeconds().ToString();
                    TempData["UserIdForOtp"] = userId;

                    await _emailSender.SendEmailAsync(
                        Input.Email,
                        "Mã OTP xác nhận tài khoản của bạn",
                        $"Mã OTP của bạn để xác nhận tài khoản là: <strong>{otpCode}</strong>. Mã này sẽ hết hạn trong 1 phút. Vui lòng nhập mã này vào trang xác nhận."
                    );

                    _logger.LogInformation($"OTP: {otpCode} gửi đến email {Input.Email} lúc {otpCreationTime.ToLocalTime()}.");

                    return RedirectToPage("ConfirmOtp", new { userId = userId, email = Input.Email, returnUrl = returnUrl });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }


        private IdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<IdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Không thể tạo một thể hiện của '{nameof(IdentityUser)}'. " +
                    $"Đảm bảo rằng '{nameof(IdentityUser)}' không phải là một lớp trừu tượng và có hàm tạo không tham số, hoặc thay vào đó " +
                    $"ghi đè trang đăng ký trong /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("Giao diện người dùng mặc định yêu cầu một kho người dùng có hỗ trợ email.");
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}
