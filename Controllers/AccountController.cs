using System.Security.Claims;
using MediTrack.Web.Models.ViewModels;
using MediTrack.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediTrack.Web.Controllers;

/// <summary>
/// Coordinates the login flow: validates input, calls AuthService, signs the user in,
/// and picks the view. No database or password logic lives here.
/// </summary>
public class AccountController : Controller
{
    private readonly IAuthService _auth;

    public AccountController(IAuthService auth)
    {
        _auth = auth;
    }

    // GET /Account/Login
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    // POST /Account/Login
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _auth.ValidateCredentialsAsync(model.Email, model.Password);

        switch (result.Status)
        {
            case LoginStatus.InvalidCredentials:
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);

            case LoginStatus.Inactive:
                ModelState.AddModelError(string.Empty, "This account is inactive. Please contact the administrator.");
                return View(model);

            case LoginStatus.LockedOut:
                var minutes = result.LockoutEndUtc.HasValue
                    ? Math.Max(1, (int)Math.Ceiling((result.LockoutEndUtc.Value - DateTime.UtcNow).TotalMinutes))
                    : (int)AuthService.LockoutDuration.TotalMinutes;
                ModelState.AddModelError(string.Empty,
                    $"Too many failed attempts. Try again in {minutes} minute(s).");
                return View(model);
        }

        var user = result.User!;
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id!),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var properties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(7) : null
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            properties);

        // Only redirect to local URLs (prevents open-redirect attacks).
        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return LocalRedirect(model.ReturnUrl);

        return RedirectToAction("Index", "Home");
    }

    // POST /Account/Logout
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }
}
