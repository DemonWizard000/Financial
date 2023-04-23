using Microsoft.AspNetCore.Mvc;
using Financial.Models;
using Financial.Entities;
using Microsoft.AspNetCore.Identity;
using Financial.DAL;
using Microsoft.EntityFrameworkCore;
using Going.Plaid.Entity;
using System.Linq;

namespace Financial.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class CashflowController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly FinancialContext db;

        public CashflowController(UserManager<AppUser> userManager, FinancialContext context)
        {
            _userManager = userManager;
            db = context;
        }

        [HttpPost("all")]
        public async Task<IActionResult> GetAll(CashflowRequest model)
        {
            var userId = HttpContext.Items["UserId"] as string;
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null)
            {
                return BadRequest(new { message = "An error occoured." });
            }

            var accounts = new List<Entities.Account>();
            decimal balance = 0;
            if (model.item_id == "all")
            {
                var items = db.Items.Where(i => i.UserId == userId);
                foreach (var item in items)
                {
                    var _accs = db.Accounts.Where(a => a.ItemId == item.Id);
                    foreach (var acc in _accs)
                    {
                        accounts.Add(acc);
                        balance += acc.AvailableBalance.HasValue ? (decimal)acc.AvailableBalance : (decimal)acc.CurrentBalance!;
                    }
                }
            }
            else
            {
                var _accs = db.Accounts.Where(a => a.ItemId == model.item_id);
                foreach (var acc in _accs)
                {
                    accounts.Add(acc);
                    balance += acc.AvailableBalance.HasValue ? (decimal)acc.AvailableBalance : (decimal)acc.CurrentBalance!;
                }
            }
            var accs = new List<string>();
            foreach (var account in accounts)
            {
                accs.Add(account.Id!);
            }
            var upcoming = model.item_id == "all" ? db.Generateds.Where(t => accs.Contains(t.AccountId!)
                && t.TransactionId == null && t.Date > DateTime.Now && t.Date < DateTime.Now.AddDays(model.days)).OrderBy(t => t.Date) :
                 db.Generateds.Where(t => (accs.Contains(t.AccountId!) || t.Type == ScheduleType.Transfer
                        && accs.Contains(t.TransferAccId!))
                && t.TransactionId == null && t.Date > DateTime.Now && t.Date < DateTime.Now.AddDays(model.days)).OrderBy(t => t.Date);
            var _upcoming = new List<object>();
            var future = balance;
            foreach (var generated in upcoming)
            {
                var acc = db.Accounts.Find(generated.AccountId);
                if (acc != null)
                {
                    switch (generated.Type)
                    {
                        case ScheduleType.Income:
                            future += generated.Amount;
                            break;
                        case ScheduleType.Expense:
                            future -= generated.Amount;
                            break;
                        case ScheduleType.Transfer:
                            if (model.item_id != "all")
                            {
                                if (accs.Contains(acc.Id!))
                                    future -= generated.Amount;
                                else
                                    future += generated.Amount;
                            }
                            break;
                    }
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
                        transfer_acc_id = generated.TransferAccId,
                        balance = future
                    });
                }
            }
            return Ok(new
            {
                balance,
                future,
                currency = "USD",
                upcoming = _upcoming,
            });
        }
    }
}
