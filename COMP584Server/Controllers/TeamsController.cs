using COMP584Server.Data.DTO;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using worldmodel;


namespace COMP584Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamsController : ControllerBase
    {
        private readonly Comp584Context _context;

        public TeamsController(Comp584Context context)
        {
            _context = context;
        }

        // GET: api/Teams
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Team>>> GetTeams()
        {
            return await _context.Teams.ToListAsync();
        }

        // GET: api/Teams/mine
        //returns only the logged in user's teams
        [HttpGet("mine")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TeamDto>>> GetMyTeams()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("Missing user id claim.");

            var teams = await _context.TeamMembers
                .Where(tm => tm.UserId == userId)
                .Select(tm => new TeamDto
                {
                    TeamId = tm.TeamId,
                    Name = tm.Team!.Name,
                    MyRole = tm.RoleInTeam
                })
                .ToListAsync();

            return Ok(teams);
        }


        // GET: api/Teams/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Team>> GetTeam(int id)
        {
            var team = await _context.Teams.FindAsync(id);

            if (team == null)
                return NotFound();

            return team;
        }

        // POST: api/Teams
        [HttpPost]
        public async Task<ActionResult<Team>> PostTeam(Team team)
        {
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTeam), new { id = team.TeamId }, team);
        }

        // DELETE: api/Teams/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeam(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team == null)
                return NotFound();

            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
