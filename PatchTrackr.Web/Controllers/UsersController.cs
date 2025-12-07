using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatchTrackr.Core.Models;

namespace PatchTrackr.Web.Controllers;

[Authorize]
public class UsersController(IUsersService _usersService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Manage()
    {
        List<MUser> users = await _usersService.GetAllUsersAsync();
        return View(users);
    }

    [HttpGet]
    public async Task<IActionResult> GetUserInfo(string userName)
    {
        MUser? user = await _usersService.GetUserByUserNameAsync(userName);
        return Json(user);
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers() {
        var users = await _usersService.GetAllUsersAsync();
        return Json(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveUserInfo(UserDTO user)
    {
        bool isUserInfoSaved = await _usersService.AddUpdateUserAsync(user);

        if (isUserInfoSaved)
        {
            return Json(new { success = true, message = "User details saved successfully." });
        }

        return Json(new { message = "Failed to save user details." });
    }
}
