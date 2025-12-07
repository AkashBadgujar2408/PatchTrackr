using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace PatchTrackr.Core.Helpers;

public static class Utility
{
    /// <summary>
    /// Extension method for <see cref="IHtmlHelper"/>
    /// </summary>
    /// <param name="html"><see cref="IHtmlHelper"/> usable in razor views</param>
    /// <param name="obj">Any C# <see cref="object"/></param>
    /// <param name="convertToCamelCase">If set to <see langword="true"/> then property names are converted to camel case</param>
    /// <returns>Returns raw html json to be directly used with frontend (e.g. datatable)</returns>
    public static IHtmlContent GetRawJson(this IHtmlHelper html, object obj, bool convertToCamelCase = false)
    {
        var settings = new JsonSerializerSettings();
        if (convertToCamelCase)
        {
            settings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
        }

        string json = JsonConvert.SerializeObject(obj, settings);

        return html.Raw(json);
    }
}