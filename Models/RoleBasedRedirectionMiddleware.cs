using System.Security.Claims;

namespace SmartHotelBooking.Models

{

    public class RoleBasedRedirectionMiddleware

    {

        private readonly RequestDelegate _next;

        public RoleBasedRedirectionMiddleware(RequestDelegate next)

        {

            _next = next;

        }

        public async Task InvokeAsync(HttpContext context)

        {

            if (context.User.Identity != null && context.User.Identity.IsAuthenticated)

            {

                var userRole = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                if (!string.IsNullOrEmpty(userRole))

                {

                    if (userRole == "Admin" && context.Request.Path == "/")

                    {

                        context.Response.Redirect("/Admin/AdminHome");

                        return;

                    }

                    else if (userRole == "Manager" && context.Request.Path == "/")

                    {

                        context.Response.Redirect("/Manager/ManagerHome");

                        return;

                    }

                    else if (userRole == "Member" && context.Request.Path == "/")

                    {

                        context.Response.Redirect("/Member/MemberHome");

                        return;

                    }

                }

            }

            await _next(context);

        }

    }

}

