using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using worldmodel;

namespace COMP584Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly Comp584Context _context;
        private readonly UserManager<WorldModelUser> _userManager;

        public HomeController(Comp584Context context, UserManager<WorldModelUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/Home/summary
        // Public: no login required
        [HttpGet("summary")]
        public async Task<ActionResult<HomeSummaryDto>> GetSummary()
        {
            // Leader definition: RoleInTeam == "Owner"
            // (Your TeamMember comments: "Owner" / "Admin" / "Member")  :contentReference[oaicite:0]{index=0}
            const string leaderRole = "Owner";

            var numberOfTeams = await _context.Teams.CountAsync(); // :contentReference[oaicite:1]{index=1}
            var numberOfUsers = await _userManager.Users.CountAsync();

            // distinct leaders (a user may lead multiple teams)
            var numberOfLeaders = await _context.TeamMembers
                .Where(tm => tm.RoleInTeam == leaderRole)
                .Select(tm => tm.UserId)
                .Distinct()
                .CountAsync();

            var teams = await _context.Teams
                .OrderBy(t => t.Name)
                .Select(t => t.Name)
                .ToListAsync();

            var teamLeaders = await _context.TeamMembers
                .Where(tm => tm.RoleInTeam == leaderRole)
                .Select(tm => new TeamLeaderDto
                {
                    TeamName = tm.Team!.Name,
                    LeaderUserName = tm.User!.UserName!
                })
                .OrderBy(x => x.TeamName)
                .ThenBy(x => x.LeaderUserName)
                .ToListAsync();

            return Ok(new HomeSummaryDto
            {
                NumberOfTeams = numberOfTeams,
                NumberOfUsers = numberOfUsers,
                NumberOfLeaders = numberOfLeaders,
                Teams = teams,
                TeamLeaders = teamLeaders
            });
        }
    }

    public class HomeSummaryDto
    {
        public int NumberOfTeams { get; set; }
        public int NumberOfUsers { get; set; }
        public int NumberOfLeaders { get; set; }
        public List<string> Teams { get; set; } = new();
        public List<TeamLeaderDto> TeamLeaders { get; set; } = new();
    }

    public class TeamLeaderDto
    {
        public string TeamName { get; set; } = "";
        public string LeaderUserName { get; set; } = "";
    }
}
