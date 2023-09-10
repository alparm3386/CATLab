namespace CAT.Middleware
{
    public class AuthDebugMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthDebugMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            var user = httpContext.User;

            // Check if user is authenticated
            if (user.Identity.IsAuthenticated)
            {
                Console.WriteLine("User is authenticated.");

                // Print out all claims for the user
                foreach (var claim in user.Claims)
                {
                    Console.WriteLine($"Claim Type: {claim.Type}, Claim Value: {claim.Value}");
                }
            }
            else
            {
                Console.WriteLine("User is not authenticated.");
            }

            // Call the next middleware in the pipeline
            await _next(httpContext);
        }
    }
}
