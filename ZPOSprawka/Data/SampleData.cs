using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZPOSprawka.Models;

namespace ZPOSprawka.Data
{
    public class SampleData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            //wstrzyknięcie role i usermanageras
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            //lista ról
            string[] roles = new string[] { "Administrator", "Prowadzący", "Student" };

            //sprawdzenie czy role istnieją, i jeżeli nie to są dodawane
            foreach (string role in roles)
            {
                if (!roleManager.RoleExistsAsync(role).Result)
                {
                    IdentityRole irole = new IdentityRole
                    {
                        Name = role
                    };
                    await roleManager.CreateAsync(irole);
                }
            }

            //dane zseedowanego użytkownika
            var user = new ApplicationUser
            {
                Email = "admin@projekt.pl",
                UserName = "admin@projekt.pl",
                Name = "Jan",
                Surname = "Kowalski",
                EmailConfirmed = true
            };


            //sprawdzenie czy ten użytkownik istnieje, i jeżeli nie to stworzenie go
            //login - admin@projekt.pl
            //hasło - Admin123!
            if (!userManager.Users.Any(u => u.UserName == user.UserName))
            {
                await userManager.CreateAsync(user, "Admin123!");
                //nadanie mu ról
                user = userManager.FindByEmailAsync("admin@projekt.pl").Result;
                await userManager.AddToRoleAsync(user, "Administrator");
            }

            
        }
    }
}
