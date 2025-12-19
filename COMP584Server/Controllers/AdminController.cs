using COMP584Server.Data.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using worldmodel;

namespace COMP584Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<WorldModelUser> _userManager;
        private readonly JwtHandler _jwtHandler;
        private readonly Comp584Context _context;

        public AdminController(
            UserManager<WorldModelUser> userManager,
            JwtHandler jwtHandler,
            Comp584Context context)
        {
            _userManager = userManager;
            _jwtHandler = jwtHandler;
            _context = context;
        }

        // ----------------------------
        // POST: api/Admin
        // Your existing admin login
        // ----------------------------
        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest loginrequest)
        {
            WorldModelUser? worldUser = await _userManager.FindByNameAsync(loginrequest.UserName);
            if (worldUser == null)
                return Unauthorized("Invalid username");

            bool loginStatus = await _userManager.CheckPasswordAsync(worldUser, loginrequest.Password);
            if (!loginStatus)
                return Unauthorized("Invalid password");

            JwtSecurityToken jwtToken = await _jwtHandler.GenerateTokenAsync(worldUser);
            var stringToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);

            return Ok(new LoginResponse
            {
                Success = true,
                Message = "Mom loves me!",
                Token = stringToken
            });
        }

        // ----------------------------
        // GET: api/Admin/users
        // All registered Identity users
        // ----------------------------
        [HttpGet("users")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var rows =
                from u in _userManager.Users
                join tm in _context.TeamMembers on u.Id equals tm.UserId into tmj
                from tm in tmj.DefaultIfEmpty()
                join t in _context.Teams on tm.TeamId equals t.TeamId into tj
                from t in tj.DefaultIfEmpty()
                select new
                {
                    userId = u.Id,
                    userName = u.UserName,
                    email = u.Email,

                    teamId = tm != null ? (int?)tm.TeamId : null,
                    teamName = t != null ? t.Name : null,
                    roleInTeam = tm != null ? tm.RoleInTeam : "None"
                };

            return Ok(await rows.ToListAsync());
        }



        // ----------------------------
        // GET: api/Admin/users-not-in-team
        // Identity users who are not in TeamMembers at all
        // ----------------------------
        [HttpGet("users-not-in-team")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetUsersNotInTeam()
        {
            var memberUserIds = await _context.TeamMembers
                .Select(tm => tm.UserId)
                .Distinct()
                .ToListAsync();

            var users = await _userManager.Users
                .Where(u => !memberUserIds.Contains(u.Id))
                .Select(u => new
                {
                    userId = u.Id,
                    userName = u.UserName,
                    email = u.Email
                })
                .ToListAsync();

            return Ok(users);
        }

        // ----------------------------
        // DTO for add/remove actions
        // ----------------------------
        public class AddToTeamRequest
        {
            public int TeamId { get; set; }
            public string UserId { get; set; } = "";
            public string RoleInTeam { get; set; } = "Member";
        }

        // ----------------------------
        // POST: api/Admin/add-to-team
        // Admin adds a user to a team
        // ----------------------------
        [HttpPost("add-to-team")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AddUserToTeam([FromBody] AddToTeamRequest request)
        {
            if (request.TeamId <= 0 || string.IsNullOrWhiteSpace(request.UserId))
                return BadRequest("TeamId and UserId are required.");

            var team = await _context.Teams.FindAsync(request.TeamId);
            if (team == null)
                return NotFound("Team not found.");

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
                return NotFound("User not found.");

            bool exists = await _context.TeamMembers.AnyAsync(tm =>
                tm.TeamId == request.TeamId && tm.UserId == request.UserId);

            if (exists)
                return Conflict("That user is already a member of this team.");

            _context.TeamMembers.Add(new TeamMember
            {
                TeamId = request.TeamId,
                UserId = request.UserId,
                RoleInTeam = string.IsNullOrWhiteSpace(request.RoleInTeam) ? "Member" : request.RoleInTeam
            });

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        // ----------------------------
        // DELETE: api/Admin/remove-from-team?teamId=1&userId=abc
        // Admin removes a user from a team
        // You cannot remove other admins
        // ----------------------------
        [HttpDelete("remove-from-team")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> RemoveUserFromTeam([FromQuery] int teamId, [FromQuery] string userId)
        {
            if (teamId <= 0 || string.IsNullOrWhiteSpace(userId))
                return BadRequest("teamId and userId are required.");

            var membership = await _context.TeamMembers
                .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == userId);

            if (membership == null)
                return NotFound("Membership not found.");
            
            if ((membership.RoleInTeam ?? "").ToLower() == "owner")
                return BadRequest("Owners cannot be removed from a team.");

            _context.TeamMembers.Remove(membership);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }
    }
}
