using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace CAT_web.Data
{
    public class CATDbCommandInterceptor : DbCommandInterceptor
    {
        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result)
        {
            // Log to console
            Console.WriteLine($"Executing Command: {command.CommandText}");

            return base.ReaderExecuting(command, eventData, result);
        }
    }

}
