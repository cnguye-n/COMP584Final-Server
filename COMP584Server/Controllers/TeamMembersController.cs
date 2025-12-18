using COMP584Server.Data.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
