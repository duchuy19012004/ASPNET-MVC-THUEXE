using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using bike.Services;

namespace bike.Attributes
{
    public class PermissionAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly string _permissionName;

        public PermissionAuthorizeAttribute(string permissionName)
        {
            _permissionName = permissionName;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity.IsAuthenticated)
            {
                context.Result = new RedirectToActionResult("Login", "Account",
                    new { returnUrl = context.HttpContext.Request.Path });
                return;
            }

            var permissionService = context.HttpContext.RequestServices.GetService<IPermissionService>();
            if (permissionService == null)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Shared", null);
                return;
            }

            // Kiểm tra quyền bất đồng bộ
            var hasPermission = permissionService.HasPermissionAsync(user, _permissionName).Result;
            if (!hasPermission)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Shared", null);
                return;
            }
        }
    }
} 