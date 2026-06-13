using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SwapShelf.Tests.Helpers
{
    /// <summary>
    /// Sets up a fake ClaimsPrincipal on a controller so that GetUserId() and role checks work
    /// without running the full ASP.NET Core authentication pipeline.
    /// </summary>
    internal static class ControllerTestHelper
    {
        internal static void SetUser(ControllerBase controller, int userId, string role = "User")
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId.ToString()),
                new(ClaimTypes.Role, role)
            };
            var identity  = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }
    }
}
