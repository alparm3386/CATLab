using Microsoft.EntityFrameworkCore;

namespace CAT.Middleware
{
    public class TransactionMiddleware
    {
        private readonly RequestDelegate _next;

        public TransactionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext, DbContext dbContext)
        {
            using var transaction = dbContext.Database.BeginTransaction();
            try
            {
                await _next(httpContext);
                //await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }

}
