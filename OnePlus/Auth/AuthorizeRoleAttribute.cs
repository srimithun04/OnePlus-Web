using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OnePlus.Models; // Your namespace for the UserRole enum
using System;
using System.Linq;

namespace OnePlus.Auth
{
    // This attribute can be applied to controller actions or entire controllers
    // Example: [AuthorizeRole(UserRole.Admin)]
    // Example: [AuthorizeRole(UserRole.Admin, UserRole.Client)]
    public class AuthorizeRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly UserRole[] _roles;

        public AuthorizeRoleAttribute(params UserRole[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Get the user's role from the session
            var userRoleString = context.HttpContext.Session.GetString("UserRole");

            // 1. Check if the user is logged in at all
            if (string.IsNullOrEmpty(userRoleString))
            {
                // If not logged in, redirect to the login page
                context.Result = new RedirectToActionResult("Login", "Uam", null);
                return;
            }

            // 2. Try to parse the role from the session string into our enum
            if (Enum.TryParse(userRoleString, out UserRole currentUserRole))
            {
                // 3. Check if the user's role is in the list of allowed roles
                if (_roles.Contains(currentUserRole))
                {
                    // Authorization successful, do nothing.
                    return;
                }
            }

            // 4. If any check fails, the user is not authorized. Redirect to Access Denied.
            context.Result = new RedirectToActionResult("AccessDenied", "Home", null);
        }
    }
}
