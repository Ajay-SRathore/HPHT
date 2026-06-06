using HPHT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
public class UserManagementController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    public UserManagementController(
     UserManager<IdentityUser> userManager,
     RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> EditRole(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
            return NotFound();

        var currentRoles =
            await _userManager.GetRolesAsync(user);

        ViewBag.Roles =
            _roleManager.Roles.ToList();

        ViewBag.CurrentRole =
            currentRoles.FirstOrDefault();

        return View(user);
    }
    [HttpPost]
    public async Task<IActionResult> EditRole(
    string id,
    string role)
    {
        var user =
            await _userManager.FindByIdAsync(id);

        if (user == null)
            return NotFound();

        var roles =
            await _userManager.GetRolesAsync(user);

        await _userManager.RemoveFromRolesAsync(
            user,
            roles);

        await _userManager.AddToRoleAsync(
            user,
            role);

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Index()
    {
        var model = new List<UserVM>();

        foreach (var user in _userManager.Users)
        {
            var roles =
                await _userManager.GetRolesAsync(user);

            model.Add(new UserVM
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                Role = string.Join(",", roles)
            });
        }

        return View(model);
    }
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        string email,
        string password,
        string role)
    {
        var user = new IdentityUser
        {
            UserName = email,
            Email = email
        };

        var result =
            await _userManager.CreateAsync(
                user,
                password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(
                user,
                role);

            return RedirectToAction("Index");
        }

        return View();
    }
    //public IActionResult Index()
    //{
    //    return View(_userManager.Users.ToList());
    //}
}