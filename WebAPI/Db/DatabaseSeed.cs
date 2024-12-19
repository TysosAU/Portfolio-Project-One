using Microsoft.EntityFrameworkCore;
using WebAPI.Models;
using System.Linq;
using System;

namespace WebAPI.Db
{
    public class DatabaseSeed
    {
        public static void Seed(DatabaseContext context)
        {
            try
            {
                if (!context.Users.Any())
                {
                    string Password = BCrypt.Net.BCrypt.HashPassword("Password123#11!xcbbvc");

                    var newUser = new User
                    {
                        UserName = "Admin",
                        Password = Password,
                        Role = "Administrator"
                    };

                    context.Users.Add(newUser);
                    context.SaveChanges();
                    Console.WriteLine("User added successfully.");
                }
                else
                {
                    Console.WriteLine("Users table already contains data.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during seeding the Users table: {ex.Message}");
            }

            try
            {
                if (!context.HeroLoadouts.Any())
                {
                    var seededLoadout = new HeroLoadout
                    {
                        Commander = "Commando Ramirez",
                        Perk = "Happy Holidays",
                        HeroOne = "Diecast Jonesy",
                        HeroTwo = "Ted",
                        HeroThree = "Archetype Havoc",
                        HeroFour = "Sergeant Jonesy",
                        HeroFive = "Survivalist Jonesy",
                        GadgetOne = "Adrenaline Rush",
                        GadgetTwo = "Slow Field",
                        CreatedBy = "Admin"
                    };

                    context.HeroLoadouts.Add(seededLoadout);
                    context.SaveChanges();
                    Console.WriteLine("HeroLoadout added successfully.");
                }
                else
                {
                    Console.WriteLine("HeroLoadouts table already contains data.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during seeding the HeroLoadouts table: {ex.Message}");
            }
        }
    }
}
