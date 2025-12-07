using Microsoft.AspNetCore.Mvc;

namespace PatchTrackr.Web.Controllers;

public class RemoteValidationsController(IUsersService _usersService) : Controller
{
    [AcceptVerbs("GET", "POST")]
    public async Task<IActionResult> ValidateUserName(string userName)
    {
        string userIdStr = Request.Query["UserId"].ToString();
        _ = Guid.TryParse(userIdStr, out Guid userId);

        var existingUser = await _usersService.GetUserByUserNameAsync(userName);

        if (existingUser != null && string.Equals(existingUser.UserName, userName, StringComparison.OrdinalIgnoreCase) && existingUser.UserId != userId)
        {
            return Json($"User Name '{userName}' is already taken.");
        }

        return Json(true);
    }
}
