using LawPlatform.Entities.Models.Auth.Identity;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LawPlatform.DataAccess.Seeder
{
    public static class UserSeeder
    {
        public static async Task SeedAsync(UserManager<User> _userManager)
        {
            var usersCount = await _userManager.Users.CountAsync();
            if (usersCount <= 0)
            {
                var adminUser = new User()
                {
                    UserName = "admin",
                    Email = "admin@gmail.com",
                    PhoneNumber = "01555803091",
                    EmailConfirmed = true,
                };
                await _userManager.CreateAsync(adminUser, "P@ssw0rd123Pass");
                await _userManager.AddToRoleAsync(adminUser, "Admin");

                var lawyerUser = new User()
                {
                    UserName = "ahmedYoussefLawyer",
                    Email = "ahmedyoussef@gmail.com",
                    PhoneNumber = "01069391275",
                    EmailConfirmed = true,
                };
                await _userManager.CreateAsync(lawyerUser, "P@ssw0rd123Pass");
                await _userManager.AddToRoleAsync(lawyerUser, "Lawyer");
                
                
                var clientUser = new User()
                {
                    UserName = "ziadMohammedClient",
                    Email = "ziadmohammed@gmail.com",
                    PhoneNumber = "01224309198",
                    EmailConfirmed = true
                };
                await _userManager.CreateAsync(clientUser, "P@ssw0rd123Pass");
                await _userManager.AddToRoleAsync(clientUser, "Client");
            }
        }
    }
}

//client :
////eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiJlY2E4NjhmNy1kNzFiLTQ3ZGUtOWRkNi1hNjAyMTNiZmIzOWUiLCJlbWFpbCI6InppYWRtb2hhbW1lZEBnbWFpbC5jb20iLCJnaXZlbl9uYW1lIjoiemlhZE1vaGFtbWVkQ2xpZW50Iiwicm9sZSI6IkNsaWVudCIsIm5iZiI6MTc2MDM4NjE1MiwiZXhwIjoxNzYyOTc4MTUyLCJpYXQiOjE3NjAzODYxNTIsImlzcyI6ImxhdyIsImF1ZCI6ImxhdyJ9.SrR1TtIj3zbQHa5dYN_A5HsmDm0sk6OuUHYaECUaJOc

// lawyer:
////eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiJmMjQzM2E2Yy1mNzUwLTRjMzgtYTRhMy02YTQ1MTg3OGU5Y2IiLCJlbWFpbCI6ImFobWVkeW91c3NlZkBnbWFpbC5jb20iLCJnaXZlbl9uYW1lIjoiYWhtZWRZb3Vzc2VmTGF3eWVyIiwicm9sZSI6Ikxhd3llciIsIm5iZiI6MTc2MDM5MDY4NSwiZXhwIjoxNzYyOTgyNjg1LCJpYXQiOjE3NjAzOTA2ODUsImlzcyI6ImxhdyIsImF1ZCI6ImxhdyJ9.4gJXfN7B8YQgsVmCztDu6jmrJAkkbyZGzkLud5VH1TM