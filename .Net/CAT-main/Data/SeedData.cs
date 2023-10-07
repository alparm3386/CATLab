using CAT.Models.Entities.Main;
using Microsoft.EntityFrameworkCore;
using System;

namespace CAT.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            //using (var context = new MainDbContext(
            //serviceProvider.GetRequiredService<
            //    DbContextOptions<MainDbContext>>()))
            //    if (!context.Specialities.Any())
            //    {
            //        context.Specialities.AddRange(
            //            new Speciality { Id = 1, Name = "General" },
            //            new Speciality { Id = 2, Name = "Marketing" },
            //            new Speciality { Id = 3, Name = "Technical" });

            //        context.SaveChanges();
            //    }
        }
    }
}

