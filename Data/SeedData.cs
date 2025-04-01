using Email_Worker_Service.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Email_Worker_Service.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            // Check if there are any users
            if (context.Users.Any())
            {
                return;   // DB has been seeded
            }

            context.Users.AddRange(
                new User
                {
                    Name = "John Doe",
                    Email = "ayomidotun.odebode@elizadeuniversity.edu.ng",
                    IsEmailSent = false
                },
                new User
                {
                    Name = "Jane Smith",
                    Email = "adedaramayowa6@gmail.com",
                    IsEmailSent = false
                },
                new User
                {
                    Name = "Bob Johnson",
                    Email = "vibezr35@gmail.com",
                    IsEmailSent = false
                },
                new User
                {
                    Name = "Jane Collins",
                    Email = "ayomidotun.odebode1@gmail.com",
                    IsEmailSent = false
                },
                 new User
                 {
                     Name = "Jane CMay",
                     Email = "adedarajesse@gmail.com",
                     IsEmailSent = false
                 }
            );

            await context.SaveChangesAsync();
        }
    }
} 