using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TravelWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AccountController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Bắt đầu đăng nhập với provider (Facebook, GitHub, Google...)
        [HttpGet]
        public IActionResult ExternalLogin(string provider, string returnUrl = "/")
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        // Callback sau khi đăng nhập từ Facebook, GitHub...
        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = "/")
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return RedirectToAction(nameof(Login));

            var result = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider, info.ProviderKey, isPersistent: false);

            if (result.Succeeded)
            {
                // Nếu đã đăng nhập trước đó
                var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                var claims = await _userManager.GetClaimsAsync(user);
                if (!claims.Any(c => c.Type == "http://schemas.microsoft.com/identity/claims/identityprovider"))
                {
                    await _userManager.AddClaimAsync(user, new Claim("http://schemas.microsoft.com/identity/claims/identityprovider", info.LoginProvider));
                }
                return LocalRedirect(returnUrl);
            }

            // Tạo user mới nếu lần đầu
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
                return RedirectToAction(nameof(Login));

            var newUser = new IdentityUser { UserName = email, Email = email };
            var createResult = await _userManager.CreateAsync(newUser);

            if (createResult.Succeeded)
            {
                await _userManager.AddLoginAsync(newUser, info);
                await _userManager.AddClaimAsync(newUser, new Claim("http://schemas.microsoft.com/identity/claims/identityprovider", info.LoginProvider));
                await _signInManager.SignInAsync(newUser, isPersistent: false);
                return LocalRedirect(returnUrl);
            }

            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View("Login");
        }

        // POST: /Account/Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        // Đăng xuất riêng GitHub (nếu có)
        public async Task<IActionResult> LogoutGitHub()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            return Redirect("https://github.com/logout");
        }

        // Đăng xuất riêng Facebook
        public async Task<IActionResult> LogoutFacebook()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            // Redirect đến Facebook logout (chỉ giả lập vì không logout thật từ client)
            return Redirect("https://www.facebook.com/logout.php");
        }
    }
}
