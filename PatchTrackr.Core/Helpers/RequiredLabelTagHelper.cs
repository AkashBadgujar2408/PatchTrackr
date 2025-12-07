using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace PatchTrackr.Core.Helpers;

[HtmlTargetElement("label", Attributes = ForAttributeName)]
public class RequiredLabelTagHelper : TagHelper
{
    private const string ForAttributeName = "asp-for";

    [HtmlAttributeName(ForAttributeName)]
    public required ModelExpression For { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        // 1️ Add the classes if they don't exist yet
        string existingClasses = output.Attributes.ContainsName("class")
            ? output.Attributes["class"]!.Value?.ToString() ?? ""
            : "";

        var requiredClasses = new[] { "col-form-label", "fw-bold", /*, "w-100", "text-start", "text-md-end"*/ };

        foreach (var cssClass in requiredClasses)
        {
            if (!existingClasses.Contains(cssClass))
                existingClasses += " " + cssClass;
        }

        output.Attributes.SetAttribute("class", existingClasses.Trim());

        // 2️ Append required asterisk or colon
        if (For.Metadata.IsRequired)
        {
            output.Content.AppendHtml(" : <span class='text-danger'>*</span>");
        }
        else
        {
            output.Content.AppendHtml(" :");
        }
    }
}
