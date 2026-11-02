using HomeNodeAPI.DTOs.Auth;
using HomeNodeAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HomeNodeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDTO>> Register(RegisterUserDTO registerUserDTO)
        {
            var userExists = await _userManager.FindByEmailAsync(registerUserDTO.Email);
            if (userExists != null)
                return BadRequest(new AuthResponseDTO(false, null, "User already exists"));

            var user = new ApplicationUser
            {
                UserName = registerUserDTO.Email,
                Email = registerUserDTO.Email
            };

            var result = await _userManager.CreateAsync(user, registerUserDTO.Password);

            if (!result.Succeeded)
                return BadRequest(new AuthResponseDTO(false, null, string.Join(", ", result.Errors.Select(e => e.Description))));

            var roleResult = await _userManager.AddToRoleAsync(user, "User");

            if (!roleResult.Succeeded)
                return BadRequest(new AuthResponseDTO(false, null, "User created but failed to add role"));

            return Ok(new AuthResponseDTO(true, null, "User created successfully"));
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDTO>> Login(LoginUserDTO loginUserDTO)
        {
            var userToLogin = await _userManager.FindByEmailAsync(loginUserDTO.Email);

            if (userToLogin == null || !await _userManager.CheckPasswordAsync(userToLogin, loginUserDTO.Password))
                return BadRequest(new AuthResponseDTO(false, null, "Invalid credentials"));

            var token = await CreateJwtToken(userToLogin);

            return Ok(new AuthResponseDTO(true, token, "Login successful"));
        }

        private async Task<string> CreateJwtToken(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim("role", role));
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration.GetValue<string>("AppSettings:Token")!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: _configuration.GetValue<string>("AppSettings:Issuer"),
                audience: _configuration.GetValue<string>("AppSettings:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        [Authorize]
        [HttpGet("AuthorizeOnly")]
        public IActionResult AuthorizeAccessOnly()
        {
            return Ok("You are authorized");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("AdminAuthorizeOnly")]
        public IActionResult AuthorizeAdminAccessOnly()
        {
            return Ok("You are admin");
        }
    }
}
