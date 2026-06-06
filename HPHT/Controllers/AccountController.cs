using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public AccountController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(
    string email,
    string password)
    {
        var result =
            await _signInManager.PasswordSignInAsync(
                email,
                password,
                false,
                false);

        if (result.Succeeded)
        {
            var user =
                await _userManager.FindByEmailAsync(email);

            var roles =
                await _userManager.GetRolesAsync(user);

            if (roles.Contains("Admin"))
            {
                return RedirectToAction(
                    "Index",
                    "Home");
            }

            if (roles.Contains("User"))
            {
                return RedirectToAction(
                    "Index",
                    "Issues");
            }
        }

        ViewBag.Error = "Invalid Login";

        return View();
    }

    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        return RedirectToAction(
            "Login");
    }
}