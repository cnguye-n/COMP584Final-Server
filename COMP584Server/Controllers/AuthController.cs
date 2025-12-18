using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using COMP584Server.Data.DTO;
using worldmodel;

// Provides endpoints for authenticating users and generating JWT tokens for secured API access

namespace COMP584Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<WorldModelUser> _userManager;
        private readonly JwtHandler _jwtHandler;

        public AuthController(
            UserManager<WorldModelUser> userManager,
            JwtHandler jwtHandler)
        {
            _userManager = userManager;
            _jwtHandler = jwtHandler;
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.UserName);

            if (user == null)
                return Unauthorized("Invalid username or password");

            if (!await _userManager.CheckPasswordAsync(user, request.Password))
                return Unauthorized("Invalid username or password");

            JwtSecurityToken token = await _jwtHandler.GenerateTokenAsync(user);

            return Ok(new LoginResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo
            });
        }
    }
}
