
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using PatchTrackr.Core.Helpers;

namespace PatchTrackr.Core.Attributes;

public class ValidateModelAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var message = context.ModelState.Values
                .Where(v => v.Errors.Count > 0)
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "Validation failed.";

            bool isAjax = context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (isAjax)
            {
                // Return JSON for AJAX calls
                context.Result = new JsonResult(new { message });
            }
            else
            {
                // Redirect back for normal requests
                var referer = context.HttpContext.Request.Headers["Referer"].ToString();
                var tempDataFactory = context.HttpContext.RequestServices.GetRequiredService<ITempDataDictionaryFactory>();
                var tempData = tempDataFactory.GetTempData(context.HttpContext);

                tempData.SetErrorNotificationAlert(message);

                if (!string.IsNullOrEmpty(referer))
                {
                    context.Result = new RedirectResult(referer);
                }
                else
                {
                    // Fallback
                    context.Result = new RedirectToActionResult("Index", "Dashboard", null);
                }
            }
        }
    }
}