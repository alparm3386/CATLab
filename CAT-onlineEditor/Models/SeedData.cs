using CATWeb.Data;
using Microsoft.EntityFrameworkCore;

namespace CATWeb.Models
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new CATWebContext(
                serviceProvider.GetRequiredService<
                    DbContextOptions<CATWebContext>>()))
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
