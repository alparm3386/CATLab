using CAT_web.Data;
using Microsoft.EntityFrameworkCore;

namespace CAT_web.Models
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new CAT_webContext(
                serviceProvider.GetRequiredService<
                    DbContextOptions<CAT_webContext>>()))
            {
                // Look for any movies.
                if (context.Job.Any())
                {
                    return;   // DB has been seeded
                }

                context.Job.AddRange(
                    //new Job
                    //{
                    //    Title = "When Harry Met Sally",
                    //    ReleaseDate = DateTime.Parse("1989-2-12"),
                    //    Genre = "Romantic Comedy",
                    //    Price = 7.99M
                    //}
                );
                context.SaveChanges();
            }
        }
    }
}
