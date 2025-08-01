using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Routing;
using bike.Services;
using System.Security.Claims;

namespace bike.Helpers
{
    public static class PermissionHelper
    {
        public static bool HasPermission(this IHtmlHelper html, string permissionName)
        {
            var httpContext = html.ViewContext.HttpContext;
            var user = httpContext.User;

            if (!user.Identity.IsAuthenticated) return false;

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return false;

            var permissionService = httpContext.RequestServices.GetService<IPermissionService>();
            if (permissionService == null) return false;

            return permissionService.HasPermissionAsync(userId, permissionName).Result;
        }

        public static IHtmlContent PermissionButton(this IHtmlHelper html, string permissionName, string action, string controller, object routeValues, string buttonText, string buttonClass = "btn btn-primary")
        {
            if (!html.HasPermission(permissionName))
                return new HtmlString("");

            var urlHelperFactory = html.ViewContext.HttpContext.RequestServices.GetService<IUrlHelperFactory>();
            var urlHelper = urlHelperFactory?.GetUrlHelper(html.ViewContext);
            var url = urlHelper?.Action(action, controller, routeValues) ?? "#";

            return new HtmlString($"<a href=\"{url}\" class=\"{buttonClass}\">{buttonText}</a>");
        }

        public static IHtmlContent PermissionLink(this IHtmlHelper html, string permissionName, string action, string controller, object routeValues, string linkText, string cssClass = "")
        {
            if (!html.HasPermission(permissionName))
                return new HtmlString("");

            var urlHelperFactory = html.ViewContext.HttpContext.RequestServices.GetService<IUrlHelperFactory>();
            var urlHelper = urlHelperFactory?.GetUrlHelper(html.ViewContext);
            var url = urlHelper?.Action(action, controller, routeValues) ?? "#";

            return new HtmlString($"<a href=\"{url}\" class=\"{cssClass}\">{linkText}</a>");
        }
    }
} 