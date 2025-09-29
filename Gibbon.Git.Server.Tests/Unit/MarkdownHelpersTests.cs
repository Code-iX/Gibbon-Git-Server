using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;

using Gibbon.Git.Server.Helpers;

using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gibbon.Git.Server.Tests.Unit;

[TestClass]
public class MarkdownHelpersTests
{
    [TestMethod]
    public void MarkdownToHtml_ShouldProcessTextLinks()
    {
        // Arrange
        var markdownText = "[Link to file](README.md)";
        
        // Act - Just verify it processes without errors and produces some output
        var result = ProcessMarkdown(markdownText);

        // Assert
        var html = result.ToString();
        Assert.IsTrue(html.Contains("<a"), "Should generate anchor tag for text link");
        Assert.IsTrue(html.Contains("Link to file"), "Should preserve link text");
    }

    [TestMethod]
    public void MarkdownToHtml_ShouldProcessImageLinks()
    {
        // Arrange
        var markdownText = "![Alt text](image.png)";
        
        // Act
        var result = ProcessMarkdown(markdownText);

        // Assert
        var html = result.ToString();
        Assert.IsTrue(html.Contains("<img"), "Should generate img tag for image link");
        Assert.IsTrue(html.Contains("Alt text"), "Should preserve alt text");
    }

    [TestMethod]
    public void MarkdownToHtml_ShouldProcessBothImageAndTextLinks()
    {
        // Arrange
        var markdownText = "![Image](image.png) and [text link](file.txt)";
        
        // Act
        var result = ProcessMarkdown(markdownText);

        // Assert
        var html = result.ToString();
        Assert.IsTrue(html.Contains("<img"), "Should generate img tag");
        Assert.IsTrue(html.Contains("<a"), "Should generate anchor tag");
        Assert.IsTrue(html.Contains("text link"), "Should preserve link text");
    }

    [TestMethod]
    public void MarkdownToHtml_ShouldHandleAbsoluteUrls()
    {
        // Arrange
        var markdownText = "[External link](https://example.com)";
        
        // Act
        var result = ProcessMarkdown(markdownText);

        // Assert
        var html = result.ToString();
        Assert.IsTrue(html.Contains("https://example.com"), "Should preserve absolute URLs");
    }

    [TestMethod]
    public void MarkdownToHtml_ShouldReturnEmptyForNullOrEmptyInput()
    {
        // Arrange & Act & Assert
        var result1 = ProcessMarkdown(null);
        var result2 = ProcessMarkdown("");

        Assert.AreEqual(HtmlString.Empty, result1);
        Assert.AreEqual(HtmlString.Empty, result2);
    }

    private static IHtmlContent ProcessMarkdown(string markdownText)
    {
        // Create minimal test helpers
        var routeData = new RouteData();
        routeData.Values["name"] = "test-repo";
        routeData.Values["version"] = "main";
        
        var viewContext = new ViewContext
        {
            RouteData = routeData
        };
        
        var htmlHelper = new MinimalHtmlHelper(viewContext);
        var urlHelper = new MinimalUrlHelper();
        
        return htmlHelper.MarkdownToHtml(urlHelper, markdownText);
    }

    private class MinimalHtmlHelper : IHtmlHelper
    {
        public MinimalHtmlHelper(ViewContext viewContext)
        {
            ViewContext = viewContext;
        }

        public ViewContext ViewContext { get; }

        // Only implement what we absolutely need - everything else throws NotImplementedException
        public Html5DateRenderingMode Html5DateRenderingMode { get; set; }
        public string IdAttributeDotReplacement => throw new NotImplementedException();
        public Microsoft.AspNetCore.Mvc.ModelBinding.IModelMetadataProvider MetadataProvider => throw new NotImplementedException();
        public ITempDataDictionary TempData => throw new NotImplementedException();
        public UrlEncoder UrlEncoder => throw new NotImplementedException();
        public dynamic ViewBag => throw new NotImplementedException();
        public ViewDataDictionary ViewData => throw new NotImplementedException();
        
        public IHtmlContent ActionLink(string linkText, string actionName, string controllerName, string protocol, string hostname, string fragment, object routeValues, object htmlAttributes) => throw new NotImplementedException();
        public IHtmlContent AntiForgeryToken() => throw new NotImplementedException();
        public Microsoft.AspNetCore.Mvc.Rendering.MvcForm BeginForm(string actionName, string controllerName, object routeValues, FormMethod method, bool? antiforgery, object htmlAttributes) => throw new NotImplementedException();
        public Microsoft.AspNetCore.Mvc.Rendering.MvcForm BeginRouteForm(string routeName, object routeValues, FormMethod method, bool? antiforgery, object htmlAttributes) => throw new NotImplementedException();
        public IHtmlContent CheckBox(string expression, bool? isChecked, object htmlAttributes) => throw new NotImplementedException();
        public IHtmlContent Display(string expression, string templateName, string htmlFieldName, object additionalViewData) => throw new NotImplementedException();
        public string DisplayName(string expression) => throw new NotImplementedException();
        public string DisplayText(string expression) => throw new NotImplementedException();
        public IHtmlContent DropDownList(string expression, IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> selectList, string optionLabel, object htmlAttributes) => throw new NotImplementedException();
        public IHtmlContent Editor(string expression, string templateName, string htmlFieldName, object additionalViewData) => throw new NotImplementedException();
        public string Encode(object value) => throw new NotImplementedException();
        public string Encode(string value) => throw new NotImplementedException();
        public void EndForm() => throw new NotImplementedException();
        public string FormatValue(object value, string format) => throw new NotImplementedException();
        public string GenerateIdFromName(string fullName) => throw new NotImplementedException();
        public IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> GetEnumSelectList<TEnum>() where TEnum : struct => throw new NotImplementedException();
        public IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> GetEnumSelectList(Type enumType) => throw new NotImplementedException();
        public IHtmlContent Hidden(string expression, object value, object htmlAttributes) => throw new NotImplementedException();
        public string Id(string expression) => throw new NotImplementedException();
        public IHtmlContent Label(string expression, string labelText, object htmlAttributes) => throw new NotImplementedException();
        public IHtmlContent ListBox(string expression, IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> selectList, object htmlAttributes) => throw new NotImplementedException();
        public string Name(string expression) => throw new NotImplementedException();
        public System.Threading.Tasks.Task<IHtmlContent> PartialAsync(string partialViewName, object model, ViewDataDictionary viewData) => throw new NotImplementedException();
        public IHtmlContent Password(string expression, object value, object htmlAttributes) => throw new NotImplementedException();
        public IHtmlContent RadioButton(string expression, object value, bool? isChecked, object htmlAttributes) => throw new NotImplementedException();
        public IHtmlContent Raw(object value) => throw new NotImplementedException();
        public IHtmlContent Raw(string value) => throw new NotImplementedException();
        public System.Threading.Tasks.Task RenderPartialAsync(string partialViewName, object model, ViewDataDictionary viewData) => throw new NotImplementedException();
        public IHtmlContent RouteLink(string linkText, string routeName, string protocol, string hostName, string fragment, object routeValues, object htmlAttributes) => throw new NotImplementedException();
        public IHtmlContent TextArea(string expression, string value, int rows, int columns, object htmlAttributes) => throw new NotImplementedException();
        public IHtmlContent TextBox(string expression, object value, string format, object htmlAttributes) => throw new NotImplementedException();
        public IHtmlContent ValidationMessage(string expression, string message, object htmlAttributes, string tag) => throw new NotImplementedException();
        public IHtmlContent ValidationSummary(bool excludePropertyErrors, string message, object htmlAttributes, string tag) => throw new NotImplementedException();
        public string Value(string expression, string format) => throw new NotImplementedException();
    }

    private class MinimalUrlHelper : IUrlHelper
    {
        public ActionContext ActionContext => throw new NotImplementedException();

        public string Action(UrlActionContext actionContext)
        {
            var routeValues = new RouteValueDictionary(actionContext.Values);
            return $"/{routeValues["name"]}/{actionContext.Action}/{routeValues["version"]}/{routeValues["path"]}";
        }

        public string Action(string action, string controller, object? values, string? protocol, string? host, string? fragment)
        {
            var routeValues = new RouteValueDictionary(values);
            var name = routeValues["name"];
            var version = routeValues["version"];
            var path = routeValues["path"];
            
            return $"/{name}/{controller}/{version}/{path}";
        }

        public string? Content(string? contentPath) => throw new NotImplementedException();
        public bool IsLocalUrl(string? url) => throw new NotImplementedException();
        public string? Link(string? routeName, object? values) => throw new NotImplementedException();
        public string RouteUrl(UrlRouteContext routeContext) => throw new NotImplementedException();
    }
}