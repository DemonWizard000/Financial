using Microsoft.AspNetCore.Mvc;
using Financial.Models;
using Financial.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Financial.DAL;
using Going.Plaid.Entity;

namespace Financial.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _config;
        private readonly FinancialContext db;

        public UserController(UserManager<AppUser> userManager, IConfiguration config, FinancialContext context)
        {
            _userManager = userManager;
            _config = config;
            db = context;
        }

        private async Task<string> GetRole(AppUser user)
        {
            IList<string> result = await _userManager.GetRolesAsync(user);
            return result.Count > 0 ? result[0] : "";
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = HttpContext.Items["UserId"] as string;
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null)
            {
                return BadRequest(new { message = "An error occoured." });
            }
            string role = await GetRole(user);
            return Ok(new
            {
                username = user.UserName,
                email = user.Email,
                name = user.Name,
                role
            });
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var userId = HttpContext.Items["UserId"] as string;
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null || (await _userManager.IsInRoleAsync(user, "Admin")) == false)
            {
                return BadRequest(new { message = "An error occoured." });
            }
            var users = db.Users.ToList();
            var result = new List<object>();
            foreach (var u in users)
            {
                result.Add(new
                {
                    id = u.Id,
                    name = u.Name,
                    email = u.Email,
                    username = u.UserName,
                    join_date = u.JoinDate,
                    role = await GetRole(u)
                });
            }
            return Ok(result);
        }

        [HttpPost("change-role")]
        public async Task<IActionResult> ChangeRole(ChangeRoleRequest model)
        {
            var userId = HttpContext.Items["UserId"] as string;
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null || (await _userManager.IsInRoleAsync(user, "Admin")) == false)
            {
                return BadRequest(new { message = "An error occoured." });
            }
            user = await _userManager.FindByIdAsync(model.user_id);
            if (user == null)
            {
                return BadRequest(new {message = "User not found."});
            }
            IList<string> roles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, roles);
            await _userManager.AddToRoleAsync(user, model.role);
            return Ok();
        }

        [HttpPost("delete-user")]
        public async Task<IActionResult> DeleteUser(UserIdRequest model)
        {
            var userId = HttpContext.Items["UserId"] as string;
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null || (await _userManager.IsInRoleAsync(user, "Admin")) == false)
            {
                return BadRequest(new { message = "An error occoured." });
            }
            user = await _userManager.FindByIdAsync(model.user_id);
            if (user == null)
            {
                return BadRequest(new { message = "User not found." });
            }
            await _userManager.DeleteAsync(user);
            return Ok();
        }

        [HttpGet("items")]
        public async Task<IActionResult> GetItems()
        {
            var userId = HttpContext.Items["UserId"] as string;
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null)
            {
                return BadRequest(new { message = "An error occoured." });
            }

            var items = db.Items.Where(i => i.UserId == userId);
            if (items == null)
            {
                return Ok(new {items = new List<string>()});
            }
            var _items = new List<object>();
            foreach (var item in items)
            {
                var accs = new List<string?>();
                var accIds = new List<string?>();
                var accCurrency = new List<string?>();
                var accounts = db.Accounts.Where(a => a.ItemId == item.Id);
                if (accounts != null )
                {
                    foreach (var account in accounts) {
                        accs.Add(account.Name);
                        accIds.Add(account.Id);
                        accCurrency.Add(account.CurrencyCode);
                    }
                }
                _items.Add(new
                {
                    item_id  = item.Id,
                    institution = item.InstitutionName,
                    accounts = accs,
                    acc_ids = accIds,
                    acc_currency = accCurrency,
                });
            }
            return Ok(new
            {
                items = _items
            });
        }

        [HttpPost("update")]
        public async Task<IActionResult> Update(UserUpdateRequest model)
        {
            var userId = HttpContext.Items["UserId"] as string;
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null)
            {
                return BadRequest(new { message = "An error occoured." });
            }

            if (user.Email != model.email)
            {
                var existing = await _userManager.FindByEmailAsync(model.email);

                if (existing != null)
                    return BadRequest(new { field = "email", message = "Email already exists." });
                user.Email = model.email;
            }

            if (user.UserName != model.username)
            {
                var existing = await _userManager.FindByNameAsync(model.username);

                if (existing != null)
                    return BadRequest(new { field = "username", message = "Username already exists." });
                user.UserName = model.username;
            }

            user.Name = model.name;

            IdentityResult result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(new { field = "error", message = result.Errors.First().Description });

            return Ok();
        }


        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest model)
        {
            var userId = HttpContext.Items["UserId"] as string;
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null)
            {
                return BadRequest(new { message = "An error occoured." });
            }

            var result = await _userManager.ChangePasswordAsync(user, model.current, model.update);

            if (!result.Succeeded)
            {
                if (result.Errors.First().Code == "PasswordMismatch")
                {
                    return BadRequest(new { field = "old-password", message = result.Errors.First().Description });
                }
                return BadRequest(new { field = "new-password", message = result.Errors.First().Description });
            }

            return Ok();
        }

        [HttpGet("categorize")]
        public async Task<IActionResult> Categorize()
        {
            var userId = HttpContext.Items["UserId"] as string;
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null)
            {
                return BadRequest(new { message = "An error occoured." });
            }

            return Ok(new {
                amount = user.CategorizeAmountPercent,
                date = user.CategorizeDateRange
            });
        }

        [HttpPost("change-categorize")]
        public async Task<IActionResult> ChangeCategorize(ChangeCategorizeRequest model)
        {
            var userId = HttpContext.Items["UserId"] as string;
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null)
            {
                return BadRequest(new { message = "An error occoured." });
            }

            user.CategorizeAmountPercent = model.amount;
            user.CategorizeDateRange = model.date;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(new { message = "An error occured" });
            }
            return Ok();
        }
    }
}
