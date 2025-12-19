using COMP584Server.Data.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using worldmodel;

namespace COMP584Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamMembersController : ControllerBase
    {
        private readonly Comp584Context _context;

        public TeamMembersController(Comp584Context context)
        {
            _context = context;
        }

        // GET: api/TeamMembers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TeamMemberDto>>> GetTeamMembers()
        {
            var result = await _context.TeamMembers
                .Select(tm => new TeamMemberDto
                {
                    TeamMemberId = tm.TeamMemberId,
                    TeamId = tm.TeamId,
                    TeamName = tm.Team!.Name,
                    UserId = tm.UserId,
                    UserName = tm.User!.UserName!,
                    RoleInTeam = tm.RoleInTeam
                })
                .ToListAsync();

            return Ok(result);
        }

        // GET: api/TeamMembers/mine
        // Returns team members for the current user's "current team".
        // If teamId is provided, returns members for that team (only if the user is in it).
        [HttpGet("mine")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TeamMemberDto>>> GetMyTeamMembers([FromQuery] int? teamId = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("Missing user id claim.");

            // Teams the user belongs to
            var myTeamIds = await _context.TeamMembers
                .Where(tm => tm.UserId == userId)
                .Select(tm => tm.TeamId)
                .Distinct()
                .ToListAsync();

            if (myTeamIds.Count == 0)
                return Ok(new List<TeamMemberDto>());

            // Choose team: either requested teamId or default to first
            int chosenTeamId = teamId ?? myTeamIds[0];

            // Security: user must belong to that team
            if (!myTeamIds.Contains(chosenTeamId))
                return Forbid("You are not a member of that team.");

            var result = await _context.TeamMembers
                .Where(tm => tm.TeamId == chosenTeamId)
                .Select(tm => new TeamMemberDto
                {
                    TeamMemberId = tm.TeamMemberId,
                    TeamId = tm.TeamId,
                    TeamName = tm.Team!.Name,
                    UserId = tm.UserId,
                    UserName = tm.User!.UserName!,
                    RoleInTeam = tm.RoleInTeam
                })
                .ToListAsync();

            return Ok(result);
        }


        // GET: api/TeamMembers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TeamMemberDto>> GetTeamMember(int id)
        {
            var tm = await _context.TeamMembers
                .Where(t => t.TeamMemberId == id)
                .Select(tm => new TeamMemberDto
                {
                    TeamMemberId = tm.TeamMemberId,
                    TeamId = tm.TeamId,
                    TeamName = tm.Team!.Name,
                    UserId = tm.UserId,
                    UserName = tm.User!.UserName!,
                    RoleInTeam = tm.RoleInTeam
                })
                .FirstOrDefaultAsync();

            if (tm == null)
                return NotFound();

            return Ok(tm);
        }

        // POST: api/TeamMembers
        [HttpPost]
        public async Task<ActionResult<TeamMember>> PostTeamMember(TeamMember teamMember)
        {
            bool exists = await _context.TeamMembers.AnyAsync(tm =>
                tm.TeamId == teamMember.TeamId && tm.UserId == teamMember.UserId);

            if (exists)
                return Conflict("That user is already a member of this team.");

            _context.TeamMembers.Add(teamMember);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTeamMember),
                new { id = teamMember.TeamMemberId }, teamMember);
        }


        // DELETE: api/TeamMembers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeamMember(int id)
        {
            var teamMember = await _context.TeamMembers.FindAsync(id);
            if (teamMember == null)
                return NotFound();

            _context.TeamMembers.Remove(teamMember);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
