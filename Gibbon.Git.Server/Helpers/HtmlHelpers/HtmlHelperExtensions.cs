using System.Linq.Expressions;

using Gibbon.Git.Server.Models;

using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Gibbon.Git.Server.Helpers.HtmlHelpers;

public static class HtmlHelperExtensions
{
    public static IHtmlContent RequiredLabelFor<TModel, TValue>(
        this IHtmlHelper<TModel> htmlHelper,
        Expression<Func<TModel, TValue>> expression,
        object htmlAttributes = null)
    {
        var baseAttributes = new { @class = "required-label" };

        var mergedAttributes = MergeHtmlAttributes(baseAttributes, htmlAttributes);

        return htmlHelper.LabelFor(expression, mergedAttributes);
    }

    private static IDictionary<string, object> MergeHtmlAttributes(object baseAttributes, object additionalAttributes)
    {
        var attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(baseAttributes)
                         ?? new Dictionary<string, object>();

        if (additionalAttributes != null)
        {
            var additionalAttr = HtmlHelper.AnonymousObjectToHtmlAttributes(additionalAttributes);
            foreach (var attr in additionalAttr)
            {
                if (attributes.ContainsKey(attr.Key))
                {
                    if (attr.Key.Equals("class", StringComparison.OrdinalIgnoreCase))
                    {
                        attributes[attr.Key] = $"{attributes[attr.Key]} {attr.Value}";
                    }
                    else
                    {
                        attributes[attr.Key] = attr.Value;
                    }
                }
                else
                {
                    attributes.Add(attr.Key, attr.Value);
                }
            }
        }

        return attributes;
    }

    public static string IsActive(this IHtmlHelper html, string action, string controller = null)
    {
        var routeData = html.ViewContext.RouteData.Values;

        var currentAction = routeData["action"]?.ToString();
        var currentController = routeData["controller"]?.ToString();

        if (string.IsNullOrEmpty(controller))
        {
            return string.Equals(currentAction, action, StringComparison.OrdinalIgnoreCase) ? "active" : "";
        }

        return string.Equals(currentAction, action, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(currentController, controller, StringComparison.OrdinalIgnoreCase) ? "active" : "";
    }

    public static IHtmlContent SummaryMessage(this IHtmlHelper html, string key, string message, string type = "success")
    {
        var tempDataValue = html.ViewContext.TempData[key];
        if (tempDataValue != null && (bool)tempDataValue)
        {
            var divTag = new TagBuilder("div");
            divTag.AddCssClass($"summary-{type}");

            var pTag = new TagBuilder("p");
            pTag.InnerHtml.Append(message);

            divTag.InnerHtml.AppendHtml(pTag);

            return divTag;
        }

        return HtmlString.Empty;
    }
}
