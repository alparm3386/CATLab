using Microsoft.EntityFrameworkCore;
using System.Transactions;

namespace CAT.Middleware
{
    public class TransactionMiddleware
    {
        private readonly RequestDelegate _next;

        public TransactionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        //public async Task InvokeAsync(HttpContext httpContext, DbContext dbContext)
        //{
        //    using var transaction = dbContext.Database.BeginTransaction();
        //    try
        //    {
        //        await _next(httpContext);
        //        //await dbContext.SaveChangesAsync();
        //        await transaction.CommitAsync();
        //    }
        //    catch
        //    {
        //        await transaction.RollbackAsync();
        //        throw;
        //    }
        //}

        public async Task InvokeAsync(HttpContext httpContext)
        {
            var transactionOptions = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadUncommitted
            };

            using var scope = new TransactionScope(
                TransactionScopeOption.Required,
                transactionOptions,
                TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                await _next(httpContext);
                scope.Complete();
            }
            catch
            {
                // Transaction will be automatically rolled back 
                // if scope.Complete() is not called.
                throw;
            }
        }
    }
}
