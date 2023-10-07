using System.Net;

namespace CAT.Middleware
{
    public class RoleBasedRedirectMiddleware
    {
        private readonly RequestDelegate _next;

        public RoleBasedRedirectMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            if (context.Response.StatusCode == (int)HttpStatusCode.Forbidden) // 403 Forbidden status
            {
                // Check user's role and redirect accordingly
                if (context.User.IsInRole("Admin"))
                {
                    context.Response.Redirect("/BackOffice/Monitoring");
                }
                else if (context.User.IsInRole("Client"))
                {
                    context.Response.Redirect("/UserArea/Home/Index");
                }
                else if (context.User.IsInRole("Linguist"))
                {
                    context.Response.Redirect("/UserArea/Home/Index");
                }
                // Add more role checks and redirections as needed
            }
        }
    }
}
