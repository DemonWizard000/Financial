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
    public class ScheduleController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly FinancialContext db;

        public ScheduleController(UserManager<AppUser> userManager, FinancialContext context)
        {
            _userManager = userManager;
            db = context;
        }

        private void generateOne(Schedule schedule, DateTime date)
        {
            var generated = new Generated
            {
                Description = schedule.Description,
                Payee = schedule.Payee,
                Amount = schedule.Amount,
                Type = schedule.Type,
                Mode = schedule.Mode,
                Date = date,
                ScheduleId = schedule.Id,
                AccountId = schedule.AccountId,
                TransferAccId = schedule.Type == ScheduleType.Transfer ? schedule.TransferAccId : null
            };
            db.Generateds.Add(generated);
        }

        private bool generateFromSchedule(Schedule schedule)
        {
            if (schedule == null)
            {
                return false;
            }
            DateTime date = (DateTime)schedule.StartDate;
            switch (schedule.Mode)
            {
                case RecurrencyMode.OneDay:
                    generateOne(schedule, date);
                    return true;
                case RecurrencyMode.Daily:
                    for (var i = 0; i < 365; i++)
                    {
                        generateOne(schedule, date.AddDays(i));
                    }
                    return true;
                case RecurrencyMode.Weekly:
                    for (var i = 0; i < 365; i += 7)
                    {
                        generateOne(schedule, date.AddDays(i));
                    }
                    return true;
                case RecurrencyMode.Monthly:
                    for (var i = 0; i < 365; i += 30)
                    {
                        generateOne(schedule, date.AddDays(i));
                    }
                    return true;
            }
            return true;
        }

        [HttpPost("all")]
        public async Task<IActionResult> GetSchedules(ItemIdRequest model)
        {
            var userId = HttpContext.Items["UserId"] as string;
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null)
            {
                return BadRequest(new { message = "An error occoured." });
            }

            var _schedules = new List<object>();
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
            var schedules = model.item_id == "all" ? db.Schedules.Where(t => accs.Contains(t.AccountId!)).OrderBy(s => s.StartDate) :
                db.Schedules.Where(t => accs.Contains(t.AccountId!) || (t.Type == ScheduleType.Transfer
                        && accs.Contains(t.TransferAccId!))).OrderBy(s => s.StartDate);
            foreach (var schedule in schedules)
            {
                var acc = db.Accounts.Find(schedule.AccountId);
                if (acc != null)
                {
                    _schedules.Add(new
                    {
                        id = schedule.Id!,
                        description = schedule.Description,
                        payee = schedule.Payee,
                        amount = schedule.Amount,
                        currency = acc.CurrencyCode,
                        type = (int)schedule.Type!,
                        mode = (int)schedule.Mode!,
                        account = acc.Name,
                        start_date = schedule.StartDate,
                        account_id = schedule.AccountId,
                        TransferAccId = schedule.Type == ScheduleType.Transfer ? schedule.TransferAccId : null
                    });
                }
            }
            return Ok(_schedules);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddNewSchedule(ScheduleRequest model)
        {
            var userId = HttpContext.Items["UserId"] as string;
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null)
            {
                return BadRequest(new { message = "An error occoured." });
            }
            var schedule = new Schedule
            {
                Description = model.description,
                Payee = model.payee,
                Amount = model.amount,
                Type = model.type,
                Mode = model.mode,
                StartDate = model.start_date,
                AccountId = model.account_id,
                TransferAccId = model.type == ScheduleType.Transfer ? model.transfer_acc_id : null
            };

            db.Schedules.Add(schedule);
            db.SaveChanges();
            generateFromSchedule(schedule);
            db.SaveChanges();
            return Ok();
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateSchedule(ScheduleRequest model)
        {
            var userId = HttpContext.Items["UserId"] as string;
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null || model.schedule_id == null)
            {
                return BadRequest(new { message = "An error occoured." });
            }
            var schedule = db.Schedules.Find(model.schedule_id);
            if (schedule == null)
            {
                return BadRequest(new { message = "Schedule not found." });
            }

            schedule.Description = model.description;
            schedule.Payee = model.payee;
            schedule.Amount = model.amount;
            schedule.Type = model.type;
            schedule.Mode = model.mode;
            schedule.StartDate = model.start_date;
            schedule.AccountId = model.account_id;
            schedule.TransferAccId = model.type == ScheduleType.Transfer ? model.transfer_acc_id : null;

            db.Generateds.RemoveRange(db.Generateds.Where(g => g.ScheduleId == schedule.Id));
            db.Schedules.Update(schedule);
            generateFromSchedule(schedule);
            db.SaveChanges();
            return Ok();
        }

        [HttpPost("delete")]
        public async Task<IActionResult> DeleteSchedule(ScheduleIdRequest model)
        {
            var userId = HttpContext.Items["UserId"] as string;
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null || model.schedule_id == null)
            {
                return BadRequest(new { message = "An error occoured." });
            }
            var schedule = db.Schedules.Find(model.schedule_id);
            if (schedule == null)
            {
                return BadRequest(new { message = "Schedule not found." });
            }

            db.Generateds.RemoveRange(db.Generateds.Where(g => g.ScheduleId == schedule.Id));
            db.Schedules.Remove(schedule);
            db.SaveChanges();
            return Ok();
        }

        /*[HttpPost("transfer_accounts")]
        public async Task<IActionResult> GetTransferAccount(AccountIdRequest model)
        {
            var userId = HttpContext.Items["UserId"] as string;
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null || model.account_id == null)
            {
                return BadRequest(new { message = "An error occoured." });
            }
            var accounts = db.Accounts.Where(a => a.Id != model.account_id);
            var names = new List<string>();
            var ids = new List<string>();
            foreach (var account in accounts)
            {
                ids.Add(account.Id!);
                names.Add(account.Name!);
            }
            return Ok(new {
                ids, names
            });
        }*/

        [HttpPost("recategorize")]
        public async Task<IActionResult> Recategorize()
        {
            var userId = HttpContext.Items["UserId"] as string;
            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null)
            {
                return BadRequest(new { message = "An error occoured." });
            }
            decimal categorizeAmount = user.CategorizeAmountPercent;
            int categorizeDate = (int)user.CategorizeDateRange!;
            DateTime firstDate = (DateTime)db.Schedules.Min(s => s.StartDate)!;
            firstDate = firstDate.AddDays(-categorizeDate);
            var transactions = db.Transactions.Where(t => t.Date >= firstDate);
            foreach (var transaction in transactions)
            {
                DateTime minDate = transaction.Date.AddDays(-categorizeDate);
                DateTime maxDate = transaction.Date.AddDays(categorizeDate);
                Generated? generated = db.Generateds
                    .Where(s => s.AccountId == transaction.AccountId && s.Date >= minDate && s.Date <= maxDate
                        && (Math.Abs(s.Amount - transaction.Amount) <= s.Amount * categorizeAmount / 100))
                    .OrderBy(s => s.Date).FirstOrDefault();
                if (generated != null) {
                    transaction.Consolidated = true;
                    generated.TransactionId = transaction.Id;
                    db.Generateds.Update(generated);
                } else
                {
                    transaction.Consolidated = false;
                }
                db.Transactions.Update(transaction);
            }
            db.SaveChanges();
            return Ok();
        }
    }
}
