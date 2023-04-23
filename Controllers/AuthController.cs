using Microsoft.AspNetCore.Mvc;
using Financial.Models;
using Financial.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Financial.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;

        public AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<IdentityRole> roleManager, IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _config = config;
        }

        private string generateJwtToken(AppUser user)
        {
            // generate token that is valid for 7 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["AppSettings:Secret"]!);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        [HttpPost("sign-in")]
        public async Task<IActionResult> SignIn(AuthSignInRequest model)
        {
            var user = await _userManager.FindByEmailAsync(model.email);

            if (user == null)
                return BadRequest(new { field = "email", message = "Email doesn't exist." });

            var result = await _signInManager.PasswordSignInAsync(user, model.password, model.rememberMe, true);
            if (!result.Succeeded)
                return BadRequest(new { field = "password", message = "Password is incorrect." });

            var token = generateJwtToken(user);

            return Ok(new {token = token});
        }

        [HttpPost("sign-up")]
        public async Task<IActionResult> SignUp(AuthSignUpRequest model)
        {
            var user = await _userManager.FindByEmailAsync(model.email);

            if (user != null)
                return BadRequest(new { field = "email", message = "Email already exists." });

            user = await _userManager.FindByNameAsync(model.username);

            if (user != null)
                return BadRequest(new { field = "username", message = "Username already exists." });

            user = new AppUser
            {
                Email = model.email,
                UserName = model.username,
                Name = model.name,
                JoinDate = DateTime.Now,
                CategorizeAmountPercent = 0,
                CategorizeDateRange = 15
            };
            IdentityResult result = await _userManager.CreateAsync(user, model.password);
            if (!result.Succeeded)
                return BadRequest(new {field = "password", message = result.Errors.First().Description });

            result = await _userManager.AddToRoleAsync(user, "User");
            if (!result.Succeeded)
                return BadRequest(new { field = "role", message = result.Errors.First().Description });

            return Ok();
        }

        [Authorize]
        [HttpGet("role")]
        public async Task<IActionResult> Role()
        {
            var userId = HttpContext.Items["UserId"] as string;
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null)
            {
                return BadRequest(new { message = "An error occoured." });
            }
            IList<string> result = await _userManager.GetRolesAsync(user);
            return Ok(result.Count > 0 ? result[0]: "");
        }

        [HttpGet("logout")]
        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }

        /*
            Add new Role(just for adding role to database) 
        */
        [HttpGet("secret/add-role")]
        public async Task<string> AddRole(string role)
        {
            IdentityRole newRole = new IdentityRole
            {
                Name = role
            };
            IdentityResult result = await _roleManager.CreateAsync(newRole);
            if (result.Succeeded)
                return "Success";
            else
                return "Failed";
        }

        [HttpPost("secret/change-role")]
        public async Task<bool> ChangeRole(string email, string role)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    IList<string> roles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, roles);
                    await _userManager.AddToRoleAsync(user, role);
                    return true;
                }
            }
            return false;
        }
    }
}
