using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public class AccountController : Controller
{
    private const string DemoUser = "admin@example.com";
    private const string DemoPass = "Password!123";

    [HttpGet]
    public IActionResult Login(string? returnUrl = null) => View(model: returnUrl);

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
    {
        if (email == DemoUser && password == DemoPass)
        {
            var claims = new[] { new Claim(ClaimTypes.Name, email) };
            var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
            return Redirect(returnUrl ?? "/");
        }

        ModelState.AddModelError(string.Empty, "Invalid credentials");
        return View(model: returnUrl);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
