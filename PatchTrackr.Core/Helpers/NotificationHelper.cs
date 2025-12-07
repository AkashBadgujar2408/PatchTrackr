using Microsoft.AspNetCore.Mvc.ViewFeatures;
namespace PatchTrackr.Core.Helpers;

public static class NotificationHelper
{
    public static void SetErrorNotificationAlert(this ITempDataDictionary tempData, string message)
    {
        tempData["AlertType"] = "error";
        tempData["AlertMessage"] = message;
    }

    public static void SetSuccessNotificationAlert(this ITempDataDictionary tempData, string message)
    {
        tempData["AlertType"] = "success";
        tempData["AlertMessage"] = message;
    }

    public static void SetInfoNotificationAlert(this ITempDataDictionary tempData, string message)
    {
        tempData["AlertType"] = "info";
        tempData["AlertMessage"] = message;
    }

    public static void SetWarningNotificationAlert(this ITempDataDictionary tempData, string message)
    {
        tempData["AlertType"] = "warning";
        tempData["AlertMessage"] = message;
    }
}