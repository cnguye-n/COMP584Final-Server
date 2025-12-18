using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using worldmodel;

namespace COMP584Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeedController(
        Comp584Context context,
        IHostEnvironment environment,
        RoleManager<IdentityRole> roleManager,
        UserManager<WorldModelUser> userManager,
        IConfiguration configuration
    ) : ControllerBase
    {
        private string SeedPath(string fileName)
            => Path.Combine(environment.ContentRootPath, "Data", "Seed", fileName);

        // ----------------------------
        // POST: api/Seed/Users
        // Reads Data/Seed/users.csv
        // ----------------------------
        [HttpPost("Users")]
        public async Task<ActionResult> PostUsers()
        {
            // Make sure roles exist
            string[] roles = ["admin", "registereduser"];
            foreach (var r in roles)
            {
                if (!await roleManager.RoleExistsAsync(r))
                    await roleManager.CreateAsync(new IdentityRole(r));
            }

            var path = SeedPath("users.csv");
            if (!System.IO.File.Exists(path))
                return NotFound($"Missing seed file: {path}");

            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                IgnoreBlankLines = true,
            };

            int created = 0, skipped = 0;

            using var reader = new StreamReader(path);
            using var csv = new CsvReader(reader, csvConfig);

            var records = csv.GetRecords<UserSeedRow>().ToList();

            foreach (var row in records)
            {
                // skip if user exists
                var existing = await userManager.FindByNameAsync(row.UserName);
                if (existing != null)
                {
                    skipped++;
                    continue;
                }

                var user = new WorldModelUser
                {
                    UserName = row.UserName,
                    Email = row.Email,
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                // choose password
                // if appsettings has DefaultPasswords:admin/user use those,
                // otherwise fallback to Password1!
                var password =
                    row.Role.Equals("admin", StringComparison.OrdinalIgnoreCase)
                        ? configuration["DefaultPasswords:admin"] ?? "Password1!"
                        : configuration["DefaultPasswords:user"] ?? "Password1!";

                var result = await userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                    return BadRequest(result.Errors);

                // assign role
                var role = row.Role.ToLower();
                if (await roleManager.RoleExistsAsync(role))
                    await userManager.AddToRoleAsync(user, role);

                created++;
            }

            return Ok(new { created, skipped });
        }

        // ----------------------------
        // POST: api/Seed/Teams
        // Reads Data/Seed/teams.csv
        // Header: Name
        // ----------------------------
        [HttpPost("Teams")]
        public async Task<ActionResult> PostTeams()
        {
            var path = SeedPath("teams.csv");
            if (!System.IO.File.Exists(path))
                return NotFound($"Missing seed file: {path}");

            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                IgnoreBlankLines = true,
            };

            int created = 0, skipped = 0;

            using var reader = new StreamReader(path);
            using var csv = new CsvReader(reader, csvConfig);

            var records = csv.GetRecords<TeamSeedRow>().ToList();

            foreach (var row in records)
            {
                // if team exists by name, skip
                var exists = await context.Teams.AnyAsync(t => t.Name == row.Name);
                if (exists)
                {
                    skipped++;
                    continue;
                }

                context.Teams.Add(new Team { Name = row.Name });
                created++;
            }

            await context.SaveChangesAsync();
            return Ok(new { created, skipped });
        }

        // ----------------------------
        // POST: api/Seed/TeamMembers
        // Reads Data/Seed/teamMembers.csv
        // Header: TeamName,UserName,RoleInTeam
        // ----------------------------
        [HttpPost("TeamMembers")]
        public async Task<ActionResult> PostTeamMembers()
        {
            var path = SeedPath("teamMembers.csv");
            if (!System.IO.File.Exists(path))
                return NotFound($"Missing seed file: {path}");

            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
                IgnoreBlankLines = true,
            };

            int created = 0, skipped = 0, missingUsers = 0, missingTeams = 0;

            using var reader = new StreamReader(path);
            using var csv = new CsvReader(reader, csvConfig);

            var records = csv.GetRecords<TeamMemberSeedRow>().ToList();

            foreach (var row in records)
            {
                var team = await context.Teams.FirstOrDefaultAsync(t => t.Name == row.TeamName);
                if (team == null) { missingTeams++; continue; }

                var user = await userManager.FindByNameAsync(row.UserName);
                if (user == null) { missingUsers++; continue; }

                // prevent duplicate TeamId+UserId (since you created unique index)
                bool exists = await context.TeamMembers.AnyAsync(tm =>
                    tm.TeamId == team.TeamId && tm.UserId == user.Id);

                if (exists)
                {
                    skipped++;
                    continue;
                }

                context.TeamMembers.Add(new TeamMember
                {
                    TeamId = team.TeamId,
                    UserId = user.Id,
                    RoleInTeam = row.RoleInTeam
                });

                created++;
            }

            await context.SaveChangesAsync();

            return Ok(new { created, skipped, missingUsers, missingTeams });
        }

        // ----------------------------
        // CSV row models (internal)
        // ----------------------------
        private class UserSeedRow
        {
            public string UserName { get; set; } = "";
            public string Email { get; set; } = "";
            public string Role { get; set; } = "";
        }

        private class TeamSeedRow
        {
            public string Name { get; set; } = "";
        }

        private class TeamMemberSeedRow
        {
            public string TeamName { get; set; } = "";
            public string UserName { get; set; } = "";
            public string RoleInTeam { get; set; } = "";
        }
    }
}
