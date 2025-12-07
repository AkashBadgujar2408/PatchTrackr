using FluentResults;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace PatchTrackr.Web.Controllers;

public class HomeController(ICommonService _commonService, IUsersService _usersService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Login(string? returnUrl)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDTO model)
    {
        string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        string browser = Request.Headers.UserAgent.ToString();

        Result<MUser?> loginResult = await _usersService.LoginUserAsync(model);

        if (loginResult.IsSuccess)
        {
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Dashboard");
        }

        TempData.SetErrorNotificationAlert(loginResult.Errors.FirstOrDefault()?.Message ?? "An error occurred during login.");
        ViewBag.ReturnUrl = model.ReturnUrl;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Remove("FinYear");
        return RedirectToAction("Login", "Home");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var exceptionHandlerPathFeature =
        HttpContext.Features.Get<IExceptionHandlerPathFeature>();

        if (exceptionHandlerPathFeature == null)
            return NotFound(); // Prevents direct access

        // ✅ Check if it's an AJAX request
        if (Request.Headers.XRequestedWith == "XMLHttpRequest")
        {
            Response.StatusCode = 500; // Internal Server Error
            return Json(new
            {
                message = exceptionHandlerPathFeature.Error.Message
                + ($"Unhandled exception at path: {exceptionHandlerPathFeature.Path}")
            });
        }

        Log.Error(exceptionHandlerPathFeature.Error,
                      "Unhandled exception at path: {Path}",
                      exceptionHandlerPathFeature.Path);

        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error404()
    {
        return View();
    }
}
