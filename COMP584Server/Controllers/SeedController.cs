using COMP584Server.Data;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using worldmodel;

namespace COMP584Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeedController(Comp584Context context, IHostEnvironment environment,
        RoleManager<IdentityRole> roleManager, UserManager<WorldModelUser> userManager,
        IConfiguration configuration
        ) : ControllerBase
    {
        //string _path = Path.Combine(environment.ContentRootPath, "Data/worldcities.csv");

       



        [HttpPost("Users")]
        public async Task<ActionResult> PostUsers()
        {
            string administrator = "admin";
            string registeredUser = "registereduser";

            if (!await roleManager.RoleExistsAsync(administrator))

            {
                await roleManager.CreateAsync(new IdentityRole(administrator));
            }

            if(!await roleManager.RoleExistsAsync(registeredUser))
            {
                await roleManager.CreateAsync(new IdentityRole(registeredUser));
            }
            WorldModelUser adminUser = new()
            {
                UserName = "admin",
                Email = "123456@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            await userManager.CreateAsync(adminUser, configuration["DefaultPasswords:admin"]!);
            await userManager.AddToRoleAsync(adminUser, administrator);

            WorldModelUser regularUser = new()

            {
                UserName = "user",
                Email = "user1@gmail.com",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString()
            };
         
            await userManager.CreateAsync(regularUser, configuration["DefaultPasswords:user"]!);
            await userManager.AddToRoleAsync(regularUser, registeredUser);

            return Ok();

        }
        
        
        }
    }

