using Microsoft.AspNetCore.Mvc;
using Financial.Models;
using Financial.Entities;
using Microsoft.AspNetCore.Identity;
using Financial.DAL;
using Microsoft.EntityFrameworkCore;
using Going.Plaid.Entity;

namespace Financial.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ReminderController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly FinancialContext db;

        public ReminderController(UserManager<AppUser> userManager, FinancialContext context)
        {
            _userManager = userManager;
            db = context;
        }

        [HttpPost("all")]
        public async Task<IActionResult> GetAll(ItemIdRequest model)
        {
            var userId = HttpContext.Items["UserId"] as string;
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null)
            {
                return BadRequest(new { message = "An error occoured." });
            }

            var accounts = new List<Entities.Account>();
            if (model.item_id == "all")
            {
                var items = db.Items.Where(i => i.UserId == userId);
                foreach (var item in items)
                {
                    accounts.AddRange(db.Accounts.Where(a => a.ItemId == item.Id));
                }
            }
            else
            {
                accounts.AddRange(db.Accounts.Where(a => a.ItemId == model.item_id));
            }
            var accs = new List<string>();
            foreach (var account in accounts)
            {
                accs.Add(account.Id!);
            }
            var overdues = model.item_id == "all" ? db.Generateds.Where(t => accs.Contains(t.AccountId!)
                && t.TransactionId == null && t.Date < DateTime.Now).OrderByDescending(t => t.Date) :
                 db.Generateds.Where(t => (accs.Contains(t.AccountId!) || t.Type == ScheduleType.Transfer
                        && accs.Contains(t.TransferAccId!))
                && t.TransactionId == null && t.Date < DateTime.Now).OrderByDescending(t => t.Date);
            var _overdues = new List<object>();
            foreach (var generated in overdues)
            {
                var acc = db.Accounts.Find(generated.AccountId);
                if (acc != null)
                {
                    _overdues.Add(new
                    {
                        description = generated.Description,
                        payee = generated.Payee,
                        amount = generated.Amount,
                        currency = acc.CurrencyCode,
                        type = (int)generated.Type!,
                        mode = (int)generated.Mode!,
                        date = generated.Date,
                        account = acc.Name,
                        transfer_acc_id = generated.TransferAccId
                    });
                }
            }
            var upcoming = model.item_id == "all" ? db.Generateds.Where(t => accs.Contains(t.AccountId!)
                && t.TransactionId == null && t.Date > DateTime.Now && t.Date < DateTime.Now.AddDays(7)).OrderBy(t => t.Date) :
                 db.Generateds.Where(t => (accs.Contains(t.AccountId!) || t.Type == ScheduleType.Transfer
                        && accs.Contains(t.TransferAccId!))
                && t.TransactionId == null && t.Date > DateTime.Now && t.Date < DateTime.Now.AddDays(7)).OrderBy(t => t.Date);
            var _upcoming = new List<object>();
            foreach (var generated in upcoming)
            {
                var acc = db.Accounts.Find(generated.AccountId);
                if (acc != null)
                {
                    _upcoming.Add(new
                    {
                        description = generated.Description,
                        payee = generated.Payee,
                        amount = generated.Amount,
                        currency = acc.CurrencyCode,
                        type = (int)generated.Type!,
                        mode = (int)generated.Mode!,
                        date = generated.Date,
                        account = acc.Name,
                        transfer_acc_id = generated.TransferAccId
                    });
                }
            }
            return Ok(new
            {
                overdues = _overdues,
                upcoming = _upcoming,
            });
        }
    }
}
